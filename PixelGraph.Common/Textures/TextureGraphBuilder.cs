using Microsoft.Extensions.Logging;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Effects;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureGraphBuilder
    {
        Task ProcessInputGraphAsync(CancellationToken token = default);
        Task ProcessOutputGraphAsync(CancellationToken token = default);
    }

    internal class TextureGraphBuilder : ITextureGraphBuilder
    {
        private readonly ITextureGraphContext context;
        private readonly ITextureGraph graph;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly INamingStructure naming;
        private readonly IImageWriter imageWriter;
        private readonly IEdgeFadeImageEffect edgeFadeEffect;
        private readonly IInventoryTextureGenerator itemGenerator;
        private readonly ITextureRegionEnumerator regions;
        private readonly ILogger logger;
        private string matMetaFileIn;


        public TextureGraphBuilder(
            ILogger<TextureGraphBuilder> logger,
            ITextureGraphContext context,
            ITextureGraph graph,
            IInputReader reader,
            IOutputWriter writer,
            INamingStructure naming,
            IImageWriter imageWriter,
            IEdgeFadeImageEffect edgeFadeEffect,
            IInventoryTextureGenerator itemGenerator,
            ITextureRegionEnumerator regions)
        {
            this.context = context;
            this.graph = graph;
            this.reader = reader;
            this.writer = writer;
            this.naming = naming;
            this.imageWriter = imageWriter;
            this.edgeFadeEffect = edgeFadeEffect;
            this.itemGenerator = itemGenerator;
            this.regions = regions;
            this.logger = logger;
        }

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        public async Task ProcessInputGraphAsync(CancellationToken token = default)
        {
            context.ApplyInputEncoding();

            matMetaFileIn = context.GetMetaInputFilename();
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;
            var sourceTime = reader.GetWriteTime(context.Material.LocalFilename);
            
            if (context.Material.Publish ?? true) {
                if (!IsOutputUpToDate(packWriteTime, sourceTime)) {
                    logger.LogDebug($"Publishing texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
                    await ProcessAllTexturesAsync(true, token);
                }
                else {
                    logger.LogDebug($"Skipping up-to-date texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
                }
            }

            if (context.Material.PublishInventory ?? false) {
                if (!IsInventoryUpToDate(packWriteTime, sourceTime)) {
                    await GenerateInventoryTextureAsync(token);
                }
                else {
                    logger.LogDebug($"Skipping up-to-date item texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
                }
            }
        }

        /// <summary>
        /// Output -> Input; for importing textures
        /// </summary>
        public async Task ProcessOutputGraphAsync(CancellationToken token = default)
        {
            context.ApplyOutputEncoding();

            await ProcessAllTexturesAsync(false, token);
        }

        private async Task ProcessAllTexturesAsync(bool createEmpty, CancellationToken token)
        {
            var allOutputTags = context.OutputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            try {
                await graph.PreBuildNormalTextureAsync(token);
            }
            catch (HeightSourceEmptyException) {}

            foreach (var tag in allOutputTags)
                await graph.MapAsync(tag, createEmpty, null, token);

            context.MaxFrameCount = graph.GetMaxFrameCount();
            
            foreach (var tag in allOutputTags) {
                var tagOutputEncoding = context.OutputEncoding
                    .Where(e => TextureTags.Is(e.Texture, tag)).ToArray();

                if (tagOutputEncoding.Any()) {
                    var hasAlpha = tagOutputEncoding.Any(c => c.Color == ColorChannel.Alpha);
                    var hasColor = tagOutputEncoding.Any(c => c.Color != ColorChannel.Red);

                    if (hasAlpha) {
                        await PublishImageAsync<Rgba32>(tag, ImageChannels.ColorAlpha, createEmpty, token);
                    }
                    else if (hasColor) {
                        await PublishImageAsync<Rgb24>(tag, ImageChannels.Color, createEmpty, token);
                    }
                    else {
                        await PublishImageAsync<L8>(tag, ImageChannels.Gray, createEmpty, token);
                    }
                }

                if (context.IsAnimated)
                    await CopyMetaAsync(tag, token);
            }

            await CopyPropertiesAsync(token);
        }

        private async Task PublishImageAsync<TPixel>(string textureTag, ImageChannels type, bool createEmpty, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var image = await graph.CreateImageAsync<TPixel>(textureTag, createEmpty, token);

            if (image == null) {
                // TODO: log
                logger.LogWarning("No texture sources found for item {DisplayName} texture {textureTag}.", context.Material.DisplayName, textureTag);
                return;
            }

            imageWriter.Format = context.ImageFormat;

            var p = context.Material.LocalPath;
            if (!context.PublishAsGlobal || (context.IsMaterialCtm && !context.Material.UseGlobalMatching)) p = PathEx.Join(p, context.Material.Name);

            var maxFrameCount = graph.GetMaxFrameCount();
            foreach (var part in regions.GetAllPublishRegions(maxFrameCount)) {
                var name = naming.GetOutputTextureName(context.Profile, part.Name, textureTag, context.PublishAsGlobal);
                var destFile = PathEx.Join(p, name);
                var srcWidth = image.Width;
                var srcHeight = image.Height;

                if (srcWidth == 1 && srcHeight == 1) {
                    await imageWriter.WriteAsync(image, destFile, type, token);
                }
                else {
                    var firstFrame = part.Frames.First();
                    var frameCount = part.Frames.Length;
                    var partWidth = (int)(firstFrame.SourceBounds.Width * srcWidth);
                    var partHeight = (int)(firstFrame.SourceBounds.Height * srcHeight * frameCount);
                    using var regionImage = new Image<TPixel>(partWidth, partHeight);

                    var options = new CopyRegionProcessor<TPixel>.Options {
                        SourceImage = image,
                    };

                    var processor = new CopyRegionProcessor<TPixel>(options);

                    foreach (var frame in part.Frames) {
                        options.SourceX = (int) (frame.SourceBounds.X * srcWidth);
                        options.SourceY = (int) (frame.SourceBounds.Y * srcHeight);

                        var outBounds = frame.DestBounds.ScaleTo(partWidth, partHeight);
                        regionImage.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }

                    edgeFadeEffect.Apply(regionImage, textureTag);

                    await imageWriter.WriteAsync(regionImage, destFile, type, token);
                }

                logger.LogInformation("Published texture region {Name} tag {textureTag}.", part.Name, textureTag);
            }
        }

        private async Task GenerateInventoryTextureAsync(CancellationToken token = default)
        {
            var name = naming.GetOutputTextureName(context.Profile, context.Material.Name, TextureTags.Inventory, true);

            var path = context.Material.LocalPath;
            if (!context.PublishAsGlobal || (context.IsMaterialCtm && !context.Material.UseGlobalMatching)) path = PathEx.Join(path, context.Material.Name);
            var destFile = PathEx.Join(path, name);

            // Generate item image
            using var itemImage = await itemGenerator.CreateAsync(graph, token);
            if (itemImage == null) {
                logger.LogWarning("Failed to publish inventory texture {DisplayName}! No sources found.", context.Material.DisplayName);
                return;
            }

            imageWriter.Format = context.ImageFormat;
            await imageWriter.WriteAsync(itemImage, destFile, ImageChannels.ColorAlpha, token);

            logger.LogInformation("Published item texture {DisplayName}.", context.Material.DisplayName);
        }

        private async Task CopyPropertiesAsync(CancellationToken token)
        {
            var propsFileIn = naming.GetInputPropertiesName(context.Material);
            if (!reader.FileExists(propsFileIn)) return;

            var propsFileOut = naming.GetOutputPropertiesName(context.Material, context.PublishAsGlobal);

            await using var sourceStream = reader.Open(propsFileIn);
            await using var destStream = writer.Open(propsFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private async Task CopyMetaAsync(string tag, CancellationToken token)
        {
            var metaFileIn = naming.GetInputMetaName(context.Material, tag);
            if (!reader.FileExists(metaFileIn)) {
                metaFileIn = naming.GetInputMetaName(context.Material);
                if (!reader.FileExists(metaFileIn)) return;
            }

            var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, context.PublishAsGlobal);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private bool IsOutputUpToDate(DateTime packWriteTime, DateTime? sourceTime)
        {
            DateTime? destinationTime = null;

            var tags = context.OutputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            foreach (var file in GetMaterialOutputFiles(tags)) {
                var writeTime = writer.GetWriteTime(file);
                if (!writeTime.HasValue) continue;

                if (!destinationTime.HasValue || writeTime.Value < destinationTime.Value)
                    destinationTime = writeTime;
            }

            foreach (var tag in tags) {
                var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, context.PublishAsGlobal);
                var metaTime = reader.GetWriteTime(metaFileOut);

                if (metaTime.HasValue && (!sourceTime.HasValue || metaTime.Value > sourceTime.Value))
                    sourceTime = metaTime.Value;
            }

            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private IEnumerable<string> GetMaterialOutputFiles(IEnumerable<string> textureTags)
        {
            var repeatCount = new Lazy<int>(() => (context.Material.CtmCountX ?? 1) * (context.Material.CtmCountY ?? 1));

            foreach (var tag in textureTags) {
                if (context.IsMaterialMultiPart) {
                    foreach (var part in context.Material.Parts) {
                        var outputName = naming.GetOutputTextureName(context.Profile, part.Name, tag, true);
                        yield return PathEx.Join(context.Material.LocalPath, outputName);
                    }
                }
                else if (context.IsMaterialCtm) {
                    var tileCount = context.Material.CtmType switch {
                        CtmTypes.Compact => 5,
                        CtmTypes.Expanded => 47,
                        CtmTypes.Full => 47,
                        CtmTypes.Repeat => repeatCount.Value,
                        _ => 1,
                    };

                    for (var i = 0; i < tileCount; i++) {
                        var outputName = naming.GetOutputTextureName(context.Profile, i.ToString(), tag, true);
                        yield return PathEx.Join(context.Material.LocalPath, context.Material.Name, outputName);
                    }
                }
                else {
                    var outputName = naming.GetOutputTextureName(context.Profile, context.Material.Name, tag, true);
                    yield return PathEx.Join(context.Material.LocalPath, outputName);
                }
            }
        }

        private bool IsInventoryUpToDate(DateTime packWriteTime, DateTime? sourceTime)
        {
            var albedoOutputName = naming.GetOutputTextureName(context.Profile, context.Material.Name, TextureTags.Inventory, true);
            var albedoFile = PathEx.Join(context.Material.LocalPath, albedoOutputName);
            var destinationTime = writer.GetWriteTime(albedoFile);

            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private static bool IsUpToDate(DateTime profileWriteTime, DateTime? sourceWriteTime, DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }
    }
}
