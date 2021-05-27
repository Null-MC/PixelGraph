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
        Task PublishInventoryAsync(string suffix, CancellationToken token = default);
    }

    internal class TextureGraphBuilder : ITextureGraphBuilder
    {
        private readonly ILogger<TextureGraphBuilder> logger;
        private readonly ITextureGraphContext context;
        private readonly ITextureGraph graph;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly IImageWriter imageWriter;
        private readonly IEdgeFadeImageEffect edgeFadeEffect;
        private readonly IInventoryTextureGenerator itemGenerator;
        private readonly ITextureRegionEnumerator regions;
        private string matMetaFileIn;


        public TextureGraphBuilder(
            ILogger<TextureGraphBuilder> logger,
            ITextureGraphContext context,
            ITextureGraph graph,
            IInputReader reader,
            IOutputWriter writer,
            IImageWriter imageWriter,
            IEdgeFadeImageEffect edgeFadeEffect,
            IInventoryTextureGenerator itemGenerator,
            ITextureRegionEnumerator regions)
        {
            this.context = context;
            this.graph = graph;
            this.reader = reader;
            this.writer = writer;
            this.imageWriter = imageWriter;
            this.edgeFadeEffect = edgeFadeEffect;
            this.itemGenerator = itemGenerator;
            this.regions = regions;
            this.logger = logger;
        }

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        /// <returns>An array of all published texture tags.</returns>
        public async Task ProcessInputGraphAsync(CancellationToken token = default)
        {
            context.ApplyInputEncoding();

            matMetaFileIn = context.GetMetaInputFilename();
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;
            var sourceTime = reader.GetWriteTime(context.Material.LocalFilename);
            
            if (context.Material.Publish ?? true) {
                if (!IsOutputUpToDate(packWriteTime, sourceTime)) {
                    logger.LogDebug("Publishing material '{DisplayName}'.", context.Material.DisplayName);
                    await ProcessAllTexturesAsync(true, token);
                }
                else {
                    logger.LogDebug("Skipping up-to-date material '{DisplayName}'.", context.Material.DisplayName);
                }
            }
        }

        public async Task PublishInventoryAsync(string suffix, CancellationToken token = default)
        {
            if (!(context.Material.PublishInventory ?? false)) return;

            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;
            var sourceTime = reader.GetWriteTime(context.Material.LocalFilename);
            
            if (!IsInventoryUpToDate(suffix, packWriteTime, sourceTime)) {
                await GenerateInventoryTextureAsync(suffix, token);
            }
            else {
                logger.LogDebug("Skipping up-to-date inventory texture for material '{DisplayName}.", context.Material.DisplayName);
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

        /// <summary>
        /// Publishes all textures with mapped output.
        /// </summary>
        /// <returns>An array of all published texture tags.</returns>
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
                logger.LogWarning("No texture sources found for item {DisplayName} texture {textureTag}.", context.Material.DisplayName, textureTag);
                return;
            }

            //imageWriter.Format = context.ImageFormat;

            var sourcePath = context.Material.LocalPath;
            //var destPath = context.DestinationPath;
            //if (!context.PublishAsGlobal || (context.IsMaterialCtm && !context.Material.UseGlobalMatching)) destPath = PathEx.Join(destPath, context.DestinationName);

            var maxFrameCount = graph.GetMaxFrameCount();
            var usePlaceholder = context.Material.CTM?.Placeholder ?? false;
            var ext = NamingStructure.GetExtension(context.Profile);

            foreach (var part in regions.GetAllPublishRegions(maxFrameCount)) {
                string destFile;
                if (usePlaceholder && part.TileIndex == 0) {
                    var placeholderPath = PathEx.Join("assets", "minecraft", "textures", "block");
                    if (!context.Mapping.TryMap(placeholderPath, context.Material.Name, out var destPath, out var destName)) continue;

                    var destTagName = NamingStructure.Get(textureTag, destName, ext, true);
                    destFile = PathEx.Join(destPath, destTagName);
                }
                else {
                    if (!context.Mapping.TryMap(sourcePath, part.Name, out var destPath, out var destName)) continue;

                    var destTagName = NamingStructure.Get(textureTag, destName, ext, true);
                    destFile = PathEx.Join(destPath, destTagName);
                }

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

        private async Task GenerateInventoryTextureAsync(string suffix, CancellationToken token = default)
        {
            if (!GetMappedInventoryName(suffix, out var destFile)) return;

            // Generate item image
            using var itemImage = await itemGenerator.CreateAsync(graph, token);
            if (itemImage == null) {
                logger.LogWarning("Failed to publish inventory texture {destFile}! No sources found.", destFile);
                return;
            }

            //imageWriter.Format = context.ImageFormat;
            await imageWriter.WriteAsync(itemImage, destFile, ImageChannels.ColorAlpha, token);

            logger.LogInformation("Published item texture {destFile}.", destFile);
        }

        private async Task CopyPropertiesAsync(CancellationToken token)
        {
            var propsFileIn = NamingStructure.GetInputPropertiesName(context.Material);
            if (!reader.FileExists(propsFileIn)) return;

            var propsFileOut = NamingStructure.GetOutputPropertiesName(context.Material, context.PublishAsGlobal);

            await using var sourceStream = reader.Open(propsFileIn);
            await using var destStream = writer.Open(propsFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private async Task CopyMetaAsync(string tag, CancellationToken token)
        {
            var metaFileIn = NamingStructure.GetInputMetaName(context.Material, tag);
            if (!reader.FileExists(metaFileIn)) {
                metaFileIn = NamingStructure.GetInputMetaName(context.Material);
                if (!reader.FileExists(metaFileIn)) return;
            }

            var metaFileOut = NamingStructure.GetOutputMetaName(context.Profile, context.Material, tag, context.PublishAsGlobal);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private bool IsOutputUpToDate(DateTime packWriteTime, DateTime? sourceTime)
        {
            DateTime? destinationTime = null;

            var inputTags = context.InputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            foreach (var file in GetMaterialInputFiles(inputTags)) {
                var writeTime = reader.GetWriteTime(file);
                if (!writeTime.HasValue) continue;

                if (!sourceTime.HasValue || writeTime.Value > sourceTime.Value)
                    sourceTime = writeTime;
            }

            var outputTags = context.OutputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            foreach (var file in GetMaterialOutputFiles(outputTags)) {
                var writeTime = writer.GetWriteTime(file);
                if (!writeTime.HasValue) continue;

                if (!destinationTime.HasValue || writeTime.Value < destinationTime.Value)
                    destinationTime = writeTime;
            }

            foreach (var tag in outputTags) {
                var metaFileOut = NamingStructure.GetOutputMetaName(context.Profile, context.Material, tag, context.PublishAsGlobal);
                var metaTime = reader.GetWriteTime(metaFileOut);

                if (metaTime.HasValue && (!sourceTime.HasValue || metaTime.Value > sourceTime.Value))
                    sourceTime = metaTime.Value;
            }

            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private IEnumerable<string> GetMaterialInputFiles(IEnumerable<string> textureTags)
        {
            return textureTags.SelectMany(tag => reader.EnumerateInputTextures(context.Material, tag));
        }

        private IEnumerable<string> GetMaterialOutputFiles(IEnumerable<string> textureTags)
        {
            var repeatCount = new Lazy<int>(() => (context.Material.CTM?.CountX ?? 1) * (context.Material.CTM?.CountY ?? 1));

            //var sourcePath = context.
            //var ext = NamingStructure.GetExtension(context.Profile);

            var ext = NamingStructure.GetExtension(context.Profile);
            var sourcePath = context.Material.LocalPath;
            if (!context.PublishAsGlobal || (context.IsMaterialCtm && !context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, context.Material.Name);

            foreach (var tag in textureTags) {
                if (context.IsMaterialMultiPart) {
                    foreach (var part in context.Material.Parts) {
                        var sourceName = NamingStructure.Get(tag, part.Name, ext, true);

                        if (context.Mapping.TryMap(sourcePath, sourceName, out var destPath, out var destName))
                            yield return PathEx.Join(destPath, destName);
                    }
                }
                else if (context.IsMaterialCtm) {
                    var tileCount = context.Material.CTM?.Type switch {
                        CtmTypes.Compact => 5,
                        CtmTypes.Expanded => 47,
                        CtmTypes.Full => 47,
                        CtmTypes.Repeat => repeatCount.Value,
                        _ => 1,
                    };

                    for (var i = 0; i < tileCount; i++) {
                        var usePlaceholder = context.Material.CTM?.Placeholder ?? false;
                        var name = usePlaceholder ? context.Material.Name : i.ToString();
                        var sourceName = NamingStructure.Get(tag, name, ext, true);

                        if (!context.Material.UseGlobalMatching && !usePlaceholder)
                            sourceName = PathEx.Join(context.Material.Name, sourceName);

                        var outputPath = usePlaceholder
                            ? PathEx.Join("assets", "minecraft", "textures", "block")
                            : context.Material.LocalPath;

                        yield return PathEx.Join(outputPath, sourceName);
                    }
                }
                else {
                    var sourceName = NamingStructure.Get(tag, context.Material.Name, ext, true);

                    if (context.Mapping.TryMap(sourcePath, sourceName, out var destPath, out var destName))
                        yield return PathEx.Join(destPath, destName);
                }
            }
        }

        private bool IsInventoryUpToDate(in string suffix, in DateTime packWriteTime, in DateTime? sourceTime)
        {
            if (!GetMappedInventoryName(suffix, out var destFile)) return false;

            var destinationTime = writer.GetWriteTime(destFile);
            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private static bool IsUpToDate(in DateTime profileWriteTime, in DateTime? sourceWriteTime, in DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }

        private bool GetMappedInventoryName(in string suffix, out string destFile)
        {
            var sourcePath = context.Material.LocalPath;

            if (!context.PublishAsGlobal || (context.IsMaterialCtm && !context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, context.Material.Name);

            if (!context.Mapping.TryMap(sourcePath, context.Material.Name, out var destPath, out var destName)) {
                destFile = null;
                return false;
            }

            destFile = PathEx.Join(destPath, $"{destName}{suffix}");
            return true;
        }
    }
}
