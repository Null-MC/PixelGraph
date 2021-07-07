using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureNormalGraph
    {
        Image<Rgb24> NormalTexture {get;}
        int NormalFrameCount {get;}
        int NormalFrameWidth {get;}
        int NormalFrameHeight {get;}
        bool HasNormalTexture {get;}

        Image<L8> MagnitudeTexture {get;}
        int MagnitudeFrameCount {get;}
        int MagnitudeFrameWidth {get;}
        int MagnitudeFrameHeight {get;}
        bool HasMagnitudeTexture {get;}

        Task<bool> TryBuildNormalMapAsync(CancellationToken token = default);
        Task<Image<Rgb24>> GenerateAsync(CancellationToken token = default);

        ISampler<Rgb24> GetNormalSampler();
        ISampler<L8> GetMagnitudeSampler();
    }

    internal class TextureNormalGraph : ITextureNormalGraph, IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly ITextureGraphContext context;
        private readonly ITextureSourceGraph sourceGraph;
        private readonly ITextureRegionEnumerator regions;
        private readonly IInputReader reader;
        private readonly ILogger logger;

        public Image<Rgb24> NormalTexture {get; private set;}
        public int NormalFrameCount {get; private set;}
        public int NormalFrameWidth {get; private set;}
        public int NormalFrameHeight {get; private set;}
        public bool HasNormalTexture => NormalTexture != null;

        public Image<L8> MagnitudeTexture {get; private set;}
        public int MagnitudeFrameCount {get; private set;}
        public int MagnitudeFrameWidth {get; private set;}
        public int MagnitudeFrameHeight {get; private set;}
        public bool HasMagnitudeTexture => MagnitudeTexture != null;


        public TextureNormalGraph(
            ILogger<TextureNormalGraph> logger,
            IServiceProvider provider,
            ITextureGraphContext context,
            ITextureSourceGraph sourceGraph,
            ITextureRegionEnumerator regions,
            IInputReader reader)
        {
            this.provider = provider;
            this.context = context;
            this.sourceGraph = sourceGraph;
            this.regions = regions;
            this.reader = reader;
            this.logger = logger;

            NormalFrameCount = 1;
            MagnitudeFrameCount = 1;
        }

        public void Dispose()
        {
            MagnitudeTexture?.Dispose();
            NormalTexture?.Dispose();
        }

        public async Task<bool> TryBuildNormalMapAsync(CancellationToken token = default)
        {
            // Try to compose from existing channels first
            var normalXChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalX));
            var normalYChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalY));
            var normalZChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalZ));

            var hasNormalX = normalXChannel?.HasMapping ?? false;
            var hasNormalY = normalYChannel?.HasMapping ?? false;
            var hasNormalZ = normalZChannel?.HasMapping ?? false;

            if (hasNormalX && hasNormalY) {
                using var scope = provider.CreateScope();
                var normalContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                var builder = scope.ServiceProvider.GetRequiredService<ITextureBuilder>();

                // make image from normal X & Y; z if found
                normalContext.Input = context.Input;
                normalContext.Profile = context.Profile;
                normalContext.Material = context.Material;
                normalContext.IsImport = context.IsImport;
                
                builder.InputChannels = new [] {normalXChannel, normalYChannel, normalZChannel}
                    .Where(x => x != null).ToArray();

                builder.OutputChannels = new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Red,
                    },
                    new ResourcePackNormalYChannelProperties {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Green,
                    },
                    new ResourcePackNormalZChannelProperties {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Blue,
                    },
                };

                await builder.MapAsync(false, token);
                if (builder.HasMappedSources) {
                    NormalTexture = await builder.BuildAsync<Rgb24>(false, token);

                    if (NormalTexture != null) {
                        NormalFrameCount = builder.FrameCount;
                        MagnitudeFrameCount = builder.FrameCount;

                        TryExtractMagnitude();
                    }
                }
            }

            var autoGenNormal = context.Profile?.AutoGenerateNormal
                ?? ResourcePackProfileProperties.AutoGenerateNormalDefault;

            if (NormalTexture == null && autoGenNormal && !context.IsImport)
                NormalTexture = await GenerateAsync(token);

            if (NormalTexture == null) return false;

            NormalFrameWidth = NormalTexture.Width;
            NormalFrameHeight = NormalTexture.Height;
            if (NormalFrameCount > 1) NormalFrameHeight /= NormalFrameCount;

            var options = new NormalRotateProcessor.Options {
                RestoreNormalZ = !hasNormalZ,
                CurveX = (float?)context.Material.Normal?.CurveX ?? 0f,
                CurveY = (float?)context.Material.Normal?.CurveY ?? 0f,
                Noise = (float?)context.Material.Normal?.Noise ?? 0f,
            };

            var processor = new NormalRotateProcessor(options);
            NormalTexture.Mutate(c => c.ApplyProcessor(processor));

            // Apply filtering
            if (context.Material.Filters != null)
                ApplyFiltering();

            // apply magnitude channels
            var magnitudeOutputChannels = context.OutputEncoding
                .Where(c => TextureTags.Is(c.Texture, TextureTags.Normal))
                .Where(c => c.Color == ColorChannel.Magnitude).ToArray();

            if (magnitudeOutputChannels.Length > 1) {
                logger.LogWarning("Only a single encoding channel can be mapped to normal-magnitude! using first mapping. material={DisplayName}", context.Material.DisplayName);
            }

            var magnitudeOutputChannel = magnitudeOutputChannels.FirstOrDefault();
            if (magnitudeOutputChannel != null) await ApplyMagnitudeAsync(magnitudeOutputChannel, token);

            return true;
        }

        public Task<Image<Rgb24>> GenerateAsync(CancellationToken token = default)
        {
            return GenerateAsync<Rgba64>(token);
        }

        public async Task<Image<Rgb24>> GenerateAsync<THeight>(CancellationToken token = default)
            where THeight : unmanaged, IPixel<THeight>
        {
            logger.LogInformation("Generating normal map for texture {DisplayName}.", context.Material.DisplayName);

            var heightChannelIn = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));

            if (heightChannelIn == null || !heightChannelIn.HasMapping)
                throw new HeightSourceEmptyException("No height sources mapped!");

            Image<THeight> heightTexture = null;

            try {
                float scale;
                (heightTexture, scale) = await LoadHeightTextureAsync<THeight>(token);

                if (heightTexture == null) {
                    var up = new Rgb24(127, 127, 255);
                    var size = context.GetBufferSize(1f);
                    return new Image<Rgb24>(Configuration.Default, size?.Width ?? 1, size?.Height ?? 1, up);
                }

                if (!NormalMapMethod.TryParse(context.Material.Normal?.Method, out var normalMethod))
                    normalMethod = NormalMapMethods.Sobel3;

                var builder = new NormalMapBuilder<THeight>(regions) {
                    HeightImage = heightTexture,
                    HeightChannel = heightChannelIn.Color ?? ColorChannel.None,
                    Strength = (float)(context.Material.Normal?.Strength ?? MaterialNormalProperties.DefaultStrength),
                    Method = normalMethod,
                    WrapX = context.MaterialWrapX,
                    WrapY = context.MaterialWrapY,

                    // TODO: testing
                    VarianceStrength = 0.998f,
                    LowFreqDownscale = 4,
                    VarianceBlur = 3f,
                };

                // WARN: testing, scale strength by resolution scaling to preserve slope
                builder.Strength *= scale;

                // WARN: temporary hard-coded
                builder.LowFreqStrength = builder.Strength / 4f;

                return builder.Build(NormalFrameCount);
            }
            finally {
                heightTexture?.Dispose();
            }
        }

        public ISampler<Rgb24> GetNormalSampler()
        {
            return NormalTexture == null ? null : context.CreateSampler(NormalTexture, Samplers.Samplers.Nearest);
        }

        public ISampler<L8> GetMagnitudeSampler()
        {
            return MagnitudeTexture == null ? null : context.CreateSampler(MagnitudeTexture, Samplers.Samplers.Nearest);
        }

        private void ApplyFiltering()
        {
            foreach (var filter in context.Material.Filters) {
                filter.GetRectangle(NormalTexture.Width, NormalTexture.Height, out var region);

                if (filter.HasNormalRotation) {
                    var curveOptions = new NormalRotateProcessor.Options {
                        CurveX = (float?)filter.NormalCurveX ?? 0f,
                        CurveY = (float?)filter.NormalCurveY ?? 0f,
                        Noise = (float?)filter.NormalNoise ?? 0f,
                    };

                    var curveProcessor = new NormalRotateProcessor(curveOptions);
                    NormalTexture.Mutate(c => c.ApplyProcessor(curveProcessor, region));
                }

                //...
            }
        }

        private void TryExtractMagnitude()
        {
            var magnitudeInputChannels = context.InputEncoding
                .Where(c => TextureTags.Is(c.Texture, TextureTags.Normal))
                .Where(c => c.Color == ColorChannel.Magnitude).ToArray();

            if (magnitudeInputChannels.Length > 1) {
                logger.LogWarning("Only a single encoding channel can be mapped to normal-magnitude! using first mapping. material={DisplayName}", context.Material.DisplayName);
            }

            var magnitudeInputChannel = magnitudeInputChannels.FirstOrDefault();
            if (magnitudeInputChannel == null) return;

            var mapping = new TextureChannelMapping {
                OutputColor = ColorChannel.Red,
            };

            mapping.ApplyInputChannel(magnitudeInputChannel);

            var options = new NormalMagnitudeReadProcessor<Rgb24>.Options {
                Mapping = new PixelMapping(mapping),
                NormalTexture = NormalTexture,
            };

            MagnitudeTexture = new Image<L8>(NormalTexture.Width, NormalTexture.Height);

            var processor = new NormalMagnitudeReadProcessor<Rgb24>(options);
            MagnitudeTexture.Mutate(c => c.ApplyProcessor(processor));

            MagnitudeFrameWidth = MagnitudeTexture.Width;
            MagnitudeFrameHeight = MagnitudeTexture.Height;
            if (MagnitudeFrameCount > 1) MagnitudeFrameHeight /= MagnitudeFrameCount;
        }

        private async Task ApplyMagnitudeAsync(ResourcePackChannelProperties magnitudeChannel, CancellationToken token)
        {
            var inputChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, magnitudeChannel.ID));
            if (inputChannel == null) return;

            var mapping = new TextureChannelMapping {
                ValueShift = (float) context.Material.GetChannelShift(magnitudeChannel.ID),
                OutputScale = (float) context.Material.GetChannelScale(magnitudeChannel.ID),
            };

            mapping.ApplyInputChannel(inputChannel);
            mapping.ApplyOutputChannel(magnitudeChannel);

            var options = new NormalMagnitudeWriteProcessor<L8>.Options {
                Scale = (float)context.Material.GetChannelScale(magnitudeChannel.ID),
                Mapping = new PixelMapping(mapping),
            };

            int srcFrameCount;
            if (EncodingChannel.Is(magnitudeChannel.ID, EncodingChannel.Occlusion)) {
                var occlusionGraph = provider.GetRequiredService<ITextureOcclusionGraph>();

                options.MagSampler = await occlusionGraph.GetSamplerAsync(token);
                if (options.MagSampler == null) return;

                srcFrameCount = occlusionGraph.FrameCount;

                // TODO: set these properly
                options.MagSampler.RangeX = 1f;
                options.MagSampler.RangeY = 1f;
            }
            else {
                throw new SourceEmptyException("No sources found for applying normal magnitude!");
            }

            var processor = new NormalMagnitudeWriteProcessor<L8>(options);

            foreach (var frame in regions.GetAllRenderRegions(null, NormalFrameCount)) {
                foreach (var part in frame.Tiles) {
                    var srcPart = regions.GetRenderRegion(frame.Index, srcFrameCount);
                    options.MagSampler.Bounds = srcPart.Tiles[part.Index].Bounds;

                    var outBounds = part.Bounds.ScaleTo(NormalTexture.Width, NormalTexture.Height);
                    NormalTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }

        private async Task<TextureSource> GetHeightSourceAsync(CancellationToken token)
        {
            var file = reader.EnumerateInputTextures(context.Material, TextureTags.Bump).FirstOrDefault();

            if (file != null) {
                var info = await sourceGraph.GetOrCreateAsync(file, token);
                if (info != null) return info;
            }

            file = reader.EnumerateInputTextures(context.Material, TextureTags.Height).FirstOrDefault();
            if (file == null) return null;

            return await sourceGraph.GetOrCreateAsync(file, token);
        }

        private async Task<(Image<TPixel>, float)> LoadHeightTextureAsync<TPixel>(CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var info = await GetHeightSourceAsync(token);
            if (info == null) return (null, 0f);

            await using var stream = reader.Open(info.LocalFile);

            Image<TPixel> heightTexture = null;
            try {
                heightTexture = await Image.LoadAsync<TPixel>(Configuration.Default, stream, token);
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");

                // scale height texture instead of using samplers
                var aspect = (float)info.Height / info.Width;
                var bufferSize = context.GetBufferSize(aspect);
                NormalFrameCount = info.FrameCount;
                var scale = 1f;

                if (bufferSize.HasValue && info.Width != bufferSize.Value.Width) {
                    scale = (float)bufferSize.Value.Width / info.Width;
                    var scaledWidth = (int)MathF.Ceiling(info.Width * scale);
                    var scaledHeight = (int)MathF.Ceiling(info.Height * scale * info.FrameCount);

                    var samplerName = context.Profile?.Encoding?.Height?.Sampler ?? context.DefaultSampler;
                    var sampler = context.CreateSampler(heightTexture, samplerName);
                    sampler.Bounds = new RectangleF(0f, 0f, 1f, 1f);
                    sampler.RangeX = sampler.RangeY = 1f / scale;

                    var options = new ResizeProcessor<TPixel>.Options {
                        Sampler = sampler,
                    };

                    var processor = new ResizeProcessor<TPixel>(options);

                    Image<TPixel> heightCopy = null;
                    try {
                        heightCopy = heightTexture;
                        heightTexture = new Image<TPixel>(Configuration.Default, scaledWidth, scaledHeight);

                        foreach (var frame in regions.GetAllRenderRegions(null, NormalFrameCount)) {
                            foreach (var tile in frame.Tiles) {
                                sampler.Bounds = tile.Bounds;

                                var outBounds = tile.Bounds.ScaleTo(scaledWidth, scaledHeight);
                                heightTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                            }
                        }
                    }
                    finally {
                        heightCopy?.Dispose();
                    }
                }
                
                return (heightTexture, scale);
            }
            catch (SourceEmptyException) {throw;}
            catch {
                heightTexture?.Dispose();
                throw;
            }
        }
    }
}
