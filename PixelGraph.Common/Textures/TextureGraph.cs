using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureGraph : IDisposable
    {
        MaterialContext Context {get;}
        //bool UseGlobalOutput {get; set;}

        Task BuildFinalImageAsync(string textureTag, ResourcePackChannelProperties[] encoding, CancellationToken token = default);
        Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token = default);
        Task<Image<Rgba32>> GenerateOcclusionAsync(CancellationToken token = default);
    }

    internal class TextureGraph : ITextureGraph
    {
        private readonly Dictionary<string, List<ChannelSource>> sourceMap;
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly INamingStructure naming;
        private readonly IImageWriter imageWriter;
        private readonly ILogger logger;
        private Image<Rgba32> normalTexture;
        private Image<Rgba32> occlusionTexture;

        public MaterialContext Context {get;}
        public bool UseGlobalOutput {get; set;}


        public TextureGraph(IServiceProvider provider, MaterialContext context)
        {
            this.provider = provider;
            Context = context;

            reader = provider.GetRequiredService<IInputReader>();
            naming = provider.GetRequiredService<INamingStructure>();
            imageWriter = provider.GetRequiredService<IImageWriter>();
            logger = provider.GetRequiredService<ILogger<TextureGraph>>();
            sourceMap = new Dictionary<string, List<ChannelSource>>();
        }

        public void BuildFromInput()
        {
            foreach (var tag in Context.Input.GetAllTags()) {
                foreach (var channel in Context.Input.GetByTag(tag)) {
                    MapSource(tag, channel);
                }
            }

            if (ContainsSource(EncodingChannel.Height))
                AddHeightGeneratedInputs();
        }

        public void BuildFromOutput()
        {
            foreach (var tag in Context.Profile.Output.GetAllTags()) {
                foreach (var channel in Context.Profile.Output.GetByTag(tag)) {
                    if (!channel.Color.HasValue) continue;

                    MapSource(tag, channel);
                }
            }

            //if (ContainsSource(EncodingChannel.Height))
            //    AddHeightGeneratedInputs();
        }

        public async Task BuildFinalImageAsync(string textureTag, ResourcePackChannelProperties[] encoding, CancellationToken token = default)
        {
            using var builder = provider.GetRequiredService<ITextureBuilder>();

            builder.Material = Context.Material;
            builder.InputChannels = Context.Input.All;
            builder.OutputChannels = Context.Profile.Output.GetByTag(textureTag).ToArray();

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
                        using var resizedRegionImage = Resize(Context, regionImage, encoding);

                        await imageWriter.WriteAsync(resizedRegionImage ?? builder.ImageResult, destFile, imageFormat, token);
                    }

                    logger.LogInformation("Published texture region {Name} tag {textureTag}.", region.Name, textureTag);
                }
            }
            else {
                var name = naming.GetOutputTextureName(Context.Profile, Context.Material.Name, textureTag, UseGlobalOutput);
                var destFile = PathEx.Join(p, name);

                using var resizedImage = Resize(Context, builder.ImageResult, encoding);

                await imageWriter.WriteAsync(resizedImage ?? builder.ImageResult, destFile, imageFormat, token);

                logger.LogInformation("Published texture {DisplayName} tag {textureTag}.", Context.Material.DisplayName, textureTag);
            }

            //await CopyMetaAsync(graph.Context, tag, token);
        }

        private static Image<Rgba32> Resize(MaterialContext context, Image<Rgba32> image, ResourcePackChannelProperties[] encoding)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (image == null) throw new ArgumentNullException(nameof(image));

            if (!(context.Material?.ResizeEnabled ?? true)) return null;
            if (!context.Profile.TextureSize.HasValue && !context.Profile.TextureScale.HasValue) return null;

            var options = new ResizeProcessor.Options {
                Source = image,
                TargetWidth = context.Profile.TextureSize ?? 0,
                Channels = encoding,
            };

            var (width, height) = image.Size();

            //var encoding = context.Profile.Output.GetFinalTextureEncoding(tag);
            //var samplerName = encoding.Sampler;

            //var resampler = KnownResamplers.Bicubic;
            //if (samplerName != null && Samplers.TryParse(samplerName, out var _resampler))
            //    resampler = _resampler;

            if (context.Profile.TextureSize.HasValue) {
                if (width == context.Profile.TextureSize) return null;

                var aspect = height / (float) width;
                options.TargetWidth = context.Profile.TextureSize.Value;
                options.TargetHeight = (int)(context.Profile.TextureSize.Value * aspect);
            }
            else {
                options.TargetWidth = (int)Math.Max(width * context.Profile.TextureScale.Value, 1f);
                options.TargetHeight = (int)Math.Max(height * context.Profile.TextureScale.Value, 1f);
            }

            var processor = new ResizeProcessor(options);
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

        public async Task<Image<Rgba32>> GetGeneratedNormalAsync(CancellationToken token = default)
        {
            if (normalTexture == null) {
                normalTexture = await GenerateNormalAsync(token);
            }
            
            return normalTexture;
        }

        public async Task<Image<Rgba32>> GetGeneratedOcclusionAsync(CancellationToken token = default)
        {
            if (occlusionTexture == null) {
                occlusionTexture = await GenerateOcclusionAsync(token);
            }
            
            return occlusionTexture;
        }

        public bool ContainsSource(string encodingChannel)
        {
            return sourceMap.Keys.Contains(encodingChannel, StringComparer.InvariantCultureIgnoreCase);
        }

        public bool TryGetSources(string outputChannel, out ChannelSource[] sources)
        {
            var result = sourceMap.TryGetValue(outputChannel, out var sourceList);
            sources = result ? sourceList.ToArray() : null;
            return result;
        }

        public Stream OpenTexture(string tag)
        {
            var file = reader.EnumerateTextures(Context.Material, tag).FirstOrDefault();
            return file == null ? null : reader.Open(file);
        }

        public async Task<(Image<Rgba32>, ColorChannel)> GetSourceImageAsync(string encodingChannel, CancellationToken token = default)
        {
            if (sourceMap.TryGetValue(encodingChannel, out var sourceList)) {
                foreach (var source in sourceList) {
                    var file = reader.EnumerateTextures(Context.Material, source.Tag).FirstOrDefault();
                    if (file == null) continue;

                    await using var stream = reader.Open(file);

                    try {
                        var image = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                        return (image, source.Channel);
                    }
                    catch {
                        logger.LogWarning("Failed to load texture {file}!", file);
                    }
                }
            }

            return (null, ColorChannel.None);
        }

        public void Dispose()
        {
            normalTexture?.Dispose();
            occlusionTexture?.Dispose();
        }

        private void AddHeightGeneratedInputs()
        {
            sourceMap.GetOrCreate(EncodingChannel.NormalX, NewSourceMap)
                .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Red));

            sourceMap.GetOrCreate(EncodingChannel.NormalY, NewSourceMap)
                .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Green));

            sourceMap.GetOrCreate(EncodingChannel.NormalZ, NewSourceMap)
                .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Blue));

            sourceMap.GetOrCreate(EncodingChannel.Occlusion, NewSourceMap)
                .Add(new ChannelSource(TextureTags.OcclusionGenerated, ColorChannel.Red));
        }

        //private void MapSource(string tag, ColorChannel channel, string input)
        //{
        //    if (string.IsNullOrEmpty(input) || EncodingChannel.Is(input, EncodingChannel.None)) return;
        //    sourceMap.GetOrCreate(input, NewSourceMap).Add(new ChannelSource(tag, channel));
        //}

        private void MapSource(string tag, ResourcePackChannelProperties channel)
        {
            if (!channel.Color.HasValue) return;
            if (string.IsNullOrEmpty(channel.Texture) || EncodingChannel.Is(channel.Texture, EncodingChannel.None)) return;

            sourceMap.GetOrCreate(channel.Texture, NewSourceMap).Add(new ChannelSource(tag, channel.Color.Value));
        }

        public async Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token)
        {
            logger.LogInformation("Generating normal map for texture {DisplayName}.", Context.Material.DisplayName);

            if (!sourceMap.ContainsKey(EncodingChannel.Height))
                throw new ApplicationException("No height source textures found!");

            Image<Rgba32> heightTexture = null;
            try {
                ColorChannel heightChannel;
                (heightTexture, heightChannel) = await GetSourceImageAsync(EncodingChannel.Height, token);
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");

                var options = new NormalMapProcessor.Options {
                    Source = heightTexture,
                    HeightChannel = heightChannel,
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

            if (!sourceMap.ContainsKey(EncodingChannel.Height))
                throw new SourceEmptyException("No height source textures found!");

            Image<Rgba32> heightTexture = null;
            Image<Rgba32> emissiveImage = null;
            try {
                ColorChannel heightChannel, emissiveChannel = ColorChannel.None;
                (heightTexture, heightChannel) = await GetSourceImageAsync(EncodingChannel.Height, token);
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");

                if (Context.Material.Occlusion?.ClipEmissive ?? false) {
                    (emissiveImage, emissiveChannel) = await GetSourceImageAsync(EncodingChannel.Emissive, token);
                }

                var options = new OcclusionProcessor.Options {
                    HeightSource = heightTexture,
                    HeightChannel = heightChannel,
                    EmissiveSource = emissiveImage,
                    EmissiveChannel = emissiveChannel,
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
                emissiveImage?.Dispose();
            }
        }

        private static List<ChannelSource> NewSourceMap() => new List<ChannelSource>();

        internal class ChannelSource
        {
            public string Tag {get;}
            public ColorChannel Channel {get;}


            public ChannelSource(string tag, ColorChannel channel)
            {
                Tag = tag;
                Channel = channel;
            }
        }
    }
}
