using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
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

        Task<Image<Rgba32>> BuildFinalImageAsync(string tag, CancellationToken token = default);
        Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token = default);
        Task<Image<Rgba32>> GenerateOcclusionAsync(CancellationToken token = default);
    }

    internal class TextureGraph : ITextureGraph
    {
        private readonly Dictionary<string, List<ChannelSource>> sourceMap;
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly ILogger logger;
        private Image<Rgba32> normalTexture;
        private Image<Rgba32> occlusionTexture;

        public MaterialContext Context {get;}


        public TextureGraph(IServiceProvider provider, IInputReader reader, MaterialContext context)
        {
            this.provider = provider;
            this.reader = reader;
            Context = context;

            logger = provider.GetRequiredService<ILogger<TextureGraph>>();
            sourceMap = new Dictionary<string, List<ChannelSource>>();
        }

        public void BuildFromInput()
        {
            foreach (var tag in TextureTags.All) {
                var packEncoding = Context.Input.GetFormatEncoding(tag);
                var materialEncoding = Context.Material.GetInputEncoding(tag);

                MapSource(tag, ColorChannel.Red, materialEncoding?.Red ?? packEncoding?.Red);
                MapSource(tag, ColorChannel.Green, materialEncoding?.Green ?? packEncoding?.Green);
                MapSource(tag, ColorChannel.Blue, materialEncoding?.Blue ?? packEncoding?.Blue);
                MapSource(tag, ColorChannel.Alpha, materialEncoding?.Alpha ?? packEncoding?.Alpha);
            }

            if (ContainsSource(EncodingChannel.Height))
                AddHeightGeneratedInputs();
        }

        //public void BuildFromOutput()
        //{
        //    //var format = Context.Profile.Output?.Format
        //    //             ?? TextureEncoding.Format_Default;

        //    foreach (var tag in TextureTags.All) {
        //        var packEncoding = Context.Profile.Output?.GetFinalTextureEncoding(tag);
        //        //var defaultEncoding = TextureEncoding.GetDefault(format, tag);

        //        MapSource(tag, ColorChannel.Red, packEncoding?.Red);
        //        MapSource(tag, ColorChannel.Green, packEncoding?.Green);
        //        MapSource(tag, ColorChannel.Blue, packEncoding?.Blue);
        //        MapSource(tag, ColorChannel.Alpha, packEncoding?.Alpha);
        //    }

        //    if (ContainsSource(EncodingChannel.Height))
        //        AddHeightGeneratedInputs();
        //}

        public async Task<Image<Rgba32>> BuildFinalImageAsync(string tag, CancellationToken token = default)
        {
            var op = new TextureBuilder(provider);
            op.Build(this, tag);
            return await op.CreateImageAsync(token);
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

        private void MapSource(string tag, ColorChannel channel, string input)
        {
            if (string.IsNullOrEmpty(input) || EncodingChannel.Is(input, EncodingChannel.None)) return;
            sourceMap.GetOrCreate(input, NewSourceMap).Add(new ChannelSource(tag, channel));
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
                    Noise = (float?)Context.Material.Normal?.Noise ?? MaterialNormalProperties.DefaultNoise,
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
