using Microsoft.Extensions.DependencyInjection;
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
        Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default);
        Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default);
    }

    internal class TextureGraphBuilder : ITextureGraphBuilder
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly INamingStructure naming;
        private readonly IImageWriter imageWriter;
        private readonly ILogger logger;


        public TextureGraphBuilder(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            writer = provider.GetRequiredService<IOutputWriter>();
            naming = provider.GetRequiredService<INamingStructure>();
            imageWriter = provider.GetRequiredService<IImageWriter>();
            logger = provider.GetRequiredService<ILogger<TextureGraphBuilder>>();
        }

        /// <summary>
        /// Input -> Output; for publishing textures
        /// </summary>
        public async Task ProcessInputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            context.ApplyInputEncoding();

            var graph = provider.GetRequiredService<ITextureGraph>();
            graph.Context = context;

            var sourceTime = reader.GetWriteTime(context.Material.LocalFilename);
            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;

            foreach (var texFile in reader.EnumerateAllTextures(context.Material)) {
                var z = reader.GetWriteTime(texFile);
                if (!z.HasValue) continue;

                if (!sourceTime.HasValue || z.Value > sourceTime.Value)
                    sourceTime = z.Value;
            }

            if (!IsOutputUpToDate(context, packWriteTime, sourceTime)) {
                logger.LogDebug($"Publishing texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
                await ProcessAllTexturesAsync(graph, context, token);
            }
            else {
                logger.LogDebug($"Skipping up-to-date texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
            }

            if (context.Material.CreateInventory ?? false) {
                if (!IsInventoryUpToDate(context, packWriteTime, sourceTime)) {
                    // TODO: check item generated for up-to-date
                    await GenerateItemTextureAsync(graph, context, token);
                }
                else {
                    logger.LogDebug($"Skipping up-to-date item texture {context.Material.LocalPath}:{{DisplayName}}.", context.Material.DisplayName);
                }
            }
        }

        /// <summary>
        /// Output -> Input; for importing textures
        /// </summary>
        public async Task ProcessOutputGraphAsync(MaterialContext context, CancellationToken token = default)
        {
            context.ApplyOutputEncoding();

            var graph = provider.GetRequiredService<ITextureGraph>();
            graph.Context = context;

            await ProcessAllTexturesAsync(graph, context, token);
        }

        private async Task ProcessAllTexturesAsync(ITextureGraph graph, MaterialContext context, CancellationToken token)
        {
            try {
                await graph.PreBuildNormalTextureAsync(token);
            }
            catch (HeightSourceEmptyException) {}

            var allOutputTags = graph.Context.OutputEncoding
                .Select(e => e.Texture).Distinct();

            foreach (var tag in allOutputTags) {
                var tagOutputEncoding = graph.Context.OutputEncoding
                    .Where(e => TextureTags.Is(e.Texture, tag)).ToArray();

                if (tagOutputEncoding.Any()) {
                    var hasAlpha = tagOutputEncoding.Any(c => c.Color == ColorChannel.Alpha);
                    var hasColor = tagOutputEncoding.Any(c => c.Color != ColorChannel.Red);

                    if (hasAlpha) {
                        await PublishImageAsync<Rgba32>(graph, context, tag, ImageChannels.ColorAlpha, token);
                    }
                    else if (hasColor) {
                        await PublishImageAsync<Rgb24>(graph, context, tag, ImageChannels.Color, token);
                    }
                    else {
                        await PublishImageAsync<L8>(graph, context, tag, ImageChannels.Gray, token);
                    }
                }

                await CopyMetaAsync(graph.Context, tag, token);
            }
        }

        private async Task PublishImageAsync<TPixel>(ITextureGraph graph, MaterialContext context, string textureTag, ImageChannels type, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var image = await graph.CreateImageAsync<TPixel>(textureTag, true, token);

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

        private async Task GenerateItemTextureAsync(ITextureGraph graph, MaterialContext context, CancellationToken token = default)
        {
            var name = naming.GetOutputTextureName(context.Profile, context.Material.Name, TextureTags.Inventory, true);

            var path = context.Material.LocalPath;
            if (!context.UseGlobalOutput) path = PathEx.Join(path, context.Material.Name);
            var destFile = PathEx.Join(path, name);

            // Generate item image
            var generator = provider.GetRequiredService<IItemGenerator>();
            generator.Context = context;

            using var itemImage = await generator.CreateAsync(graph, token);
            if (itemImage == null) {
                logger.LogWarning("Failed to publish item texture {DisplayName}! No sources found.", context.Material.DisplayName);
                return;
            }

            imageWriter.Format = context.ImageFormat;
            await imageWriter.WriteAsync(itemImage, destFile, ImageChannels.ColorAlpha, token);

            logger.LogInformation("Published item texture {DisplayName}.", context.Material.DisplayName);
        }

        private async Task CopyMetaAsync(MaterialContext context, string tag, CancellationToken token)
        {
            var metaFileIn = naming.GetInputMetaName(context.Material, tag);
            if (!reader.FileExists(metaFileIn)) return;

            var metaFileOut = naming.GetOutputMetaName(context.Profile, context.Material, tag, context.UseGlobalOutput);

            await using var sourceStream = reader.Open(metaFileIn);
            await using var destStream = writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        private bool IsOutputUpToDate(MaterialContext context, DateTime packWriteTime, DateTime? sourceTime)
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

        private bool IsInventoryUpToDate(MaterialContext context, DateTime packWriteTime, DateTime? sourceTime)
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
