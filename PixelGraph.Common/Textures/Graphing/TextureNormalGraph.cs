using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.ImageProcessors;
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

namespace PixelGraph.Common.Textures.Graphing
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
        private readonly ILogger<TextureNormalGraph> logger;
        private readonly IServiceProvider provider;
        private readonly ITextureGraphContext context;
        private readonly ITextureHeightGraph heightGraph;

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
            ITextureHeightGraph heightGraph)
        {
            this.provider = provider;
            this.context = context;
            this.heightGraph = heightGraph;
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
                normalContext.IsAnimated = context.IsAnimated;
                
                builder.InputChannels = new [] {normalXChannel, normalYChannel, normalZChannel}
                    .Where(x => x != null).ToArray();

                builder.OutputChannels = new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Red,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    new ResourcePackNormalYChannelProperties {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Green,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    new ResourcePackNormalZChannelProperties {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Blue,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 1m,
                    },
                };

                await builder.MapAsync(false, token);

                if (builder.HasMappedSources) {
                    normalContext.MaxFrameCount = builder.FrameCount;
                    NormalTexture = await builder.BuildAsync<Rgb24>(false, null, token);

                    if (NormalTexture != null) {
                        NormalFrameCount = builder.FrameCount;
                        MagnitudeFrameCount = builder.FrameCount;

                        TryExtractMagnitude();
                    }
                }
            }

            var autoGenNormal = context.Profile?.AutoGenerateNormal
                ?? ResourcePackProfileProperties.AutoGenerateNormalDefault;

            if (NormalTexture == null && autoGenNormal && !context.IsImport) {
                NormalTexture = await GenerateAsync(token);
                hasNormalZ = true;
            }

            if (NormalTexture == null) return false;

            NormalFrameWidth = NormalTexture.Width;
            NormalFrameHeight = NormalTexture.Height;
            if (NormalFrameCount > 1) NormalFrameHeight /= NormalFrameCount;

            var processor = new NormalRotateProcessor {
                Bounds = NormalTexture.Bounds(),
                CurveTop = (float?)context.Material.Normal?.GetCurveTop() ?? 0f,
                CurveBottom = (float?)context.Material.Normal?.GetCurveBottom() ?? 0f,
                CurveLeft = (float?)context.Material.Normal?.GetCurveLeft() ?? 0f,
                CurveRight = (float?)context.Material.Normal?.GetCurveRight() ?? 0f,
                RadiusTop = (float?)context.Material.Normal?.GetRadiusTop() ?? 1f,
                RadiusBottom = (float?)context.Material.Normal?.GetRadiusBottom() ?? 1f,
                RadiusLeft = (float?)context.Material.Normal?.GetRadiusLeft() ?? 1f,
                RadiusRight = (float?)context.Material.Normal?.GetRadiusRight() ?? 1f,
                Noise = (float?)context.Material.Normal?.Noise ?? 0f,
                RestoreNormalZ = !hasNormalZ,
            };

            processor.Apply(NormalTexture);

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

        public async Task<Image<Rgb24>> GenerateAsync(CancellationToken token = default)
        {
            logger.LogInformation("Generating normal map for texture {DisplayName}.", context.Material.DisplayName);

            var heightTex = await heightGraph.GetOrCreateAsync(token);

            if (heightTex == null) {
                var up = new Rgb24(127, 127, 255);
                var size = context.GetBufferSize(1f);
                return new Image<Rgb24>(Configuration.Default, size?.Width ?? 1, size?.Height ?? 1, up);
            }

            NormalFrameCount = heightGraph.HeightFrameCount;

            if (!NormalMapMethod.TryParse(context.Material.Normal?.Method, out var normalMethod))
                normalMethod = NormalMapMethods.Sobel3;

            var regions = provider.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = NormalFrameCount;
            regions.DestFrameCount = NormalFrameCount;

            var builder = new NormalMapBuilder<L16>(regions) {
                HeightImage = heightTex,
                HeightChannel = ColorChannel.Red,
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
            builder.Strength *= heightGraph.HeightScaleFactor;

            // WARN: temporary hard-coded
            builder.LowFreqStrength = builder.Strength / 4f;

            return builder.Build();
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
            var regions = provider.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = NormalFrameCount;
            regions.DestFrameCount = NormalFrameCount;

            foreach (var filter in context.Material.Filters) {
                if (filter.Tile == true && context.IsMaterialCtm) {
                    filter.GetRectangle(out var filterRegion);

                    foreach (var part in regions.GetAllPublishRegions()) {
                        var frame = part.Frames.FirstOrDefault();
                        if (frame == null) continue;

                        var x = frame.SourceBounds.Left + filterRegion.X * frame.SourceBounds.Width;
                        var y = frame.SourceBounds.Top + filterRegion.Y * frame.SourceBounds.Height;
                        var w = frame.SourceBounds.Width * filterRegion.Width;
                        var h = frame.SourceBounds.Height * filterRegion.Height;

                        var region = new Rectangle(
                            (int)(x * NormalTexture.Width + 0.5f),
                            (int)(y * NormalTexture.Height + 0.5f),
                            (int)(w * NormalTexture.Width + 0.5f),
                            (int)(h * NormalTexture.Height + 0.5f));

                        ApplyFilterRegion(filter, region);
                    }
                }
                else {
                    filter.GetRectangle(NormalTexture.Width, NormalTexture.Height, out var region);
                    ApplyFilterRegion(filter, region);
                }
            }
        }

        private void ApplyFilterRegion(MaterialFilter filter, Rectangle region)
        {
            if (filter.HasNormalRotation) {
                var curveProcessor = new NormalRotateProcessor {
                    CurveTop = (float?)filter.GetNormalCurveTop() ?? 0f,
                    CurveBottom = (float?)filter.GetNormalCurveBottom() ?? 0f,
                    CurveLeft = (float?)filter.GetNormalCurveLeft() ?? 0f,
                    CurveRight = (float?)filter.GetNormalCurveRight() ?? 0f,
                    RadiusTop = (float?)filter.GetNormalRadiusTop() ?? 1f,
                    RadiusBottom = (float?)filter.GetNormalRadiusBottom() ?? 1f,
                    RadiusLeft = (float?)filter.GetNormalRadiusLeft() ?? 1f,
                    RadiusRight = (float?)filter.GetNormalRadiusRight() ?? 1f,
                    Noise = (float?)filter.NormalNoise ?? 0f,
                    Bounds = region,
                };

                curveProcessor.Apply(NormalTexture);
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
            MagnitudeTexture = new Image<L8>(NormalTexture.Width, NormalTexture.Height);

            var bounds = NormalTexture.Bounds();
            var pixelMapping = new PixelMapping(mapping);
            ImageProcessors.ImageProcessors.ExtractMagnitude(NormalTexture, MagnitudeTexture, pixelMapping, bounds);

            MagnitudeFrameWidth = MagnitudeTexture.Width;
            MagnitudeFrameHeight = MagnitudeTexture.Height;
            if (MagnitudeFrameCount > 1) MagnitudeFrameHeight /= MagnitudeFrameCount;
        }

        private async Task ApplyMagnitudeAsync(ResourcePackChannelProperties magnitudeChannel, CancellationToken token)
        {
            var inputChannel = context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, magnitudeChannel.ID));
            if (inputChannel == null) return;

            var mapping = new TextureChannelMapping {
                OutputValueScale = (float) context.Material.GetChannelScale(magnitudeChannel.ID),
                OutputValueShift = (float) context.Material.GetChannelShift(magnitudeChannel.ID),
            };

            mapping.ApplyInputChannel(inputChannel);
            mapping.ApplyOutputChannel(magnitudeChannel);

            var options = new NormalMagnitudeWriteProcessor<L8>.Options {
                //Scale = (float)context.Material.GetChannelScale(magnitudeChannel.ID),
                Mapping = new PixelMapping(mapping),
                InputColor = magnitudeChannel.Color ?? ColorChannel.Magnitude,
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
            var regions = provider.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = srcFrameCount;
            regions.DestFrameCount = NormalFrameCount;

            foreach (var frame in regions.GetAllRenderRegions()) {
                foreach (var part in frame.Tiles) {
                    options.MagSampler.SetBounds(part.SourceBounds);
                    var outBounds = part.DestBounds.ScaleTo(NormalTexture.Width, NormalTexture.Height);
                    NormalTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }
    }
}
