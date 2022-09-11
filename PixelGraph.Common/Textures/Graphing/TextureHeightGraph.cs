using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures.Graphing
{
    public interface ITextureHeightGraph
    {
        int HeightFrameCount {get;}
        float HeightScaleFactor {get;}

        Task<Image<L16>> GetOrCreateAsync(CancellationToken token = default);
    }

    internal class TextureHeightGraph : ITextureHeightGraph, IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly ITextureGraphContext context;
        private Image<L16> heightTexture;
        private bool isLoaded;

        public int HeightFrameCount {get; private set;}
        public float HeightScaleFactor {get; private set;}


        public TextureHeightGraph (
            IServiceProvider provider,
            ITextureGraphContext context)
        {
            this.provider = provider;
            this.context = context;

            HeightFrameCount = 1;
        }

        public void Dispose()
        {
            heightTexture?.Dispose();
        }

        public async Task<Image<L16>> GetOrCreateAsync(CancellationToken token = default)
        {
            if (isLoaded) return heightTexture;
            isLoaded = true;

            HeightScaleFactor = 1f;

            // Try to compose from existing channels first
            var heightChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));
            var hasHeight = heightChannel?.HasMapping ?? false;

            if (!hasHeight) return null;

            using var scope = provider.CreateScope();
            var heightContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var builder = scope.ServiceProvider.GetRequiredService<ITextureBuilder>();

            // make image from normal X & Y; z if found
            heightContext.Project = context.Project;
            heightContext.Profile = context.Profile;
            heightContext.Material = context.Material;
            heightContext.IsImport = context.IsImport;
            heightContext.IsAnimated = context.IsAnimated;
            heightContext.ApplyPostProcessing = false;
            
            builder.InputChannels = new [] {heightChannel};

            builder.OutputChannels = new PackEncodingChannel[] {
                new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                    Invert = true,
                },
            };

            await builder.MapAsync(false, token);

            if (builder.HasMappedSources) {
                heightTexture = await builder.BuildAsync<L16>(false, null, token);

                if (heightTexture != null) {
                    HeightFrameCount = builder.FrameCount;

                    if (context.Profile != null) Resize();
                }
            }

            // WARN: FOR TESTING ONLY!!!
            //await heightTexture.SaveAsPngAsync("test-height.png", token);

            return heightTexture;
        }

        private void Resize()
        {
            var aspect = (float)heightTexture.Height / heightTexture.Width;
            var bufferSize = context.GetBufferSize(aspect);

            if (!bufferSize.HasValue || heightTexture.Width == bufferSize.Value.Width) return;

            HeightScaleFactor = (float)bufferSize.Value.Width / heightTexture.Width;
            var scaledWidth = (int)MathF.Ceiling(heightTexture.Width * HeightScaleFactor);
            var scaledHeight = (int)MathF.Ceiling(heightTexture.Height * HeightScaleFactor);

            var samplerName = context.Material?.Height?.Input?.Sampler
                              ?? context.Profile?.Encoding?.Height?.Sampler
                              ?? context.DefaultSampler;

            var sampler = context.CreateSampler(heightTexture, samplerName);
            sampler.RangeX = sampler.RangeY = 1f / HeightScaleFactor;

            var options = new ResizeProcessor<L16>.Options {
                Sampler = sampler,
            };

            var processor = new ResizeProcessor<L16>(options);
            var regions = provider.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = HeightFrameCount;
            regions.DestFrameCount = HeightFrameCount;

            Image<L16> heightCopy = null;
            try {
                heightCopy = heightTexture;
                heightTexture = new Image<L16>(Configuration.Default, scaledWidth, scaledHeight);

                foreach (var frame in regions.GetAllRenderRegions()) {
                    foreach (var tile in frame.Tiles) {
                        sampler.SetBounds(tile.SourceBounds);
                        var outBounds = tile.DestBounds.ScaleTo(scaledWidth, scaledHeight);
                        heightTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }
                }
            }
            finally {
                heightCopy?.Dispose();
            }
        }
    }
}
