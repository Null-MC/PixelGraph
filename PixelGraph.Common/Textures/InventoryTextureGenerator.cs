using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface IInventoryTextureGenerator
    {
        Task<Image<Rgba32>> CreateAsync(ITextureGraph graph, CancellationToken token = default);
    }

    internal class InventoryTextureGenerator : IInventoryTextureGenerator
    {
        private readonly ITextureGraphContext context;
        private readonly ITextureSourceGraph sourceGraph;
        private readonly ITextureNormalGraph normalGraph;
        private readonly ITextureOcclusionGraph occlusionGraph;
        private readonly ITextureRegionEnumerator regions;
        private readonly IInputReader reader;

        private Image<Rgba32> emissiveImage;


        public InventoryTextureGenerator(
            ITextureGraphContext context,
            ITextureSourceGraph sourceGraph,
            ITextureNormalGraph normalGraph,
            ITextureOcclusionGraph occlusionGraph,
            ITextureRegionEnumerator regions,
            IInputReader reader)
        {
            this.context = context;
            this.sourceGraph = sourceGraph;
            this.normalGraph = normalGraph;
            this.occlusionGraph = occlusionGraph;
            this.regions = regions;
            this.reader = reader;
        }

        public async Task<Image<Rgba32>> CreateAsync(ITextureGraph graph, CancellationToken token = default)
        {
            const int targetFrame = 0;

            Image<Rgba32> image = null;

            emissiveImage = null;
            try {
                await graph.MapAsync(TextureTags.Albedo, false, targetFrame, token);
                image = await graph.CreateImageAsync<Rgba32>(TextureTags.Albedo, false, token);
                if (image == null) return null;

                try {
                    await graph.PreBuildNormalTextureAsync(token);
                }
                catch (HeightSourceEmptyException) {}

                var emissiveChannel = context.InputEncoding.FirstOrDefault(c => TextureTags.Is(c.ID, TextureTags.Emissive));
                var emissiveInfo = await GetEmissiveInfoAsync(token);

                var inventoryOptions = new ItemProcessor<L8, Rgba32>.Options {
                    NormalSampler = normalGraph.GetNormalSampler(),
                    OcclusionSampler = await occlusionGraph.GetSamplerAsync(token),
                    OcclusionColor = occlusionGraph.Channel?.Color ?? ColorChannel.Red,
                };

                if (emissiveInfo != null) {
                    inventoryOptions.EmissiveColor = emissiveChannel?.Color ?? ColorChannel.Red;
                    inventoryOptions.EmissiveSampler = await GetEmissiveSamplerAsync(emissiveInfo.LocalFile, token);
                }

                if (inventoryOptions.NormalSampler != null || inventoryOptions.OcclusionSampler != null) {
                    var processor = new ItemProcessor<L8, Rgba32>(inventoryOptions);

                    foreach (var part in regions.GetAllPublishRegions(1)) {
                        var frame = part.Frames.FirstOrDefault();
                        if (frame == null) continue;

                        if (inventoryOptions.NormalSampler != null) {
                            var srcFrame = regions.GetPublishPartFrame(targetFrame, normalGraph.NormalFrameCount, part.TileIndex);
                            inventoryOptions.NormalSampler.Bounds = srcFrame.SourceBounds;
                        }

                        if (inventoryOptions.OcclusionSampler != null) {
                            var srcFrame = regions.GetPublishPartFrame(targetFrame, occlusionGraph.FrameCount, part.TileIndex);
                            inventoryOptions.OcclusionSampler.Bounds = srcFrame.SourceBounds;
                        }

                        if (emissiveChannel != null && emissiveInfo != null) {
                            var srcFrame = regions.GetPublishPartFrame(targetFrame, emissiveInfo.FrameCount, part.TileIndex);
                            inventoryOptions.EmissiveSampler.Bounds = srcFrame.SourceBounds;
                        }

                        var outBounds = frame.SourceBounds.ScaleTo(image.Width, image.Height);
                        image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }
                }

                // Make image square
                if (image.Width != image.Height) {
                    var size = Math.Max(image.Width, image.Height);
                    var temp = new Image<Rgba32>(size, size);

                    try {
                        var copyOptions = new CopyRegionProcessor<Rgba32>.Options {
                            SourceImage = image,
                            SourceX = 0,
                            SourceY = 0,
                        };

                        var outBounds = new Rectangle(
                            (size - image.Width) / 2,
                            (size - image.Height) / 2,
                            image.Width, image.Height);

                        var processor = new CopyRegionProcessor<Rgba32>(copyOptions);
                        temp.Mutate(c => c.ApplyProcessor(processor, outBounds));

                        image.Dispose();
                        image = temp;
                    }
                    catch {
                        temp.Dispose();
                    }
                }

                return image;
            }
            catch {
                image?.Dispose();
                throw;
            }
            finally {
                emissiveImage?.Dispose();
            }
        }

        private Task<TextureSource> GetEmissiveInfoAsync(CancellationToken token = default)
        {
            var emissiveFile = reader.EnumerateTextures(context.Material, TextureTags.Emissive).FirstOrDefault();
            if (emissiveFile == null) return Task.FromResult<TextureSource>(null);

            return sourceGraph.GetOrCreateAsync(emissiveFile, token);
        }

        private async Task<ISampler<Rgba32>> GetEmissiveSamplerAsync(string emissiveFile, CancellationToken token)
        {
            await using var stream = reader.Open(emissiveFile);
            emissiveImage = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

            var samplerName = context.Profile?.Encoding?.Emissive?.Sampler ?? context.DefaultSampler;
            var sampler = context.CreateSampler(emissiveImage, samplerName);

            // TODO: SET THESE PROPERLY!
            sampler.RangeX = 1f;
            sampler.RangeY = 1f;

            return sampler;
        }
    }
}
