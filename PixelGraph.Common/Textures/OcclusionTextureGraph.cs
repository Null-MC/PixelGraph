using PixelGraph.Common.Encoding;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
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
    public interface IOcclusionTextureGraph
    {
        ResourcePackChannelProperties Channel {get;}

        Task<Image<Rgba32>> GetTextureAsync(CancellationToken token = default);
        Task<ISampler<Rgba32>> GetSamplerAsync(CancellationToken token = default);
        Task ApplyMagnitudeAsync(Image normalImage, ResourcePackChannelProperties magnitudeChannel, CancellationToken token = default);
        Task<Image<Rgba32>> GenerateAsync(CancellationToken token = default);
    }
    
    internal class OcclusionTextureGraph : IOcclusionTextureGraph, IDisposable
    {
        private readonly IInputReader reader;
        private readonly ITextureGraphContext context;
        private Image<Rgba32> texture;
        private bool isLoaded;

        public ResourcePackChannelProperties Channel {get; private set;}


        public OcclusionTextureGraph(
            IInputReader reader,
            ITextureGraphContext context)
        {
            this.reader = reader;
            this.context = context;
        }

        public void Dispose()
        {
            texture?.Dispose();
        }

        public async Task<Image<Rgba32>> GetTextureAsync(CancellationToken token = default)
        {
            if (isLoaded) return texture;
            isLoaded = true;

            var occlusionChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Occlusion));

            // TODO: load file
            var occlusionFile = reader.EnumerateTextures(context.Material, TextureTags.Occlusion).FirstOrDefault();

            if (occlusionFile != null) {
                await using var stream = reader.Open(occlusionFile);
                texture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                Channel = occlusionChannel;
            }

            if (texture == null) {
                var heightChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));
                if (heightChannel == null) return null;

                var heightFile = reader.EnumerateTextures(context.Material, heightChannel.Texture).FirstOrDefault();
                if (heightFile == null) return null;

                await using var stream = reader.Open(heightFile);
                using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                var aspect = (float)heightTexture.Height / heightTexture.Width;
                var bufferSize = context.GetBufferSize(aspect);

                var occlusionWidth = bufferSize?.Width ?? heightTexture.Width;
                var occlusionHeight = bufferSize?.Height ?? heightTexture.Height;

                var heightScale = 1f;
                if (bufferSize.HasValue)
                    heightScale = (float)bufferSize.Value.Width / heightTexture.Width;

                var samplerName = heightChannel.Sampler ?? context.DefaultSampler;
                var heightSampler = Sampler<Rgba32>.Create(samplerName);
                heightSampler.Image = heightTexture;
                heightSampler.WrapX = context.WrapX;
                heightSampler.WrapY = context.WrapY;

                heightSampler.RangeX = (float)heightTexture.Width / occlusionWidth;
                heightSampler.RangeY = (float)heightTexture.Height / occlusionHeight;

                texture = GenerateImage(occlusionWidth, occlusionHeight, heightSampler, heightChannel, heightScale);

                Channel = new ResourcePackOcclusionChannelProperties {
                    //Texture = TextureTags.Occlusion,
                    Sampler = occlusionChannel?.Sampler,
                    Color = ColorChannel.Red,
                    Invert = true,
                };
            }

            return texture;
        }

        public async Task<ISampler<Rgba32>> GetSamplerAsync(CancellationToken token = default)
        {
            var occlusionTexture = await GetTextureAsync(token);
            if (occlusionTexture == null) return null;

            var samplerName = Channel?.Sampler ?? context.DefaultSampler;
            var sampler = Sampler<Rgba32>.Create(samplerName);
            sampler.Image = occlusionTexture;
            sampler.WrapX = context.WrapX;
            sampler.WrapY = context.WrapY;

            // TODO: SET THESE PROPERLY!
            sampler.RangeX = 1f;
            sampler.RangeY = 1f;

            return sampler;
        }

        public async Task<Image<Rgba32>> GenerateAsync(CancellationToken token = default)
        {
            var heightChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));
            if (heightChannel == null) return null;

            var heightFile = reader.EnumerateTextures(context.Material, heightChannel.Texture).FirstOrDefault();
            if (heightFile == null) return null;

            await using var stream = reader.Open(heightFile);
            using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

            var aspect = (float)heightTexture.Height / heightTexture.Width;
            var bufferSize = context.GetBufferSize(aspect);

            var occlusionWidth = bufferSize?.Width ?? heightTexture.Width;
            var occlusionHeight = bufferSize?.Height ?? heightTexture.Height;

            var heightScale = 1f;
            if (bufferSize.HasValue)
                heightScale = (float)bufferSize.Value.Width / heightTexture.Width;

            var samplerName = heightChannel.Sampler ?? context.DefaultSampler;
            var heightSampler = Sampler<Rgba32>.Create(samplerName);
            heightSampler.Image = heightTexture;
            heightSampler.WrapX = context.WrapX;
            heightSampler.WrapY = context.WrapY;

            heightSampler.RangeX = (float)heightTexture.Width / occlusionWidth;
            heightSampler.RangeY = (float)heightTexture.Height / occlusionHeight;

            return GenerateImage(occlusionWidth, occlusionHeight, heightSampler, heightChannel, heightScale);
        }

        public async Task ApplyMagnitudeAsync(Image normalImage, ResourcePackChannelProperties magnitudeChannel, CancellationToken token = default)
        {
            var options = new NormalMagnitudeProcessor<Rgba32>.Options {
                MagSource = await GetTextureAsync(token),
                Scale = context.Material.GetChannelScale(magnitudeChannel.ID),
                InputChannel = Channel?.Color ?? ColorChannel.Red,
                InputMinValue = (float?)Channel?.MinValue ?? 0f,
                InputMaxValue = (float?)Channel?.MaxValue ?? 1f,
                InputRangeMin = Channel?.RangeMin ?? 0,
                InputRangeMax = Channel?.RangeMax ?? 255,
                InputPower = (float?)Channel?.Power ?? 1f,
                InputInvert = Channel?.Invert ?? false,
            };

            if (options.MagSource == null) return;
            options.ApplyOutputChannel(magnitudeChannel);

            var processor = new NormalMagnitudeProcessor<Rgba32>(options);
            normalImage.Mutate(c => c.ApplyProcessor(processor));
        }

        private Image<Rgba32> GenerateImage<THeight>(int width, int height, ISampler<THeight> heightSampler, ResourcePackChannelProperties heightChannel, float heightScale)
            where THeight : unmanaged, IPixel<THeight>
        {
            if (heightSampler == null) throw new ArgumentNullException(nameof(heightSampler));
            if (heightChannel == null) throw new ArgumentNullException(nameof(heightChannel));

            var options = new OcclusionProcessor<THeight>.Options {
                HeightSampler = heightSampler,
                HeightChannel = heightChannel.Color ?? ColorChannel.Red,
                HeightMinValue = (float?)heightChannel.MinValue ?? 0f,
                HeightMaxValue = (float?)heightChannel.MaxValue ?? 1f,
                HeightRangeMin = heightChannel.RangeMin ?? 0,
                HeightRangeMax = heightChannel.RangeMax ?? 255,
                HeightShift = heightChannel.Shift ?? 0,
                HeightPower = (float?)heightChannel.Power ?? 0f,
                HeightInvert = heightChannel.Invert ?? false,

                StepDistance = (float?)context.Material.Occlusion?.StepDistance ?? MaterialOcclusionProperties.DefaultStepDistance,
                Quality = (float?)context.Material.Occlusion?.Quality ?? MaterialOcclusionProperties.DefaultQuality,
                ZScale = (float?)context.Material.Occlusion?.ZScale ?? MaterialOcclusionProperties.DefaultZScale,
                ZBias = (float?)context.Material.Occlusion?.ZBias ?? MaterialOcclusionProperties.DefaultZBias,
            };

            // adjust volume height with texture scale
            options.ZBias *= heightScale;
            options.ZScale *= heightScale;

            var processor = new OcclusionProcessor<THeight>(options);

            var occlusionTexture = new Image<Rgba32>(Configuration.Default, width, height);

            try {
                occlusionTexture.Mutate(c => c.ApplyProcessor(processor));
                return occlusionTexture;
            }
            catch {
                occlusionTexture.Dispose();
                throw;
            }
        }
    }
}
