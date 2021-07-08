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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures.Graphing
{
    public interface IPublishGraphBuilder
    {
        Task PublishAsync(CancellationToken token = default);
        Task PublishInventoryAsync(string suffix, CancellationToken token = default);
    }

    internal class PublishGraphBuilder : TextureGraphBuilder, IPublishGraphBuilder
    {
        private readonly ILogger<PublishGraphBuilder> logger;
        private readonly IEdgeFadeImageEffect edgeFadeEffect;
        private readonly IInventoryTextureGenerator itemGenerator;
        private string matMetaFileIn;


        public PublishGraphBuilder(
            ILogger<PublishGraphBuilder> logger,
            ITextureGraphContext context,
            ITextureGraph graph,
            IInputReader reader,
            IOutputWriter writer,
            IImageWriter imageWriter,
            IEdgeFadeImageEffect edgeFadeEffect,
            ITextureRegionEnumerator regions,
            IInventoryTextureGenerator itemGenerator)
            : base(context, graph, reader, writer, imageWriter, regions, logger)
        {
            this.edgeFadeEffect = edgeFadeEffect;
            this.itemGenerator = itemGenerator;
            this.logger = logger;
        }

        public async Task PublishAsync(CancellationToken token = default)
        {
            Context.ApplyInputEncoding();

            matMetaFileIn = NamingStructure.GetInputMetaName(Context.Material);
            Context.IsAnimated = Reader.FileExists(matMetaFileIn);

            var packWriteTime = Reader.GetWriteTime(Context.Profile.LocalFile) ?? DateTime.Now;
            var sourceTime = Reader.GetWriteTime(Context.Material.LocalFilename);
            
            if (Context.Material.Publish ?? true) {
                if (!IsOutputUpToDate(packWriteTime, sourceTime)) {
                    logger.LogDebug("Publishing material '{DisplayName}'.", Context.Material.DisplayName);
                    await ProcessAllTexturesAsync(true, token);
                    await CopyPropertiesAsync(token);
                }
                else {
                    logger.LogDebug("Skipping up-to-date material '{DisplayName}'.", Context.Material.DisplayName);
                }
            }
        }

        public async Task PublishInventoryAsync(string suffix, CancellationToken token = default)
        {
            if (!(Context.Material.PublishInventory ?? false)) return;

            var packWriteTime = Reader.GetWriteTime(Context.Profile.LocalFile) ?? DateTime.Now;
            var sourceTime = Reader.GetWriteTime(Context.Material.LocalFilename);
            
            if (!IsInventoryUpToDate(suffix, packWriteTime, sourceTime)) {
                await GenerateInventoryTextureAsync(suffix, token);
            }
            else {
                logger.LogDebug("Skipping up-to-date inventory texture for material '{DisplayName}.", Context.Material.DisplayName);
            }
        }

        protected override async Task ProcessTextureAsync<TPixel>(Image<TPixel> image, string textureTag, ImageChannels type, CancellationToken token = default)
        {
            await base.ProcessTextureAsync(image, textureTag, type, token);
            
            if (Context.Material.PublishInventory ?? false) {
                // WARN: half-ass filtering - fix asap
                if (TextureTags.Is(textureTag, TextureTags.Albedo)) return;

                var ext = NamingStructure.GetExtension(Context.Profile);
                var suffix = $"_inventory.{ext}";

                if (GetMappedInventoryName(suffix, out var destFile)) {
                    var _p = Path.GetDirectoryName(destFile);
                    var _n = Path.GetFileNameWithoutExtension(destFile);
                    _n = NamingStructure.Get(textureTag, _n, ext, true);
                    destFile = Path.Combine(_p, _n);

                    var part = Regions.GetAllPublishRegions(1, 0, null).First();
                    using var regionImage = GetImageRegion(image, part);

                    edgeFadeEffect.Apply(regionImage, textureTag);

                    //imageWriter.Format = context.ImageFormat;
                    await ImageWriter.WriteAsync(regionImage, type, destFile, token);

                    logger.LogInformation("Published inventory texture {destFile}.", destFile);
                }
            }
        }

        protected override async Task SaveImagePartAsync<TPixel>(Image<TPixel> image, TexturePublishPart part, ImageChannels type, string destFile, string textureTag, CancellationToken token)
        {
            using var regionImage = GetImageRegion(image, part);

            edgeFadeEffect.Apply(regionImage, textureTag);

            await ImageWriter.WriteAsync(regionImage, type, destFile, token);
            
            logger.LogInformation("Published material texture '{destFile}'.", destFile);
        }

        private async Task GenerateInventoryTextureAsync(string suffix, CancellationToken token = default)
        {
            if (!GetMappedInventoryName(suffix, out var destFile)) return;

            // Generate item image
            using var itemImage = await itemGenerator.CreateAsync(Graph, token);
            if (itemImage == null) {
                logger.LogWarning("Failed to publish inventory texture {destFile}! No sources found.", destFile);
                return;
            }

            //imageWriter.Format = context.ImageFormat;
            await ImageWriter.WriteAsync(itemImage, ImageChannels.ColorAlpha, destFile, token);

            logger.LogInformation("Published item texture {destFile}.", destFile);
        }

        private bool IsOutputUpToDate(DateTime packWriteTime, DateTime? sourceTime)
        {
            DateTime? destinationTime = null;

            var inputTags = Context.InputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            foreach (var file in GetMaterialInputFiles(inputTags)) {
                var writeTime = Reader.GetWriteTime(file);
                if (!writeTime.HasValue) continue;

                if (!sourceTime.HasValue || writeTime.Value > sourceTime.Value)
                    sourceTime = writeTime;
            }

            var outputTags = Context.OutputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            foreach (var file in GetMaterialOutputFiles(outputTags)) {
                var writeTime = Writer.GetWriteTime(file);
                if (!writeTime.HasValue) continue;

                if (!destinationTime.HasValue || writeTime.Value < destinationTime.Value)
                    destinationTime = writeTime;
            }

            foreach (var tag in outputTags) {
                var metaFileOut = NamingStructure.GetOutputMetaName(Context.Profile, Context.Material, tag, Context.PublishAsGlobal);
                var metaTime = Reader.GetWriteTime(metaFileOut);

                if (metaTime.HasValue && (!sourceTime.HasValue || metaTime.Value > sourceTime.Value))
                    sourceTime = metaTime.Value;
            }

            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private IEnumerable<string> GetMaterialInputFiles(IEnumerable<string> textureTags)
        {
            return textureTags.SelectMany(tag => Reader.EnumerateInputTextures(Context.Material, tag));
        }

        private IEnumerable<string> GetMaterialOutputFiles(IEnumerable<string> textureTags)
        {
            var repeatCount = new Lazy<int>(() => (Context.Material.CTM?.CountX ?? 1) * (Context.Material.CTM?.CountY ?? 1));

            var ext = NamingStructure.GetExtension(Context.Profile);
            var sourcePath = Context.Material.LocalPath;
            if (!Context.PublishAsGlobal || (Context.IsMaterialCtm && !Context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, Context.Material.Name);

            foreach (var tag in textureTags) {
                if (Context.IsMaterialMultiPart) {
                    foreach (var part in Context.Material.Parts) {
                        var sourceName = NamingStructure.Get(tag, part.Name, null, true);

                        if (Context.Mapping.TryMap(sourcePath, sourceName, out var destPath, out var destName))
                            yield return PathEx.Join(destPath, $"{destName}.{ext}");
                    }
                }
                else if (Context.IsMaterialCtm) {
                    var tileCount = Context.Material.CTM?.Type switch {
                        CtmTypes.Compact => 5,
                        CtmTypes.Expanded => 47,
                        CtmTypes.Full => 47,
                        CtmTypes.Repeat => repeatCount.Value,
                        _ => 1,
                    };

                    for (var i = 0; i < tileCount; i++) {
                        var usePlaceholder = Context.Material.CTM?.Placeholder ?? false;
                        var name = usePlaceholder ? Context.Material.Name : i.ToString();
                        var sourceName = NamingStructure.Get(tag, name, ext, true);

                        if (!Context.Material.UseGlobalMatching && !usePlaceholder)
                            sourceName = PathEx.Join(Context.Material.Name, sourceName);

                        var outputPath = usePlaceholder
                            ? PathEx.Join("assets", "minecraft", "textures", "block")
                            : Context.Material.LocalPath;

                        yield return PathEx.Join(outputPath, sourceName);
                    }
                }
                else {
                    var sourceName = NamingStructure.Get(tag, Context.Material.Name, null, true);

                    if (Context.Mapping.TryMap(sourcePath, sourceName, out var destPath, out var destName))
                        yield return PathEx.Join(destPath, $"{destName}.{ext}");
                }
            }
        }

        private bool IsInventoryUpToDate(in string suffix, in DateTime packWriteTime, in DateTime? sourceTime)
        {
            if (!GetMappedInventoryName(suffix, out var destFile)) return false;

            var destinationTime = Writer.GetWriteTime(destFile);
            return IsUpToDate(packWriteTime, sourceTime, destinationTime);
        }

        private bool GetMappedInventoryName(in string suffix, out string destFile)
        {
            var sourcePath = Context.Material.LocalPath;

            if (!Context.PublishAsGlobal || (Context.IsMaterialCtm && !Context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, Context.Material.Name);

            if (!Context.Mapping.TryMap(sourcePath, Context.Material.Name, out var destPath, out var destName)) {
                destFile = null;
                return false;
            }

            destFile = PathEx.Join(destPath, $"{destName}{suffix}");
            return true;
        }

        private Image<TPixel> GetImageRegion<TPixel>(Image<TPixel> image, TexturePublishPart part, int? targetFrame = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var srcWidth = image.Width;
            var srcHeight = image.Height;

            if (srcWidth == 1 && srcHeight == 1) return image.Clone();
            
            var firstFrame = part.Frames.First();
            var frameCount = !targetFrame.HasValue ? part.Frames.Length : 1;
            var partWidth = (int)(firstFrame.SourceBounds.Width * srcWidth);
            var partHeight = (int)(firstFrame.SourceBounds.Height * srcHeight * frameCount);

            var regionImage = new Image<TPixel>(partWidth, partHeight);

            try {
                var options = new CopyRegionProcessor<TPixel>.Options {
                    SourceImage = image,
                };

                var processor = new CopyRegionProcessor<TPixel>(options);

                if (targetFrame.HasValue) {
                    var frame = part.Frames[targetFrame.Value];
                    options.SourceX = (int) (frame.SourceBounds.X * srcWidth);
                    options.SourceY = (int) (frame.SourceBounds.Y * srcHeight);

                    regionImage.Mutate(c => c.ApplyProcessor(processor));
                }
                else {
                    foreach (var frame in part.Frames) {
                        options.SourceX = (int) (frame.SourceBounds.X * srcWidth);
                        options.SourceY = (int) (frame.SourceBounds.Y * srcHeight);

                        var outBounds = frame.DestBounds.ScaleTo(partWidth, partHeight);
                        regionImage.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }
                }
            }
            catch {
                regionImage.Dispose();
                throw;
            }

            return regionImage;
        }

        private static bool IsUpToDate(in DateTime profileWriteTime, in DateTime? sourceWriteTime, in DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }
    }
}
