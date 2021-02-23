using System.Linq;
using PixelGraph.Common.ImageProcessors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.IO;
using PixelGraph.Common.Samplers;

namespace PixelGraph.Common.Textures
{
    public interface IItemGenerator
    {
        MaterialContext Context {get; set;}

        Task<Image<Rgba32>> CreateAsync(ITextureGraph graph, CancellationToken token = default);
    }

    internal class ItemGenerator : IItemGenerator
    {
        private readonly IInputReader reader;

        public MaterialContext Context {get; set;}


        public ItemGenerator(IInputReader reader)
        {
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

                var options = new ItemProcessor<Rgb24, Rgb24, Rgba32>.Options {
                    NormalImage = Context.NormalTexture,
                    OcclusionImage = await graph.GetOrCreateOcclusionAsync(token),
                };

                var emissiveFile = reader.EnumerateTextures(Context.Material, TextureTags.Emissive).FirstOrDefault();

                if (emissiveFile != null) {
                    await using var stream = reader.Open(emissiveFile);
                    emissiveImage = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                    var sampler = Sampler<Rgba32>.Create(Sampler.Nearest);
                    sampler.Image = emissiveImage;
                    sampler.WrapX = Context.WrapX;
                    sampler.WrapY = Context.WrapY;

                    // TODO: SET THESE PROPERLY!
                    sampler.RangeX = 1f;
                    sampler.RangeY = 1f;

                    options.EmissiveSampler = sampler;
                    options.EmissiveColor = Context.InputEncoding
                        .FirstOrDefault(c => TextureTags.Is(c.ID, TextureTags.Emissive))?
                        .Color ?? ColorChannel.Red;
                }

                if (options.NormalImage != null || options.OcclusionImage != null) {
                    var processor = new ItemProcessor<Rgb24, Rgb24, Rgba32>(options);
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
