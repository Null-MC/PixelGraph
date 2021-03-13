using Microsoft.Extensions.Logging;
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
using PixelGraph.Common.ConnectedTextures;

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
            prep();

            var sourceTime = reader.GetWriteTime(context.Material.LocalFilename);
            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;

            foreach (var tag in TextureTags.All) {
                foreach (var texFile in reader.EnumerateTextures(context.Material, tag)) {
                    var z = reader.GetWriteTime(texFile);
                    if (!z.HasValue) continue;

                    if (!sourceTime.HasValue || z.Value > sourceTime.Value)
                        sourceTime = z.Value;
                }

                var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, context.UseGlobalOutput);
                var metaTime = reader.GetWriteTime(metaFileOut);

                if (metaTime.HasValue && (!sourceTime.HasValue || metaTime.Value > sourceTime.Value))
                    sourceTime = metaTime.Value;
            }

            if (!IsOutputUpToDate(packWriteTime, sourceTime)) {
                logger.LogDebug($"Publishing texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
                await ProcessAllTexturesAsync(true, token);
            }
            else {
                logger.LogDebug($"Skipping up-to-date texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
            }

            if (context.Material.CreateInventory ?? false) {
                if (!IsInventoryUpToDate(packWriteTime, sourceTime)) {
                    // TODO: check item generated for up-to-date
                    await GenerateItemTextureAsync(token);
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

        private void prep()
        {
            var matPath = context.Material.UseGlobalMatching
                ? context.Material.LocalPath
                : PathEx.Join(context.Material.LocalPath, context.Material.Name);

            matMetaFileIn = PathEx.Join(matPath, "mat.mcmeta");
            context.IsAnimated = reader.FileExists(matMetaFileIn);
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

                await CopyMetaAsync(tag, token);

                if (context.IsAnimated) {
                    var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, context.UseGlobalOutput);

                    await using var sourceStream = reader.Open(matMetaFileIn);
                    await using var destStream = writer.Open(metaFileOut);
                    await sourceStream.CopyToAsync(destStream, token);
                }
            }
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

            if (context.IsMaterialMultiPart || context.IsMaterialCtm) {
                await SaveMultiPartAsync(image, textureTag, type, token);
            }
            else {
                await SaveDefaultAsync(image, textureTag, type, token);
            }
        }

        private async Task SaveDefaultAsync(Image image, string textureTag, ImageChannels type, CancellationToken token)
        {
            var p = context.Material.LocalPath;
            if (!context.UseGlobalOutput) p = PathEx.Join(p, context.Material.Name);

            var name = naming.GetOutputTextureName(context.Profile, context.Material.Name, textureTag, context.UseGlobalOutput);
            var destFile = PathEx.Join(p, name);

            edgeFadeEffect.Apply(image, textureTag);

            await imageWriter.WriteAsync(image, destFile, type, token);

            logger.LogInformation("Published texture {DisplayName} tag {textureTag}.", context.Material.DisplayName, textureTag);
        }

        private async Task SaveMultiPartAsync<TPixel>(Image<TPixel> image, string textureTag, ImageChannels type, CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var p = context.Material.LocalPath;
            if (!context.UseGlobalOutput) p = PathEx.Join(p, context.Material.Name);

            foreach (var region in regions.GetRegions(image)) {
                var name = naming.GetOutputTextureName(context.Profile, region.Name, textureTag, context.UseGlobalOutput);
                var destFile = PathEx.Join(p, name);

                if (image.Width == 1 && image.Height == 1) {
                    await imageWriter.WriteAsync(image, destFile, type, token);
                }
                else if (region.Mappings != null) {
                    using var regionImage = new Image<TPixel>(region.Bounds.Width, region.Bounds.Height);

                    regionImage.Mutate(c => {
                        foreach (var mapping in region.Mappings) {
                            var options = new CopyRegionProcessor<TPixel>.Options {
                                SourceImage = image,
                                SourceX = mapping.SourceBounds.X,
                                SourceY = mapping.SourceBounds.Y,
                            };

                            var processor = new CopyRegionProcessor<TPixel>(options);
                            c.ApplyProcessor(processor, mapping.DestBounds);
                        }
                    });

                    edgeFadeEffect.Apply(regionImage, textureTag);

                    await imageWriter.WriteAsync(regionImage, destFile, type, token);
                }
                else {
                    using var regionImage = image.Clone(c => c.Crop(region.Bounds));

                    edgeFadeEffect.Apply(regionImage, textureTag);

                    await imageWriter.WriteAsync(regionImage, destFile, type, token);
                }

                logger.LogInformation("Published texture region {Name} tag {textureTag}.", region.Name, textureTag);
            }
        }

        private async Task GenerateItemTextureAsync(CancellationToken token = default)
        {
            var name = naming.GetOutputTextureName(context.Profile, context.Material.Name, TextureTags.Inventory, true);

            var path = context.Material.LocalPath;
            if (!context.UseGlobalOutput) path = PathEx.Join(path, context.Material.Name);
            var destFile = PathEx.Join(path, name);

            // Generate item image
            using var itemImage = await itemGenerator.CreateAsync(graph, token);
            if (itemImage == null) {
                logger.LogWarning("Failed to publish item texture {DisplayName}! No sources found.", context.Material.DisplayName);
                return;
            }

            imageWriter.Format = context.ImageFormat;
            await imageWriter.WriteAsync(itemImage, destFile, ImageChannels.ColorAlpha, token);

            logger.LogInformation("Published item texture {DisplayName}.", context.Material.DisplayName);
        }

        private async Task CopyMetaAsync(string tag, CancellationToken token)
        {
            var metaFileIn = naming.GetInputMetaName(context.Material, tag);
            if (!reader.FileExists(metaFileIn)) return;

            var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, context.UseGlobalOutput);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private bool IsOutputUpToDate(DateTime packWriteTime, DateTime? sourceTime)
        {
            DateTime? destinationTime = null;

            var tags = context.OutputEncoding
                .Select(e => e.Texture).Distinct();

            foreach (var file in GetMaterialOutputFiles(tags)) {
                var writeTime = writer.GetWriteTime(file);
                if (!writeTime.HasValue) continue;

                if (!destinationTime.HasValue || writeTime.Value < destinationTime.Value)
                    destinationTime = writeTime;
            }

            // TODO: update from mat.mcmeta file
            
            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private IEnumerable<string> GetMaterialOutputFiles(IEnumerable<string> textureTags)
        {
            foreach (var tag in textureTags) {
                if (context.IsMaterialMultiPart) {
                    foreach (var part in context.Material.Parts) {
                        var outputName = naming.GetOutputTextureName(context.Profile, part.Name, tag, true);
                        yield return PathEx.Join(context.Material.LocalPath, outputName);
                    }
                }
                else if (context.IsMaterialCtm) {
                    switch (context.Material.CtmType) {
                        case CtmTypes.Compact:
                        case CtmTypes.Expanded:
                            for (var x = 0; x < 5; x++) {
                                var outputName = naming.GetOutputTextureName(context.Profile, x.ToString(), tag, true);
                                yield return PathEx.Join(context.Material.LocalPath, outputName);
                            }
                            break;
                        case CtmTypes.Full:
                            for (var y = 0; y < 4; y++) {
                                for (var x = 0; x < 12; x++) {
                                    var index = y * 12 + x;
                                    var outputName = naming.GetOutputTextureName(context.Profile, index.ToString(), tag, true);
                                    yield return PathEx.Join(context.Material.LocalPath, outputName);
                                }
                            }
                            break;
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
