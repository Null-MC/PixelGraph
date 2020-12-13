using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Encoding;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureGraph : IDisposable
    {
        MaterialContext Context {get; set;}
        List<ResourcePackChannelProperties> InputEncoding {get; set;}
        List<ResourcePackChannelProperties> OutputEncoding {get; set;}
        Image<Rgba32> NormalTexture {get;}
        bool UseGlobalOutput {get; set;}

        Task BuildNormalTextureAsync(CancellationToken token = default);
        Task BuildFinalImageAsync(string textureTag, CancellationToken token = default);

        Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token = default);
        Task<Image<Rgba32>> GenerateOcclusionAsync(CancellationToken token = default);
        Task<Image<Rgba32>> GetGeneratedOcclusionAsync(CancellationToken token = default);
    }

    internal class TextureGraph : ITextureGraph
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly INamingStructure naming;
        private readonly IImageWriter imageWriter;
        private readonly ILogger logger;
        private Image<Rgba32> occlusionTexture;

        public MaterialContext Context {get; set;}
        public List<ResourcePackChannelProperties> InputEncoding {get; set;}
        public List<ResourcePackChannelProperties> OutputEncoding {get; set;}
        public Image<Rgba32> NormalTexture {get; private set;}
        public bool UseGlobalOutput {get; set;}


        public TextureGraph(IServiceProvider provider)
        {
            this.provider = provider;

            reader = provider.GetRequiredService<IInputReader>();
            naming = provider.GetRequiredService<INamingStructure>();
            imageWriter = provider.GetRequiredService<IImageWriter>();
            logger = provider.GetRequiredService<ILogger<TextureGraph>>();

            InputEncoding = new List<ResourcePackChannelProperties>();
            OutputEncoding = new List<ResourcePackChannelProperties>();
        }

        public async Task BuildNormalTextureAsync(CancellationToken token = default)
        {
            if (await TryBuildNormalMapAsync(token)) {
                InputEncoding.RemoveAll(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalX));
                InputEncoding.RemoveAll(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalY));
                InputEncoding.RemoveAll(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalZ));

                InputEncoding.Add(new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.NormalGenerated,
                    Color = ColorChannel.Red,
                });

                InputEncoding.Add(new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.NormalGenerated,
                    Color = ColorChannel.Green,
                });

                InputEncoding.Add(new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.NormalGenerated,
                    Color = ColorChannel.Blue,
                });
            }
        }

        public async Task BuildFinalImageAsync(string textureTag, CancellationToken token = default)
        {
            using var builder = provider.GetRequiredService<ITextureBuilder>();

            builder.Graph = this;
            builder.Material = Context.Material;
            builder.InputChannels = InputEncoding.ToArray();

            builder.OutputChannels = OutputEncoding.Where(e => TextureTags.Is(e.Texture, textureTag)).ToArray();

            await builder.BuildAsync(token);

            if (builder.ImageResult == null) {
                // TODO: log
                logger.LogWarning("No texture sources found for item {DisplayName} texture {textureTag}.", Context.Material.DisplayName, textureTag);
                return;
            }

            var imageFormat = Context.Profile.ImageEncoding
                ?? ResourcePackProfileProperties.DefaultImageEncoding;

            var p = Context.Material.LocalPath;
            if (!UseGlobalOutput) p = PathEx.Join(p, Context.Material.Name);

            if (Context.Material.IsMultiPart) {
                foreach (var region in Context.Material.Parts) {
                    var name = naming.GetOutputTextureName(Context.Profile, region.Name, textureTag, UseGlobalOutput);
                    var destFile = PathEx.Join(p, name);

                    if (builder.ImageResult.Width == 1 && builder.ImageResult.Height == 1) {
                        await imageWriter.WriteAsync(builder.ImageResult, destFile, imageFormat, token);
                    }
                    else {
                        using var regionImage = GetImageRegion(builder.ImageResult, region.GetRectangle());

                        // TODO: move resize before regions; then scale regions to match
                        using var resizedRegionImage = Resize(regionImage, textureTag);

                        await imageWriter.WriteAsync(resizedRegionImage ?? regionImage, destFile, imageFormat, token);
                    }

                    logger.LogInformation("Published texture region {Name} tag {textureTag}.", region.Name, textureTag);
                }
            }
            else {
                var name = naming.GetOutputTextureName(Context.Profile, Context.Material.Name, textureTag, UseGlobalOutput);
                var destFile = PathEx.Join(p, name);

                using var resizedImage = Resize(builder.ImageResult, textureTag);

                await imageWriter.WriteAsync(resizedImage ?? builder.ImageResult, destFile, imageFormat, token);

                logger.LogInformation("Published texture {DisplayName} tag {textureTag}.", Context.Material.DisplayName, textureTag);
            }

            //await CopyMetaAsync(graph.Context, tag, token);
        }

        private Image<Rgba32> Resize(Image<Rgba32> image, string tag)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            if (!(Context.Material?.ResizeEnabled ?? true)) return null;
            if (!Context.Profile.TextureSize.HasValue && !Context.Profile.TextureScale.HasValue) return null;

            var (width, height) = image.Size();

            var options = new ChannelResizeProcessor.Options {
                SourceWidth = width,
                SourceHeight = height,
            };

            var textureEncodings = OutputEncoding
                .Where(e => TextureTags.Is(e.Texture, tag));

            foreach (var encoding in textureEncodings) {
                var color = encoding.Color ?? ColorChannel.None;
                if (color == ColorChannel.None) continue;

                var samplerName = encoding.Sampler ?? Context.Profile.Output?.Sampler;
                var sampler = Sampler.Create(samplerName) ?? new NearestSampler();

                sampler.Image = image;
                sampler.Wrap = Context.Material?.Wrap ?? true;

                var channel = new ChannelResizeProcessor.ChannelOptions {
                    Color = color,
                    MinValue = encoding.MinValue,
                    MaxValue = encoding.MaxValue,
                    Sampler = sampler,
                };

                options.Channels.Add(channel);
            }

            if (Context.Profile.TextureSize.HasValue) {
                if (width == Context.Profile.TextureSize) return null;

                var aspect = height / (float) width;
                options.TargetWidth = Context.Profile.TextureSize.Value;
                options.TargetHeight = (int)(Context.Profile.TextureSize.Value * aspect);
            }
            else {
                options.TargetWidth = (int)Math.Max(width * Context.Profile.TextureScale.Value, 1f);
                options.TargetHeight = (int)Math.Max(height * Context.Profile.TextureScale.Value, 1f);
            }

            var processor = new ChannelResizeProcessor(options);
            var resizedImage = new Image<Rgba32>(Configuration.Default, options.TargetWidth, options.TargetHeight);

            try {
                resizedImage.Mutate(c => c.ApplyProcessor(processor));
                return resizedImage;
            }
            catch {
                resizedImage.Dispose();
                throw;
            }
        }

        private Image<Rgba32> GetImageRegion(Image<Rgba32> sourceImage, Rectangle bounds)
        {
            Image<Rgba32> regionImage = null;
            try {
                regionImage = new Image<Rgba32>(Configuration.Default, bounds.Width, bounds.Height);

                // TODO: copy region from source
                var options = new RegionProcessor.Options {
                    Source = sourceImage,
                    Offset = bounds.Location,
                };

                regionImage.Mutate(context => {
                    var processor = new RegionProcessor(options);
                    context.ApplyProcessor(processor);
                });

                return regionImage;
            }
            catch {
                regionImage?.Dispose();
                throw;
            }
        }

        public async Task<Image<Rgba32>> GetGeneratedOcclusionAsync(CancellationToken token = default)
        {
            if (occlusionTexture == null) {
                try {
                    occlusionTexture = await GenerateOcclusionAsync(token);
                }
                catch (HeightSourceEmptyException) {}
            }
            
            return occlusionTexture;
        }

        public void Dispose()
        {
            NormalTexture?.Dispose();
            occlusionTexture?.Dispose();
        }

        private async Task<bool> TryBuildNormalMapAsync(CancellationToken token)
        {
            // Try to compose from existing channels first
            var normalXChannel = InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalX));
            var normalYChannel = InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalY));
            var normalZChannel = InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.NormalZ));

            var hasNormalX = normalXChannel?.HasMapping ?? false;
            var hasNormalY = normalYChannel?.HasMapping ?? false;

            if (hasNormalX && hasNormalY) {
                // make image from normal X & Y; z if found
                using var builder = provider.GetRequiredService<ITextureBuilder>();

                builder.Graph = this;
                builder.Material = Context.Material;
                builder.CreateEmpty = false;
                builder.InputChannels = new [] {
                    normalXChannel, normalYChannel, normalZChannel
                };

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

                await builder.BuildAsync(token);

                NormalTexture = builder.ImageResult?.Clone();
            }

            if (NormalTexture == null) {
                // generate
                NormalTexture = await GenerateNormalAsync(token);
            }

            if (NormalTexture == null) return false;

            var options = new NormalRotateProcessor.Options {
                NormalX = ColorChannel.Red,
                NormalY = ColorChannel.Green,
                NormalZ = ColorChannel.Blue,
                CurveX = (float?)Context.Material.Normal?.CurveX ?? 0f,
                CurveY = (float?)Context.Material.Normal?.CurveY ?? 0f,
                Noise = (float?)Context.Material.Normal?.Noise ?? 0f,
            };

            var processor = new NormalRotateProcessor(options);
            NormalTexture.Mutate(c => c.ApplyProcessor(processor));
            return true;
        }

        public async Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token)
        {
            logger.LogInformation("Generating normal map for texture {DisplayName}.", Context.Material.DisplayName);

            var heightChannel = InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));

            if (heightChannel == null || !heightChannel.HasMapping)
                throw new HeightSourceEmptyException("No height sources mapped!");

            Image<Rgba32> heightTexture = null;
            try {
                var file = reader.EnumerateTextures(Context.Material, heightChannel.Texture).FirstOrDefault();
                if (file == null) throw new HeightSourceEmptyException();

                await using var stream = reader.Open(file);

                try {
                    heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                }
                catch {
                    logger.LogWarning("Failed to load texture {file}!", file);
                }
                
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");
                
                var options = new NormalMapProcessor.Options {
                    Source = heightTexture,
                    HeightChannel = heightChannel.Color ?? ColorChannel.None,
                    Strength = (float?)Context.Material.Normal?.Strength ?? MaterialNormalProperties.DefaultStrength,
                    Wrap = Context.Material.Wrap ?? MaterialProperties.DefaultWrap,
                };

                var processor = new NormalMapProcessor(options);
                var image = new Image<Rgba32>(Configuration.Default, heightTexture.Width, heightTexture.Height);
                image.Mutate(c => c.ApplyProcessor(processor));
                return image;
            }
            finally {
                heightTexture?.Dispose();
            }
        }

        public async Task<Image<Rgba32>> GenerateOcclusionAsync(CancellationToken token = default)
        {
            logger.LogInformation("Generating occlusion map for texture {DisplayName}.", Context.Material.DisplayName);

            var heightChannel = InputEncoding.FirstOrDefault(e => EncodingChannel.Is(e.ID, EncodingChannel.Height));

            if (heightChannel?.Color == null || heightChannel.Color.Value == ColorChannel.None)
                throw new HeightSourceEmptyException("No height sources mapped!");

            Image<Rgba32> heightTexture = null;
            //Image<Rgba32> emissiveImage = null;
            try {
                var file = reader.EnumerateTextures(Context.Material, heightChannel.Texture).FirstOrDefault();
                if (file == null) throw new HeightSourceEmptyException();

                //ColorChannel emissiveChannel = ColorChannel.None;
                //if (Context.Material.Occlusion?.ClipEmissive ?? false) {
                //    (emissiveImage, emissiveChannel) = await GetSourceImageAsync(EncodingChannel.Emissive, token);
                //}

                await using var stream = reader.Open(file);

                try {
                    heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                }
                catch {
                    logger.LogWarning("Failed to load texture {file}!", file);
                }
                
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");

                var options = new OcclusionProcessor.Options {
                    Sampler = new BilinearSampler {
                        Image = heightTexture,
                        Wrap = Context.Material?.Wrap ?? MaterialProperties.DefaultWrap,
                    },
                    //HeightSource = heightTexture,
                    HeightChannel = heightChannel.Color.Value,
                    HeightMin = heightChannel.MinValue ?? 0,
                    HeightMax = heightChannel.MaxValue ?? 255,
                    HeightShift = heightChannel.Shift ?? 0,
                    HeightPower = (float?)heightChannel.Power ?? 0f,
                    HeightInvert = heightChannel.Invert ?? false,
                    //EmissiveSource = emissiveImage,
                    //EmissiveChannel = emissiveChannel,
                    StepCount = Context.Material.Occlusion?.Steps ?? MaterialOcclusionProperties.DefaultSteps,
                    Quality = (float?)Context.Material.Occlusion?.Quality ?? MaterialOcclusionProperties.DefaultQuality,
                    ZScale = (float?)Context.Material.Occlusion?.ZScale ?? MaterialOcclusionProperties.DefaultZScale,
                    ZBias = (float?)Context.Material.Occlusion?.ZBias ?? MaterialOcclusionProperties.DefaultZBias,
                    Wrap = Context.Material.Wrap ?? MaterialProperties.DefaultWrap,
                };

                var processor = new OcclusionProcessor(options);
                var image = new Image<Rgba32>(Configuration.Default, heightTexture.Width, heightTexture.Height);
                image.Mutate(c => c.ApplyProcessor(processor));
                return image;
            }
            finally {
                heightTexture?.Dispose();
                //emissiveImage?.Dispose();
            }
        }
    }
}
