using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
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
        private readonly IItemGenerator itemGenerator;
        private readonly ILogger logger;


        public TextureGraphBuilder(
            ILogger<TextureGraphBuilder> logger,
            ITextureGraphContext context,
            ITextureGraph graph,
            IInputReader reader,
            IOutputWriter writer,
            INamingStructure naming,
            IImageWriter imageWriter,
            IItemGenerator itemGenerator)
        {
            this.context = context;
            this.graph = graph;
            this.reader = reader;
            this.writer = writer;
            this.naming = naming;
            this.imageWriter = imageWriter;
            this.itemGenerator = itemGenerator;
            this.logger = logger;
        }

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        public async Task ProcessInputGraphAsync(CancellationToken token = default)
        {
            context.ApplyInputEncoding();

            var sourceTime = reader.GetWriteTime(context.Material.LocalFilename);
            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;

            foreach (var texFile in reader.EnumerateAllTextures(context.Material)) {
                var z = reader.GetWriteTime(texFile);
                if (!z.HasValue) continue;

                if (!sourceTime.HasValue || z.Value > sourceTime.Value)
                    sourceTime = z.Value;
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

        private async Task ProcessAllTexturesAsync(bool createEmpty, CancellationToken token)
        {
            try {
                await graph.PreBuildNormalTextureAsync(token);
            }
            catch (HeightSourceEmptyException) {}

            var allOutputTags = context.OutputEncoding
                .Select(e => e.Texture).Distinct();

            var matPath = context.Material.UseGlobalMatching
                ? context.Material.LocalPath
                : PathEx.Join(context.Material.LocalPath, context.Material.Name);

            var matMetaFileIn = PathEx.Join(matPath, "mat.mcmeta");
            var hasMatMeta = reader.FileExists(matMetaFileIn);

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

                if (hasMatMeta) {
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

            var p = context.Material.LocalPath;
            if (!context.UseGlobalOutput) p = PathEx.Join(p, context.Material.Name);

            if (context.Material.TryGetSourceBounds(out var bounds)) {
                var scaleX = (float)image.Width / bounds.Width;

                foreach (var region in context.Material.Parts) {
                    var name = naming.GetOutputTextureName(context.Profile, region.Name, textureTag, context.UseGlobalOutput);
                    var destFile = PathEx.Join(p, name);

                    if (image.Width == 1 && image.Height == 1) {
                        await imageWriter.WriteAsync(image, destFile, type, token);
                    }
                    else {
                        var regionBounds = region.GetRectangle();
                        regionBounds.X = (int)MathF.Ceiling(regionBounds.X * scaleX);
                        regionBounds.Y = (int)MathF.Ceiling(regionBounds.Y * scaleX);
                        regionBounds.Width = (int)MathF.Ceiling(regionBounds.Width * scaleX);
                        regionBounds.Height = (int)MathF.Ceiling(regionBounds.Height * scaleX);

                        using var regionImage = image.Clone(c => c.Crop(regionBounds));

                        graph.FixEdges(regionImage, textureTag);

                        await imageWriter.WriteAsync(regionImage, destFile, type, token);
                    }

                    logger.LogInformation("Published texture region {Name} tag {textureTag}.", region.Name, textureTag);
                }
            }
            else {
                var name = naming.GetOutputTextureName(context.Profile, context.Material.Name, textureTag, context.UseGlobalOutput);
                var destFile = PathEx.Join(p, name);

                graph.FixEdges(image, textureTag);

                await imageWriter.WriteAsync(image, destFile, type, token);

                logger.LogInformation("Published texture {DisplayName} tag {textureTag}.", context.Material.DisplayName, textureTag);
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

            if (context.Material.IsMultiPart) {
                foreach (var part in context.Material.Parts) {
                    var albedoOutputName = naming.GetOutputTextureName(context.Profile, part.Name, TextureTags.Albedo, true);
                    var albedoFile = PathEx.Join(context.Material.LocalPath, albedoOutputName);
                    var writeTime = writer.GetWriteTime(albedoFile);
                    if (!writeTime.HasValue) continue;

                    if (!destinationTime.HasValue || writeTime.Value > destinationTime.Value)
                        destinationTime = writeTime;
                }
            }
            else {
                var albedoOutputName = naming.GetOutputTextureName(context.Profile, context.Material.Name, TextureTags.Albedo, true);
                var albedoFile = PathEx.Join(context.Material.LocalPath, albedoOutputName);
                destinationTime = writer.GetWriteTime(albedoFile);
            }

            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
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
