using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
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
        //Image<L16> HeightTexture {get;}
        int HeightFrameCount {get;}
        //int HeightFrameWidth {get;}
        //int HeightFrameHeight {get;}
        float HeightScaleFactor {get;}

        Task<Image<L16>> GetOrCreateAsync(CancellationToken token = default);
    }

    internal class TextureHeightGraph : ITextureHeightGraph, IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly ITextureGraphContext context;
        private readonly ITextureRegionEnumerator regions;
        private bool isLoaded;

        public Image<L16> HeightTexture {get; private set;}
        public int HeightFrameCount {get; private set;}
        //public int HeightFrameWidth {get; private set;}
        //public int HeightFrameHeight {get; private set;}
        public float HeightScaleFactor {get; private set;}


        public TextureHeightGraph (
            IServiceProvider provider,
            ITextureGraphContext context,
            ITextureRegionEnumerator regions)
        {
            this.provider = provider;
            this.context = context;
            this.regions = regions;

            HeightFrameCount = 1;
        }

        public void Dispose()
        {
            HeightTexture?.Dispose();
        }

        public async Task<Image<L16>> GetOrCreateAsync(CancellationToken token = default)
        {
            if (isLoaded) return HeightTexture;
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
            heightContext.Input = context.Input;
            heightContext.Profile = context.Profile;
            heightContext.Material = context.Material;
            heightContext.IsImport = context.IsImport;
            heightContext.ApplyPostProcessing = false;
            
            builder.InputChannels = new [] {heightChannel};

            builder.OutputChannels = new ResourcePackChannelProperties[] {
                new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                    Invert = true,
                },
            };

            await builder.MapAsync(false, token);

            if (builder.HasMappedSources) {
                HeightTexture = await builder.BuildAsync<L16>(false, null, token);

                if (HeightTexture != null) {
                    HeightFrameCount = builder.FrameCount;

                    if (context.Profile != null) Resize();
                }
            }

            return HeightTexture;
        }

        private void Resize()
        {
            var aspect = (float)HeightTexture.Height / HeightTexture.Width;
            var bufferSize = context.GetBufferSize(aspect);

            if (!bufferSize.HasValue || HeightTexture.Width == bufferSize.Value.Width) return;

            HeightScaleFactor = (float)bufferSize.Value.Width / HeightTexture.Width;
            var scaledWidth = (int)MathF.Ceiling(HeightTexture.Width * HeightScaleFactor);
            var scaledHeight = (int)MathF.Ceiling(HeightTexture.Height * HeightScaleFactor);

            var samplerName = context?.Material?.Height?.Input?.Sampler
                              ?? context.Profile?.Encoding?.Height?.Sampler
                              ?? context.DefaultSampler;

            var sampler = context.CreateSampler(HeightTexture, samplerName);
            sampler.Bounds = new RectangleF(0f, 0f, 1f, 1f);
            sampler.RangeX = sampler.RangeY = 1f / HeightScaleFactor;

            var options = new ResizeProcessor<L16>.Options {
                Sampler = sampler,
            };

            var processor = new ResizeProcessor<L16>(options);

            Image<L16> heightCopy = null;
            try {
                heightCopy = HeightTexture;
                HeightTexture = new Image<L16>(Configuration.Default, scaledWidth, scaledHeight);

                foreach (var frame in regions.GetAllRenderRegions(null, HeightFrameCount)) {
                    foreach (var tile in frame.Tiles) {
                        sampler.Bounds = tile.Bounds;

                        var outBounds = tile.Bounds.ScaleTo(scaledWidth, scaledHeight);
                        HeightTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }
                }
            }
            finally {
                heightCopy?.Dispose();
            }
        }
    }
}
