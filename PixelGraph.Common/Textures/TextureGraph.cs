using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public interface ITextureGraph
    {
        MaterialContext Context {get; set;}

        Task PreBuildNormalTextureAsync(CancellationToken token = default);
        void FixEdges(Image image, string tag, Rectangle? bounds = null);

        Task<Image<TPixel>> CreateImageAsync<TPixel>(string textureTag, bool createEmpty, CancellationToken token = default) where TPixel : unmanaged, IPixel<TPixel>;
        Task<Image<Rgb24>> GenerateNormalAsync(CancellationToken token = default);
        Task<Image<Rgb24>> GenerateOcclusionAsync(CancellationToken token = default);
        Task<Image<Rgb24>> GetOrCreateOcclusionAsync(CancellationToken token = default);
    }

    internal class TextureGraph : ITextureGraph
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly ILogger logger;
        private bool hasPreBuiltNormals;

        public MaterialContext Context {get; set;}


        public TextureGraph(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            logger = provider.GetRequiredService<ILogger<TextureGraph>>();

            hasPreBuiltNormals = false;
        }

        private bool HasOutputNormals()
        {
            return Context.OutputEncoding.Where(e => e.HasMapping)
                .Any(e => {
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalX)) return true;
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalY)) return true;
                    if (EncodingChannel.Is(e.ID, EncodingChannel.NormalZ)) return true;
                    return false;
                });
        }

        public async Task PreBuildNormalTextureAsync(CancellationToken token = default)
        {
            if (hasPreBuiltNormals || !HasOutputNormals()) return;
            hasPreBuiltNormals = true;

            if (await TryBuildNormalMapAsync(token)) {
                Context.InputEncoding.RemoveAll(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalX));
                Context.InputEncoding.RemoveAll(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalY));
                Context.InputEncoding.RemoveAll(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalZ));

                Context.InputEncoding.Add(new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.NormalGenerated,
                    Color = ColorChannel.Red,
                });

                Context.InputEncoding.Add(new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.NormalGenerated,
                    Color = ColorChannel.Green,
                });

                Context.InputEncoding.Add(new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.NormalGenerated,
                    Color = ColorChannel.Blue,
                });
            }
        }

        public async Task<Image<TPixel>> CreateImageAsync<TPixel>(string textureTag, bool createEmpty, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var builder = provider.GetRequiredService<ITextureBuilder<TPixel>>();

            try {
                builder.Graph = this;
                builder.Context = Context;
                builder.InputChannels = Context.InputEncoding.ToArray();
                builder.OutputChannels = Context.OutputEncoding
                    .Where(e => TextureTags.Is(e.Texture, textureTag)).ToArray();

                await builder.BuildAsync(createEmpty, token);
                return builder.ImageResult;
            }
            catch {
                builder.Dispose();
                throw;
            }
        }

        public async Task<Image<Rgb24>> GetOrCreateOcclusionAsync(CancellationToken token = default)
        {
            if (Context.OcclusionTexture == null && Context.AutoGenerateOcclusion) {
                try {
                    Context.OcclusionTexture = await GenerateOcclusionAsync(token);
                }
                catch (HeightSourceEmptyException) {}
            }
            
            return Context.OcclusionTexture;
        }

        public void FixEdges(Image image, string tag, Rectangle? bounds = null)
        {
            var hasEdgeSizeX = Context.Material.Height?.EdgeFadeX.HasValue ?? false;
            var hasEdgeSizeY = Context.Material.Height?.EdgeFadeY.HasValue ?? false;
            if (!hasEdgeSizeX && !hasEdgeSizeY) return;

            var heightChannels = Context.OutputEncoding
                .Where(c => TextureTags.Is(c.Texture, tag))
                .Where(c => EncodingChannel.Is(c.ID, EncodingChannel.Height))
                .Select(c => c.Color ?? ColorChannel.None).ToArray();

            if (!heightChannels.Any()) return;

            var options = new HeightEdgeProcessor.Options {
                SizeX = (float?)Context.Material.Height?.EdgeFadeX ?? 0f,
                SizeY = (float?)Context.Material.Height?.EdgeFadeY ?? 0f,
                Colors = heightChannels,
            };

            //if (Context.Material.Height?.EdgeFadeX.HasValue ?? false)
            //    options.SizeX = Context.Material.Height.EdgeFadeX.Value * ,

            var processor = new HeightEdgeProcessor(options);
            image.Mutate(context => {
                if (!bounds.HasValue) context.ApplyProcessor(processor);
                else context.ApplyProcessor(processor, bounds.Value);
            });
        }

        private async Task<bool> TryBuildNormalMapAsync(CancellationToken token)
        {
            // Try to compose from existing channels first
            var normalXChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalX));
            var normalYChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalY));
            var normalZChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalZ));

            var hasNormalX = normalXChannel?.HasMapping ?? false;
            var hasNormalY = normalYChannel?.HasMapping ?? false;

            if (hasNormalX && hasNormalY) {
                // make image from normal X & Y; z if found
                using var context = new MaterialContext {
                    Input = Context.Input,
                    Profile = Context.Profile,
                    Material = Context.Material,
                    //CreateEmpty = false,
                };

                using var builder = provider.GetRequiredService<ITextureBuilder<Rgb24>>();
                builder.Graph = this;
                builder.Context = context;

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

                await builder.BuildAsync(false, token);

                Context.NormalTexture = builder.ImageResult?.CloneAs<Rgb24>();
            }

            var autoGenNormal = Context.Profile?.AutoGenerateNormal
                ?? ResourcePackProfileProperties.AutoGenerateNormalDefault;

            if (Context.NormalTexture == null && autoGenNormal)
                Context.NormalTexture = await GenerateNormalAsync(token);

            if (Context.NormalTexture == null) return false;

            var options = new NormalRotateProcessor.Options {
                NormalX = ColorChannel.Red,
                NormalY = ColorChannel.Green,
                NormalZ = ColorChannel.Blue,
                CurveX = (float?)Context.Material.Normal?.CurveX ?? 0f,
                CurveY = (float?)Context.Material.Normal?.CurveY ?? 0f,
                Noise = (float?)Context.Material.Normal?.Noise ?? 0f,
            };

            var processor = new NormalRotateProcessor(options);
            Context.NormalTexture.Mutate(c => c.ApplyProcessor(processor));

            // apply magnitude channels
            var magnitudeChannels = Context.OutputEncoding
                .Where(c => TextureTags.Is(c.Texture, TextureTags.Normal))
                .Where(c => c.Color == ColorChannel.Magnitude).ToArray();

            if (magnitudeChannels.Length > 1) {
                logger.LogWarning("Only a single encoding channel can be mapped to normal-magnitude! using first mapping. material={DisplayName}", Context.Material.DisplayName);
            }

            var magnitudeChannel = magnitudeChannels.FirstOrDefault();
            if (magnitudeChannel != null) await ApplyMagnitudeAsync(magnitudeChannel, token);

            return true;
        }

        private async Task ApplyMagnitudeAsync(ResourcePackChannelProperties magnitudeChannel, CancellationToken token)
        {
            var localFile = reader.EnumerateTextures(Context.Material, magnitudeChannel.Texture).FirstOrDefault();

            if (localFile != null) {
                await using var sourceStream = reader.Open(localFile);
                using var sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);
                if (sourceImage == null) return;

                var inputChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, magnitudeChannel.ID));
                if (inputChannel == null) return;

                var options = new NormalMagnitudeProcessor<Rgba32>.Options {
                    MagSource = sourceImage,
                    Scale = Context.Material.GetChannelScale(magnitudeChannel.ID),
                };

                options.ApplyInputChannel(inputChannel);
                options.ApplyOutputChannel(magnitudeChannel);

                var processor = new NormalMagnitudeProcessor<Rgba32>(options);
                Context.NormalTexture.Mutate(c => c.ApplyProcessor(processor));
            }
            else if (EncodingChannel.Is(magnitudeChannel.ID, EncodingChannel.Occlusion)) {
                var options = new NormalMagnitudeProcessor<Rgb24>.Options {
                    MagSource = await GetOrCreateOcclusionAsync(token),
                    Scale = Context.Material.GetChannelScale(magnitudeChannel.ID),
                    InputChannel = ColorChannel.Red,
                    InputMinValue = 0f,
                    InputMaxValue = 1f,
                    InputRangeMin = 0,
                    InputRangeMax = 255,
                    InputPower = 1f,
                    //InputPerceptual = false,
                    InputInvert = false,
                };

                if (options.MagSource == null) return;
                options.ApplyOutputChannel(magnitudeChannel);

                var processor = new NormalMagnitudeProcessor<Rgb24>(options);
                Context.NormalTexture.Mutate(c => c.ApplyProcessor(processor));
            }
            else {
                throw new SourceEmptyException("No sources found for applying normal magnitude!");
            }
        }

        public async Task<Image<Rgb24>> GenerateNormalAsync(CancellationToken token)
        {
            logger.LogInformation("Generating normal map for texture {DisplayName}.", Context.Material.DisplayName);

            var heightChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));

            if (heightChannel == null || !heightChannel.HasMapping)
                throw new HeightSourceEmptyException("No height sources mapped!");

            Image<Rgba32> heightTexture = null;
            var heightValue = Context.Material.Height?.Value;

            try {
                var file = reader.EnumerateTextures(Context.Material, heightChannel.Texture).FirstOrDefault();

                float scale;
                if (file != null) {
                    var heightSampler = Context.Profile?.Encoding?.Height?.Sampler;
                    (heightTexture, scale) = await LoadHeightTextureAsync(file, heightSampler, token);
                }
                else if (heightValue.HasValue) {
                    var up = new Rgb24(127, 127, 255);
                    var size = Context.GetBufferSize(1f);
                    return new Image<Rgb24>(Configuration.Default, size?.Width ?? 1, size?.Height ?? 1, up);
                }
                else throw new HeightSourceEmptyException();

                var builder = new NormalMapBuilder {
                    HeightImage = heightTexture,
                    HeightChannel = heightChannel.Color ?? ColorChannel.None,
                    Filter = Context.Material.Normal?.Filter ?? MaterialNormalProperties.DefaultFilter,
                    Strength = (float?)Context.Material.Normal?.Strength ?? MaterialNormalProperties.DefaultStrength,
                    WrapX = Context.WrapX,
                    WrapY = Context.WrapY,

                    // TODO: testing
                    VarianceStrength = 0.998f,
                    LowFreqDownscale = 4,
                    VarianceBlur = 3f,
                };

                // WARN: testing, scale strength by resolution scaling to preserve slope
                builder.Strength *= scale;

                // WARN: temporary hard-coded
                builder.LowFreqStrength = builder.Strength / 4f;

                return builder.Build();
            }
            finally {
                heightTexture?.Dispose();
            }
        }

        public async Task<Image<Rgb24>> GenerateOcclusionAsync(CancellationToken token = default)
        {
            logger.LogInformation("Generating occlusion map for texture {DisplayName}.", Context.Material.DisplayName);

            var heightChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));
            //var emissiveChannel = Context.InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Emissive));

            if (heightChannel?.Color == null || heightChannel.Color.Value == ColorChannel.None)
                throw new HeightSourceEmptyException("No height sources mapped!");

            Image<Rgba32> heightTexture = null;
            //Image<Rgba32> emissiveImage = null;
            //ISampler<Rgba32> emissiveSampler = null;
            try {
                var file = reader.EnumerateTextures(Context.Material, heightChannel.Texture).FirstOrDefault();
                if (file == null) throw new HeightSourceEmptyException();

                //var clipEmissive = Context.Material.Occlusion?.ClipEmissive ?? true;

                //if (clipEmissive && emissiveChannel != null) {
                //    var emissiveFile = reader.EnumerateTextures(Context.Material, emissiveChannel.Texture).FirstOrDefault();

                //    if (emissiveFile != null) {
                //        await using var stream = reader.Open(file);
                //        emissiveImage = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                //        //(emissiveImage, emissiveChannel) = await GetSourceImageAsync(EncodingChannel.Emissive, token);

                //        var emissiveSamplerName = Context.Profile?.Encoding?.Emissive?.Sampler ?? Context.Profile?.Encoding?.Sampler ?? Sampler.Nearest;
                //        emissiveSampler = Sampler<Rgba32>.Create(emissiveSamplerName);
                //        emissiveSampler.Image = emissiveImage;
                //        emissiveSampler.WrapX = Context.WrapX;
                //        emissiveSampler.WrapY = Context.WrapY;

                //        // TODO: set this right!
                //        emissiveSampler.RangeX = emissiveSampler.RangeY = 1f;
                //    }
                //}

                float scale;
                var heightSamplerName = Context.Profile?.Encoding?.Height?.Sampler ?? Context.Profile?.Encoding?.Sampler ?? Sampler.Nearest;
                (heightTexture, scale) = await LoadHeightTextureAsync(file, heightSamplerName, token);

                var occlusionSamplerName = Context.Profile?.Encoding?.Occlusion?.Sampler ?? Context.Profile?.Encoding?.Sampler ?? Sampler.Nearest;
                var sampler = Sampler<Rgba32>.Create(occlusionSamplerName);
                sampler.Image = heightTexture;
                sampler.WrapX = Context.WrapX;
                sampler.WrapY = Context.WrapY;
                sampler.RangeX = sampler.RangeY = scale;

                var options = new OcclusionProcessor.Options {
                    HeightSampler = sampler,
                    HeightChannel = heightChannel.Color.Value,
                    HeightMinValue = (float?)heightChannel.MinValue ?? 0f,
                    HeightMaxValue = (float?)heightChannel.MaxValue ?? 1f,
                    HeightRangeMin = heightChannel.RangeMin ?? 0,
                    HeightRangeMax = heightChannel.RangeMax ?? 255,
                    HeightShift = heightChannel.Shift ?? 0,
                    HeightPower = (float?)heightChannel.Power ?? 0f,
                    HeightInvert = heightChannel.Invert ?? false,

                    //EmissiveSampler = emissiveSampler,
                    //EmissiveChannel = emissiveChannel?.Color ?? ColorChannel.None,
                    //EmissiveMinValue = (float?)emissiveChannel?.MinValue ?? 0f,
                    //EmissiveMaxValue = (float?)emissiveChannel?.MaxValue ?? 1f,
                    //EmissiveRangeMin = emissiveChannel?.RangeMin ?? 0,
                    //EmissiveRangeMax = emissiveChannel?.RangeMax ?? 255,
                    //EmissiveShift = emissiveChannel?.Shift ?? 0,
                    //EmissivePower = (float?)emissiveChannel?.Power ?? 0f,
                    //EmissiveInvert = emissiveChannel?.Invert ?? false,

                    StepDistance = (float?)Context.Material.Occlusion?.StepDistance ?? MaterialOcclusionProperties.DefaultStepDistance,
                    Quality = (float?)Context.Material.Occlusion?.Quality ?? MaterialOcclusionProperties.DefaultQuality,
                    ZScale = (float?)Context.Material.Occlusion?.ZScale ?? MaterialOcclusionProperties.DefaultZScale,
                    ZBias = (float?)Context.Material.Occlusion?.ZBias ?? MaterialOcclusionProperties.DefaultZBias,
                };

                // adjust volume height with texture scale
                options.ZBias *= scale;
                options.ZScale *= scale;

                var processor = new OcclusionProcessor(options);
                var image = new Image<Rgb24>(Configuration.Default, heightTexture.Width, heightTexture.Height);
                image.Mutate(c => c.ApplyProcessor(processor));
                return image;
            }
            finally {
                heightTexture?.Dispose();
                //emissiveImage?.Dispose();
            }
        }

        private async Task<(Image<Rgba32>, float)> LoadHeightTextureAsync(string file, string heightSampler, CancellationToken token)
        {
            await using var stream = reader.Open(file);

            Image<Rgba32> heightTexture = null;
            try {
                heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");

                // scale height texture instead of using samplers
                var aspect = (float)heightTexture.Height / heightTexture.Width;
                var bufferSize = Context.GetBufferSize(aspect);

                var scale = 1f;
                if (bufferSize.HasValue && heightTexture.Width != bufferSize.Value.Width) {
                    scale = (float)bufferSize.Value.Width / heightTexture.Width;
                    var scaledWidth = (int)MathF.Ceiling(heightTexture.Width * scale);
                    var scaledHeight = (int)MathF.Ceiling(heightTexture.Height * scale);

                    var samplerName = heightSampler ?? Context.Profile?.Encoding?.Sampler ?? Sampler.Nearest;
                    var sampler = Sampler<Rgba32>.Create(samplerName);
                    sampler.Image = heightTexture;
                    sampler.WrapX = Context.WrapX;
                    sampler.WrapY = Context.WrapY;
                    sampler.RangeX = sampler.RangeY = 1f / scale;

                    var options = new ResizeProcessor<Rgba32>.Options {
                        Sampler = sampler,
                    };

                    var processor = new ResizeProcessor<Rgba32>(options);

                    Image<Rgba32> heightCopy = null;
                    try {
                        heightCopy = heightTexture;
                        heightTexture = new Image<Rgba32>(Configuration.Default, scaledWidth, scaledHeight);
                        heightTexture.Mutate(c => c.ApplyProcessor(processor));
                    }
                    finally {
                        heightCopy?.Dispose();
                    }
                }
                
                return (heightTexture, scale);
            }
            catch (SourceEmptyException) {throw;}
            catch {
                //logger.LogWarning("Failed to load texture {file}!", file);
                heightTexture?.Dispose();
                throw;
            }
        }
    }
}
