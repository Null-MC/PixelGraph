using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
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
        PackProperties Pack {get;}
        PbrProperties Texture {get;}
        EncodingProperties Encoding {get;}

        Task<Image<Rgba32>> GetGeneratedNormalAsync(CancellationToken token = default);
        Task<Image<Rgba32>> GetGeneratedOcclusionAsync(CancellationToken token = default);
        Task<Image<Rgba32>> BuildFinalImageAsync(string tag, CancellationToken token = default);
        Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token = default);
        Task<Image<Rgba32>> GenerateOcclusionAsync(CancellationToken token = default);
        bool ContainsSource(string encodingChannel);
    }

    internal class TextureGraph : ITextureGraph
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly Dictionary<string, List<ChannelSource>> sourceMap;
        private readonly ILogger logger;
        private Image<Rgba32> normalTexture;
        private Image<Rgba32> occlusionTexture;

        public PackProperties Pack {get;}
        public PbrProperties Texture {get;}
        public EncodingProperties Encoding {get;}


        public TextureGraph(IServiceProvider provider, IInputReader reader, PackProperties pack, PbrProperties texture)
        {
            this.provider = provider;
            this.reader = reader;
            Pack = pack;
            Texture = texture;

            logger = provider.GetRequiredService<ILogger<TextureGraph>>();
            sourceMap = new Dictionary<string, List<ChannelSource>>();
            Encoding = new EncodingProperties();

            Build();
        }

        public async Task<Image<Rgba32>> BuildFinalImageAsync(string tag, CancellationToken token = default)
        {
            var textureEncoding = TextureEncoding.CreateOutput(Encoding, tag);
            if (textureEncoding == null) return null;

            var op = new TextureBuilder(provider, this, textureEncoding);
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
            var file = reader.EnumerateTextures(Texture, tag).FirstOrDefault();
            return file == null ? null : reader.Open(file);
        }

        public async Task<(Image<Rgba32>, ColorChannel)> GetSourceImageAsync(string encodingChannel, CancellationToken token = default)
        {
            if (sourceMap.TryGetValue(encodingChannel, out var sourceList)) {
                foreach (var source in sourceList) {
                    var file = reader.EnumerateTextures(Texture, source.Tag).FirstOrDefault();
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

        private void Build()
        {
            Encoding.Build(Pack, Texture);

            MapSource(TextureTags.Albedo, ColorChannel.Red, Encoding.AlbedoInputR);
            MapSource(TextureTags.Albedo, ColorChannel.Green, Encoding.AlbedoInputG);
            MapSource(TextureTags.Albedo, ColorChannel.Blue, Encoding.AlbedoInputB);
            MapSource(TextureTags.Albedo, ColorChannel.Alpha, Encoding.AlbedoInputA);

            MapSource(TextureTags.Height, ColorChannel.Red, Encoding.HeightInputR);
            MapSource(TextureTags.Height, ColorChannel.Green, Encoding.HeightInputG);
            MapSource(TextureTags.Height, ColorChannel.Blue, Encoding.HeightInputB);
            MapSource(TextureTags.Height, ColorChannel.Alpha, Encoding.HeightInputA);

            MapSource(TextureTags.Normal, ColorChannel.Red, Encoding.NormalInputR);
            MapSource(TextureTags.Normal, ColorChannel.Green, Encoding.NormalInputG);
            MapSource(TextureTags.Normal, ColorChannel.Blue, Encoding.NormalInputB);
            MapSource(TextureTags.Normal, ColorChannel.Alpha, Encoding.NormalInputA);

            MapSource(TextureTags.Occlusion, ColorChannel.Red, Encoding.OcclusionInputR);
            MapSource(TextureTags.Occlusion, ColorChannel.Green, Encoding.OcclusionInputG);
            MapSource(TextureTags.Occlusion, ColorChannel.Blue, Encoding.OcclusionInputB);
            MapSource(TextureTags.Occlusion, ColorChannel.Alpha, Encoding.OcclusionInputA);

            MapSource(TextureTags.Specular, ColorChannel.Red, Encoding.SpecularInputR);
            MapSource(TextureTags.Specular, ColorChannel.Green, Encoding.SpecularInputG);
            MapSource(TextureTags.Specular, ColorChannel.Blue, Encoding.SpecularInputB);
            MapSource(TextureTags.Specular, ColorChannel.Alpha, Encoding.SpecularInputA);

            MapSource(TextureTags.Rough, ColorChannel.Red, Encoding.RoughInputR);
            MapSource(TextureTags.Rough, ColorChannel.Green, Encoding.RoughInputG);
            MapSource(TextureTags.Rough, ColorChannel.Blue, Encoding.RoughInputB);
            MapSource(TextureTags.Rough, ColorChannel.Alpha, Encoding.RoughInputA);

            MapSource(TextureTags.Smooth, ColorChannel.Red, Encoding.SmoothInputR);
            MapSource(TextureTags.Smooth, ColorChannel.Green, Encoding.SmoothInputG);
            MapSource(TextureTags.Smooth, ColorChannel.Blue, Encoding.SmoothInputB);
            MapSource(TextureTags.Smooth, ColorChannel.Alpha, Encoding.SmoothInputA);

            MapSource(TextureTags.Metal, ColorChannel.Red, Encoding.MetalInputR);
            MapSource(TextureTags.Metal, ColorChannel.Green, Encoding.MetalInputG);
            MapSource(TextureTags.Metal, ColorChannel.Blue, Encoding.MetalInputB);
            MapSource(TextureTags.Metal, ColorChannel.Alpha, Encoding.MetalInputA);

            MapSource(TextureTags.Porosity, ColorChannel.Red, Encoding.PorosityInputR);
            MapSource(TextureTags.Porosity, ColorChannel.Green, Encoding.PorosityInputG);
            MapSource(TextureTags.Porosity, ColorChannel.Blue, Encoding.PorosityInputB);
            MapSource(TextureTags.Porosity, ColorChannel.Alpha, Encoding.PorosityInputA);

            MapSource(TextureTags.SubSurfaceScattering, ColorChannel.Red, Encoding.SubSurfaceScatteringInputR);
            MapSource(TextureTags.SubSurfaceScattering, ColorChannel.Green, Encoding.SubSurfaceScatteringInputG);
            MapSource(TextureTags.SubSurfaceScattering, ColorChannel.Blue, Encoding.SubSurfaceScatteringInputB);
            MapSource(TextureTags.SubSurfaceScattering, ColorChannel.Alpha, Encoding.SubSurfaceScatteringInputA);

            MapSource(TextureTags.Emissive, ColorChannel.Red, Encoding.EmissiveInputR);
            MapSource(TextureTags.Emissive, ColorChannel.Green, Encoding.EmissiveInputG);
            MapSource(TextureTags.Emissive, ColorChannel.Blue, Encoding.EmissiveInputB);
            MapSource(TextureTags.Emissive, ColorChannel.Alpha, Encoding.EmissiveInputA);

            if (ContainsSource(EncodingChannel.Height)) {
                sourceMap.GetOrCreate(EncodingChannel.NormalX, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Red));

                sourceMap.GetOrCreate(EncodingChannel.NormalY, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Green));

                sourceMap.GetOrCreate(EncodingChannel.NormalZ, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Blue));

                sourceMap.GetOrCreate(EncodingChannel.Occlusion, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.OcclusionGenerated, ColorChannel.Red));
            }
        }

        private void MapSource(string tag, ColorChannel channel, string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (string.Equals(input, EncodingChannel.None, StringComparison.InvariantCultureIgnoreCase)) return;

            sourceMap.GetOrCreate(input, NewSourceMap)
                .Add(new ChannelSource(tag, channel));
        }

        public async Task<Image<Rgba32>> GenerateNormalAsync(CancellationToken token)
        {
            logger.LogInformation("Generating normal map for texture {DisplayName}.", Texture.DisplayName);

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
                    Strength = Texture.NormalStrength ?? 1f,
                    Noise = Texture.NormalNoise ?? 0f,
                    Wrap = Texture.Wrap,
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
            logger.LogInformation("Generating occlusion map for texture {DisplayName}.", Texture.DisplayName);

            if (!sourceMap.ContainsKey(EncodingChannel.Height))
                throw new SourceEmptyException("No height source textures found!");

            Image<Rgba32> heightTexture = null;
            Image<Rgba32> emissiveImage = null;
            try {
                ColorChannel heightChannel, emissiveChannel = ColorChannel.None;
                (heightTexture, heightChannel) = await GetSourceImageAsync(EncodingChannel.Height, token);
                if (heightTexture == null) throw new SourceEmptyException("No height source textures found!");

                if (Texture.OcclusionClipEmissive ?? false) {
                    (emissiveImage, emissiveChannel) = await GetSourceImageAsync(EncodingChannel.Emissive, token);
                }

                var options = new OcclusionProcessor.Options {
                    HeightSource = heightTexture,
                    HeightChannel = heightChannel,
                    EmissiveSource = emissiveImage,
                    EmissiveChannel = emissiveChannel,
                    StepCount = Texture.OcclusionSteps ?? PbrProperties.Default_OcclusionSteps,
                    Quality = Texture.OcclusionQuality ?? PbrProperties.Default_OcclusionQuality,
                    ZScale = Texture.OcclusionZScale ?? PbrProperties.Default_OcclusionZScale,
                    ZBias = Texture.OcclusionZBias ?? PbrProperties.Default_OcclusionZBias,
                    Wrap = Texture.Wrap,
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
