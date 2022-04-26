using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Effects;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures.Graphing.Builders
{
    public interface IPublishGraphBuilder
    {
        Task PublishAsync(CancellationToken token = default);
        Task PublishInventoryAsync(CancellationToken token = default);
    }

    internal class PublishGraphBuilder : TextureGraphBuilder, IPublishGraphBuilder
    {
        private readonly ILogger<PublishGraphBuilder> logger;
        private readonly IEdgeFadeImageEffect edgeFadeEffect;
        private readonly IItemTextureGenerator itemGenerator;
        private string matMetaFileIn;


        public PublishGraphBuilder(
            ILogger<PublishGraphBuilder> logger,
            IServiceProvider provider,
            IEdgeFadeImageEffect edgeFadeEffect,
            IItemTextureGenerator itemGenerator)
            : base(provider, logger)
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

            var sourceTime = Reader.GetWriteTime(Context.Material.LocalFilename);

            if (!(Context.Material.Publish ?? true)) return;

            if (IsOutputUpToDate(sourceTime)) {
                logger.LogDebug("Skipping up-to-date material '{DisplayName}'.", Context.Material.DisplayName);
                return;
            }

            logger.LogDebug("Publishing material '{DisplayName}'.", Context.Material.DisplayName);

            try {
                await ProcessAllTexturesAsync(true, token);
                //await CopyPropertiesAsync(token);
                Summary.IncrementMaterialCount();
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to publish material '{Context.Material.DisplayName}'!", error);
            }
        }

        public async Task PublishInventoryAsync(CancellationToken token = default)
        {
            var publish = Context.Profile.PublishInventory ?? ResourcePackProfileProperties.PublishInventoryDefault;

            if (!publish) {
                logger.LogDebug("Skipping inventory texture '{DisplayName}'. feature disabled.", Context.Material.DisplayName);
                return;
            }

            var sourceTime = Reader.GetWriteTime(Context.Material.LocalFilename);
            
            var ext = NamingStructure.GetExtension(Context.Profile);
            if (!GetMappedInventoryName($"_inventory.{ext}", out var destFile)) return;

            if (!IsInventoryUpToDate(destFile, sourceTime)) {
                await GenerateInventoryTextureAsync(destFile, token);
            }
            else {
                logger.LogDebug("Skipping up-to-date inventory texture for material '{DisplayName}.", Context.Material.DisplayName);
            }
        }

        protected override async Task ProcessTextureAsync<TPixel>(Image<TPixel> image, string textureTag, ImageChannels type, CancellationToken token = default)
        {
            await base.ProcessTextureAsync(image, textureTag, type, token);
            
            var packPublishInventory = Context.Profile.PublishInventory ?? ResourcePackProfileProperties.PublishInventoryDefault;

            // WARN: hack for only publishing inventory color
            if (TextureTags.Is(textureTag, TextureTags.Color) && packPublishInventory && (Context.Material.PublishItem ?? false)) {
                var ext = NamingStructure.GetExtension(Context.Profile);
                var suffix = $"_inventory.{ext}";

                if (GetMappedInventoryName(suffix, out var destFile)) {
                    var _p = Path.GetDirectoryName(destFile);
                    var _n = Path.GetFileNameWithoutExtension(destFile);
                    _n = TexWriter.TryGet(textureTag, _n, ext, true);
                    if (_p == null || _n == null) {
                        // WARN: WHAT DO WE DO?!
                        throw new NotImplementedException();
                    }

                    destFile = Path.Combine(_p, _n); 

                    var regions = Provider.GetRequiredService<TextureRegionEnumerator>();
                    regions.SourceFrameCount = Context.MaxFrameCount;
                    regions.DestFrameCount = Context.MaxFrameCount;

                    var part = regions.GetAllPublishRegions().First();
                    using var regionImage = GetImageRegion(image, part);

                    edgeFadeEffect.Apply(regionImage, textureTag);

                    await ImageWriter.WriteAsync(regionImage, type, destFile, token);

                    logger.LogInformation("Published inventory texture {destFile}.", destFile);
                }
            }
        }

        protected override async Task<long> SaveImagePartAsync<TPixel>(Image<TPixel> image, TexturePublishPart part, ImageChannels type, string destFile, string textureTag, CancellationToken token)
        {
            using var regionImage = GetImageRegion(image, part);

            edgeFadeEffect.Apply(regionImage, textureTag);

            var diskSize = await ImageWriter.WriteAsync(regionImage, type, destFile, token);
            
            logger.LogInformation("Published material texture '{destFile}'.", destFile);
            return diskSize;
        }

        private async Task GenerateInventoryTextureAsync(string destFile, CancellationToken token = default)
        {
            // Generate item image
            using var itemImage = await itemGenerator.CreateAsync(Graph, token);
            if (itemImage == null) {
                logger.LogWarning("Failed to publish inventory texture {destFile}! No sources found.", destFile);
                return;
            }

            var diskSize = await ImageWriter.WriteAsync(itemImage, ImageChannels.ColorAlpha, destFile, token);

            Summary.IncrementTextureCount();
            Summary.AddRawBytes(itemImage.Width, itemImage.Height);
            Summary.AddDiskBytes(diskSize);

            logger.LogInformation("Published item texture {destFile}.", destFile);
        }

        private bool IsOutputUpToDate(DateTime? sourceTime)
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
                var metaFileOut = TexWriter.GetOutputMetaName(Context.Profile, Context.Material, tag, Context.PublishAsGlobal);
                var metaTime = Reader.GetWriteTime(metaFileOut);

                if (metaTime.HasValue && (!sourceTime.HasValue || metaTime.Value > sourceTime.Value))
                    sourceTime = metaTime.Value;
            }

            return IsUpToDate(Context.PackWriteTime, sourceTime, destinationTime);
        }

        private IEnumerable<string> GetMaterialInputFiles(IEnumerable<string> textureTags)
        {
            return textureTags.SelectMany(tag => TexReader.EnumerateInputTextures(Context.Material, tag));
        }

        private IEnumerable<string> GetMaterialOutputFiles(IEnumerable<string> textureTags)
        {
            var ext = NamingStructure.GetExtension(Context.Profile);
            var sourcePath = Context.Material.LocalPath;
            if (!Context.PublishAsGlobal || (Context.IsMaterialCtm && !Context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, Context.Material.Name);

            foreach (var tag in textureTags) {
                if (Context.IsMaterialMultiPart) {
                    foreach (var part in Context.Material.Parts) {
                        var sourceName = TexWriter.TryGet(tag, part.Name, null, true);
                        if (sourceName == null) {
                            // WARN: WHAT DO WE DO?!
                            throw new NotImplementedException();
                        }

                        if (Context.Mapping.TryMap(sourcePath, sourceName, out var destPath, out var destName))
                            yield return PathEx.Join(destPath, $"{destName}.{ext}");
                    }
                }
                else if (Context.IsMaterialCtm) {
                    var tileCount = CtmTypes.GetBounds(Context.Material.CTM)?.Total ?? 1;

                    var usePlaceholder = Context.Material.CTM?.Placeholder ?? false;
                    var firstTileIndex = Context.Material.CTM?.TileStartIndex ??
                                         Context.Profile?.TileStartIndex ?? 1;

                    for (var i = 0; i < tileCount; i++) {
                        var name = usePlaceholder && i == 0
                            ? Context.Material.Name
                            : (firstTileIndex + i).ToString();

                        var sourceName = TexWriter.TryGet(tag, name, ext, true);
                        if (sourceName == null) {
                            // WARN: WHAT DO WE DO?!
                            throw new NotImplementedException();
                        }

                        if (!Context.Material.UseGlobalMatching && !usePlaceholder)
                            sourceName = PathEx.Join(Context.Material.Name, sourceName);

                        var outputPath = usePlaceholder
                            ? PathEx.Join("assets", "minecraft", "textures", "block")
                            : Context.Material.LocalPath;

                        yield return PathEx.Join(outputPath, sourceName);
                    }
                }
                else {
                    var sourceName = TexWriter.TryGet(tag, Context.Material.Name, null, true);
                    if (sourceName == null) {
                        // WARN: WHAT DO WE DO?!
                        throw new NotImplementedException();
                    }

                    if (Context.Mapping.TryMap(sourcePath, sourceName, out var destPath, out var destName))
                        yield return PathEx.Join(destPath, $"{destName}.{ext}");
                }
            }
        }

        private bool IsInventoryUpToDate(in string destFile, in DateTime? sourceTime)
        {
            var destinationTime = Writer.GetWriteTime(destFile);
            return IsUpToDate(Context.PackWriteTime, sourceTime, destinationTime);
        }

        private bool GetMappedInventoryName(in string suffix, out string destFile)
        {
            var sourcePath = Context.Material.LocalPath;

            if (Context.IsMaterialCtm)
                sourcePath = PathEx.Join("assets", "minecraft", "textures", "block");
            else if (!Context.PublishAsGlobal) // || (Context.IsMaterialCtm && !Context.Material.UseGlobalMatching)
                sourcePath = PathEx.Join(sourcePath, Context.Material.Name);

            if (!Context.Mapping.TryMap(sourcePath, Context.Material.Name, out var destPath, out var destName)) {
                destFile = null;
                return false;
            }

            destFile = PathEx.Join(destPath, $"{destName}{suffix}");
            return true;
        }

        private static Image<TPixel> GetImageRegion<TPixel>(Image<TPixel> image, TexturePublishPart part, int? targetFrame = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var srcWidth = image.Width;
            var srcHeight = image.Height;

            if (srcWidth == 1 && srcHeight == 1) return image.Clone();
            
            var firstFrame = part.Frames.First();
            var frameCount = !targetFrame.HasValue ? part.Frames.Length : 1;
            var partWidth = (int)(firstFrame.SourceBounds.Width * srcWidth + 0.5d);
            var partHeight = (int)(firstFrame.SourceBounds.Height * srcHeight * frameCount + 0.5d);

            var regionImage = new Image<TPixel>(partWidth, partHeight);

            try {
                if (targetFrame.HasValue) {
                    var frame = part.Frames[targetFrame.Value];
                    var sourceX = (int) (frame.SourceBounds.Left * srcWidth);
                    var sourceY = (int) (frame.SourceBounds.Top * srcHeight);

                    var outBounds = new Rectangle(0, 0, regionImage.Width, regionImage.Height);
                    ImageProcessors.ImageProcessors.CopyRegion(image, sourceX, sourceY, regionImage, outBounds);
                }
                else {
                    foreach (var frame in part.Frames) {
                        var sourceX = (int) (frame.SourceBounds.Left * srcWidth);
                        var sourceY = (int) (frame.SourceBounds.Top * srcHeight);

                        var outBounds = frame.DestBounds.ScaleTo(partWidth, partHeight);
                        ImageProcessors.ImageProcessors.CopyRegion(image, sourceX, sourceY, regionImage, outBounds);
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
