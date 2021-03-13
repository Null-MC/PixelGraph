using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
        private readonly IInputReader reader;

        private Image<Rgba32> emissiveImage;


        public InventoryTextureGenerator(
            ITextureGraphContext context,
            ITextureSourceGraph sourceGraph,
            ITextureNormalGraph normalGraph,
            ITextureOcclusionGraph occlusionGraph,
            IInputReader reader)
        {
            this.context = context;
            this.sourceGraph = sourceGraph;
            this.normalGraph = normalGraph;
            this.occlusionGraph = occlusionGraph;
            this.reader = reader;
        }

        public async Task<Image<Rgba32>> CreateAsync(ITextureGraph graph, CancellationToken token = default)
        {
            Image<Rgba32> image = null;

            emissiveImage = null;
            try {
                await graph.MapAsync(TextureTags.Albedo, false, 0, token);
                image = await graph.CreateImageAsync<Rgba32>(TextureTags.Albedo, false, token);
                if (image == null) return null;

                try {
                    await graph.PreBuildNormalTextureAsync(token);
                }
                catch (HeightSourceEmptyException) {}

                var options = new ItemProcessor<Rgba32, Rgba32>.Options {
                    NormalSampler = GetNormalSampler(),
                    OcclusionSampler = await GetOcclusionSamplerAsync(token),
                    OcclusionColor = occlusionGraph.Channel?.Color ?? ColorChannel.Red,
                };

                var emissiveChannel = context.InputEncoding.FirstOrDefault(c => TextureTags.Is(c.ID, TextureTags.Emissive));

                if (emissiveChannel != null) {
                    options.EmissiveSampler = await GetEmissiveSamplerAsync(emissiveChannel, token);
                    options.EmissiveColor = emissiveChannel.Color ?? ColorChannel.Red;
                }

                if (options.NormalSampler != null || options.OcclusionSampler != null) {
                    var processor = new ItemProcessor<Rgba32, Rgba32>(options);
                    image.Mutate(c => c.ApplyProcessor(processor));
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

        private ISampler<Rgb24> GetNormalSampler()
        {
            if (normalGraph.Texture == null) return null;

            var sampler = Sampler<Rgb24>.Create(Samplers.Samplers.Nearest);

            sampler.Image = normalGraph.Texture;
            sampler.WrapX = context.MaterialWrapX;
            sampler.WrapY = context.MaterialWrapY;
            sampler.FrameCount = normalGraph.FrameCount;
            sampler.Frame = 0;

            // TODO: SET THESE PROPERLY!
            sampler.RangeX = 1f;
            sampler.RangeY = 1f;

            return sampler;
        }

        private async Task<ISampler<Rgba32>> GetOcclusionSamplerAsync(CancellationToken token)
        {
            var sampler = await occlusionGraph.GetSamplerAsync(token);
            if (sampler == null) return null;

            sampler.FrameCount = occlusionGraph.FrameCount;
            sampler.Frame = 0;

            return sampler;
        }

        private async Task<ISampler<Rgba32>> GetEmissiveSamplerAsync(ResourcePackChannelProperties emissiveChannel, CancellationToken token)
        {
            var emissiveFile = reader.EnumerateTextures(context.Material, TextureTags.Emissive).FirstOrDefault();
            if (emissiveFile == null) return null;

            var emissiveInfo = await sourceGraph.GetOrCreateAsync(emissiveFile, token);
            if (emissiveInfo == null) return null;

            await using var stream = reader.Open(emissiveInfo.LocalFile);
            emissiveImage = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

            var samplerName = emissiveChannel.Sampler ?? context.DefaultSampler;
            var sampler = Sampler<Rgba32>.Create(samplerName);
            sampler.Image = emissiveImage;
            sampler.WrapX = context.MaterialWrapX;
            sampler.WrapY = context.MaterialWrapY;
            sampler.FrameCount = 1; // TODO: !
            sampler.Frame = 0;

            // TODO: SET THESE PROPERLY!
            sampler.RangeX = 1f;
            sampler.RangeY = 1f;

            return sampler;
        }
    }
}
