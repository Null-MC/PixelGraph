using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Extensions;
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
using PixelGraph.Common.TextureFormats;

namespace PixelGraph.Common.Textures
{
    public interface ITextureOcclusionGraph
    {
        ResourcePackChannelProperties Channel {get;}
        int FrameCount {get;}
        int FrameWidth {get;}
        int FrameHeight {get;}
        bool HasTexture {get;}

        Task<Image<L8>> GetTextureAsync(CancellationToken token = default);
        Task<Image<L8>> GenerateAsync(CancellationToken token = default);
        Task<ISampler<L8>> GetSamplerAsync(CancellationToken token = default);
    }
    
    internal class TextureOcclusionGraph : ITextureOcclusionGraph, IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly ITextureGraphContext context;
        private readonly ITextureSourceGraph sourceGraph;
        private readonly ITextureRegionEnumerator regions;
        private Image<L8> texture;
        private bool isLoaded;

        public ResourcePackChannelProperties Channel {get; private set;}
        public int FrameCount {get; private set;}
        public int FrameWidth {get; private set;}
        public int FrameHeight {get; private set;}
        public bool HasTexture => texture != null;


        public TextureOcclusionGraph(
            IServiceProvider provider,
            IInputReader reader,
            ITextureGraphContext context,
            ITextureSourceGraph sourceGraph,
            ITextureRegionEnumerator regions)
        {
            this.provider = provider;
            this.reader = reader;
            this.context = context;
            this.sourceGraph = sourceGraph;
            this.regions = regions;

            FrameCount = 1;
        }

        public void Dispose()
        {
            texture?.Dispose();
        }

        public async Task<Image<L8>> GetTextureAsync(CancellationToken token = default)
        {
            if (isLoaded) return texture;
            isLoaded = true;

            var bufferChannel = new ResourcePackOcclusionChannelProperties {
                Color = ColorChannel.Red,
            };
            var (image, frames) = await ExtractInputAsync<L8>(EncodingChannel.Occlusion, bufferChannel, token);

            if (image != null) {
                texture = image;
                FrameCount = frames;

                var outputChannel = context.OutputEncoding.GetChannel(EncodingChannel.Occlusion);

                Channel = new ResourcePackOcclusionChannelProperties {
                    Sampler = outputChannel?.Sampler,
                    Color = ColorChannel.Red,
                    //Invert = true,
                };
            }

            if (context.AutoGenerateOcclusion && !context.IsImport)
                texture ??= await GenerateAsync(token);

            if (texture != null) {
                FrameWidth = texture.Width;
                FrameHeight = texture.Height;
                if (FrameCount > 1) FrameHeight /= FrameCount;
            }

            return texture;
        }

        public async Task<ISampler<L8>> GetSamplerAsync(CancellationToken token = default)
        {
            var occlusionTexture = await GetTextureAsync(token);
            if (occlusionTexture == null) return null;

            var samplerName = Channel?.Sampler ?? context.DefaultSampler;
            var sampler = context.CreateSampler(occlusionTexture, samplerName);

            // TODO: SET THESE PROPERLY!
            sampler.RangeX = 1f;
            sampler.RangeY = 1f;

            return sampler;
        }

        public async Task<Image<L8>> GenerateAsync(CancellationToken token = default)
        {
            var (heightImage, heightFrames, heightChannel) = await GetChannelTextureAsync<Rgba32>(EncodingChannel.Height, token);
            if (heightImage == null) return null;

            FrameCount = heightFrames;

            var aspect = (float)heightImage.Height / heightImage.Width;
            var bufferSize = context.GetBufferSize(aspect);

            var occlusionWidth = bufferSize?.Width ?? heightImage.Width;
            var occlusionHeight = bufferSize?.Height ?? heightImage.Height;

            var heightScale = 1f;
            if (bufferSize.HasValue)
                heightScale = (float)bufferSize.Value.Width / heightImage.Width;

            var heightSampler = context.CreateSampler(heightImage, Samplers.Samplers.Bilinear);

            heightSampler.RangeX = (float)heightImage.Width / occlusionWidth;
            heightSampler.RangeY = (float)heightImage.Height / occlusionHeight;

            var quality = (float) (context.Profile?.OcclusionQuality ?? ResourcePackProfileProperties.DefaultOcclusionQuality);
            var stepDistance = (float)(context.Material.Occlusion?.StepDistance ?? MaterialOcclusionProperties.DefaultStepDistance);
            var zScale = (float) (context.Material.Occlusion?.ZScale ?? MaterialOcclusionProperties.DefaultZScale);
            var zBias = (float) (context.Material.Occlusion?.ZBias ?? MaterialOcclusionProperties.DefaultZBias);
            var hitPower = (float)(context.Profile?.OcclusionPower ?? ResourcePackProfileProperties.DefaultOcclusionPower);

            // adjust volume height with texture scale
            zBias *= heightScale;
            zScale *= heightScale;

            var heightMapping = new TextureChannelMapping();
            heightMapping.ApplyInputChannel(heightChannel);
            heightMapping.OutputScale = (float)context.Material.GetChannelScale(EncodingChannel.Height);

            OcclusionProcessor<Rgba32>.Options options = null;
            OcclusionProcessor<Rgba32> processor = null;
            //OcclusionGpuProcessor<Rgba32>.Options gpuOptions = null;
            //OcclusionGpuProcessor<Rgba32> gpuProcessor = null;
            var enableGpu = false; //Gpu.IsSupported;

            if (enableGpu) {
                //gpuOptions = new OcclusionGpuProcessor<Rgba32>.Options {
                //    HeightImage = heightTexture,
                //    HeightMapping = heightMapping,
                //    Quality = quality,
                //    ZScale = zScale,
                //    ZBias = zBias,
                //};

                //gpuProcessor = new OcclusionGpuProcessor<Rgba32>(gpuOptions);
            }
            else {
                options = new OcclusionProcessor<Rgba32>.Options {
                    HeightSampler = heightSampler,
                    HeightMapping = heightMapping,
                    Quality = quality,
                    ZScale = zScale,
                    ZBias = zBias,
                    HitPower = hitPower,
                    Token = token,
                };

                // adjust volume height with texture scale
                options.ZBias *= heightScale;
                options.ZScale *= heightScale;

                processor = new OcclusionProcessor<Rgba32>(options);
            }

            var occlusionTexture = new Image<L8>(Configuration.Default, occlusionWidth, occlusionHeight);
            var outputChannel = context.OutputEncoding.GetChannel(EncodingChannel.Occlusion);

            Channel = new ResourcePackOcclusionChannelProperties {
                Sampler = outputChannel?.Sampler,
                Color = ColorChannel.Red,
                Invert = true,
            };

            try {
                foreach (var frame in regions.GetAllRenderRegions(null, FrameCount)) {
                    foreach (var tile in frame.Tiles) {
                        var outBounds = tile.Bounds.ScaleTo(occlusionWidth, occlusionHeight);

                        if (enableGpu) {
                            //gpuOptions.StepCount = (int)MathF.Max(outBounds.Width * stepDistance, 1f);
                            //gpuOptions.HeightWidth = (int)(tile.Bounds.Width * heightTexture.Width);
                            //gpuOptions.HeightHeight = (int)(tile.Bounds.Height * heightTexture.Height);

                            //gpuProcessor.Process(occlusionTexture, outBounds);
                        }
                        else {
                            var size = GetTileSize(in occlusionWidth);
                            options.StepCount = (int) MathF.Max(size * stepDistance, 1f);
                            heightSampler.Bounds = tile.Bounds;

                            occlusionTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                        }
                    }
                }

                return occlusionTexture;
            }
            catch (ImageProcessingException) when (token.IsCancellationRequested) {
                throw new OperationCanceledException();
            }
            catch {
                occlusionTexture.Dispose();
                throw;
            }
        }

        private async Task<(Image<TPixel> image, int frameCount)> ExtractInputAsync<TPixel>(string inputEncodingChannel, ResourcePackChannelProperties outputChannel, CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var inputChannel = context.InputEncoding.GetChannel(inputEncodingChannel);
            if (!(inputChannel?.HasMapping ?? false)) return (null, 0);

            using var scope = provider.CreateScope();
            var subContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var builder = scope.ServiceProvider.GetRequiredService<ITextureBuilder>();

            subContext.Input = context.Input;
            subContext.Profile = context.Profile;
            subContext.Material = context.Material;
            subContext.IsAnimated = context.IsAnimated;
            subContext.InputEncoding.Add(inputChannel);
            builder.InputChannels = new [] {inputChannel};
            builder.OutputChannels = new[] {outputChannel};

            await builder.MapAsync(false, token);
            if (!builder.HasMappedSources) return (null, 0);

            Image<TPixel> image = null;
            try {
                image = await builder.BuildAsync<TPixel>(false, token);
                return (image, builder.FrameCount);
            }
            catch {
                image?.Dispose();
                throw;
            }
        }

        private async Task<(Image<T> image, int frameCount, ResourcePackChannelProperties channel)> GetChannelTextureAsync<T>(string encodingChannel, CancellationToken token)
            where T : unmanaged, IPixel<T>
        {
            if (EncodingChannel.Is(encodingChannel, EncodingChannel.Bump)) {
                foreach (var file in reader.EnumerateInputTextures(context.Material, TextureTags.Bump)) {
                    if (file == null) continue;

                    var info = await sourceGraph.GetOrCreateAsync(file, token);
                    if (info == null) continue;

                    await using var stream = reader.Open(file);

                    var image = await Image.LoadAsync<T>(Configuration.Default, stream, token);
                    var channel = new ResourcePackBumpChannelProperties {
                        Color = ColorChannel.Red,
                        Invert = true,
                        Power = 1,
                    };
                    return (image, info.FrameCount, channel);
                }
            }
            else {
                foreach (var channel in context.InputEncoding.Where(c => EncodingChannel.Is(c.ID, encodingChannel))) {
                    if (!channel.HasTexture) continue;

                    foreach (var file in reader.EnumerateInputTextures(context.Material, channel.Texture)) {
                        if (file == null) continue;

                        var info = await sourceGraph.GetOrCreateAsync(file, token);
                        if (info == null) continue;

                        await using var stream = reader.Open(file);

                        var image = await Image.LoadAsync<T>(Configuration.Default, stream, token);
                        return (image, info.FrameCount, channel);
                    }
                }
            }

            return (null, 0, null);
        }

        private int GetTileSize(in int imageWidth)
        {
            var profileSize = context.Profile?.BlockTextureSize ?? context.Profile?.TextureSize;
            if (profileSize.HasValue) return profileSize.Value;

            if (CtmTypes.Is(CtmTypes.Compact, context.Material.CTM?.Type))
                return imageWidth / 5;
            
            if (CtmTypes.Is(CtmTypes.Full, context.Material.CTM?.Type))
                return imageWidth / 12;
            
            if (CtmTypes.Is(CtmTypes.Repeat, context.Material.CTM?.Type))
                return imageWidth / (context.Material.CTM?.CountX ?? 1);

            return imageWidth;
        }
    }
}
