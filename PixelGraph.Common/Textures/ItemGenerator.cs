using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface IItemGenerator
    {
        Task<Image<Rgba32>> CreateAsync(ITextureGraph graph, CancellationToken token = default);
    }

    internal class ItemGenerator : IItemGenerator
    {
        private readonly ITextureGraphContext context;
        private readonly INormalTextureGraph normalGraph;
        private readonly IOcclusionTextureGraph occlusionGraph;
        private readonly IInputReader reader;


        public ItemGenerator(
            ITextureGraphContext context,
            INormalTextureGraph normalGraph,
            IOcclusionTextureGraph occlusionGraph,
            IInputReader reader)
        {
            this.context = context;
            this.normalGraph = normalGraph;
            this.occlusionGraph = occlusionGraph;
            this.reader = reader;
        }

        public async Task<Image<Rgba32>> CreateAsync(ITextureGraph graph, CancellationToken token = default)
        {
            Image<Rgba32> image = null;

            // Return inventory texture if exists
            try {
                image = await graph.CreateImageAsync<Rgba32>(TextureTags.Inventory, false, token);
                if (image != null) return image;
            }
            catch {
                image?.Dispose();
                throw;
            }

            // Return diffuse if exists
            try {
                // TODO: ADD SUPPORT FOR ALPHA!
                image = await graph.CreateImageAsync<Rgba32>(TextureTags.Diffuse, false, token);
                if (image != null) return image;
            }
            catch {
                image?.Dispose();
                throw;
            }

            // Otherwise build from albedo, normal, occlusion, emissive
            Image<Rgba32> emissiveImage = null;
            try {
                image = await graph.CreateImageAsync<Rgba32>(TextureTags.Albedo, false, token);
                if (image == null) return null;

                try {
                    await graph.PreBuildNormalTextureAsync(token);
                }
                catch (HeightSourceEmptyException) {}

                var options = new ItemProcessor<Rgb24, Rgba32, Rgba32>.Options {
                    NormalImage = normalGraph.Texture,
                    OcclusionSampler = await occlusionGraph.GetSamplerAsync(token),
                    OcclusionColor = occlusionGraph.Channel?.Color ?? ColorChannel.Red,
                };

                var emissiveChannel = context.InputEncoding.FirstOrDefault(c => TextureTags.Is(c.ID, TextureTags.Emissive));
                var emissiveFile = reader.EnumerateTextures(context.Material, TextureTags.Emissive).FirstOrDefault();

                if (emissiveFile != null) {
                    await using var stream = reader.Open(emissiveFile);
                    emissiveImage = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                    var samplerName = emissiveChannel?.Sampler ?? context.DefaultSampler;
                    var sampler = Sampler<Rgba32>.Create(samplerName);
                    sampler.Image = emissiveImage;
                    sampler.WrapX = context.WrapX;
                    sampler.WrapY = context.WrapY;

                    // TODO: SET THESE PROPERLY!
                    sampler.RangeX = 1f;
                    sampler.RangeY = 1f;

                    options.EmissiveSampler = sampler;
                    options.EmissiveColor = emissiveChannel?.Color ?? ColorChannel.Red;
                }

                if (options.NormalImage != null || options.OcclusionSampler != null) {
                    var processor = new ItemProcessor<Rgb24, Rgba32, Rgba32>(options);
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
    }
}
