using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureGraph : IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly PackProperties pack;
        private readonly Dictionary<string, List<ChannelSource>> sourceMap;
        private readonly ILogger logger;
        private Image<Rgba32> normalTexture;
        private Image<Rgba32> occlusionTexture;

        public PbrProperties Texture {get;}
        public EncodingProperties Encoding {get;}


        public TextureGraph(IServiceProvider provider, IInputReader reader, PackProperties pack, PbrProperties texture)
        {
            this.provider = provider;
            this.reader = reader;
            this.pack = pack;
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
            if (normalTexture == null)
                await GenerateNormalAsync(token);
            
            return normalTexture;
        }

        public async Task<Image<Rgba32>> GetGeneratedOcclusionAsync(CancellationToken token = default)
        {
            if (occlusionTexture == null)
                await GenerateOcclusionAsync(token);
            
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
            var file = Texture.GetTextureFile(reader, tag);
            return file == null ? null : reader.Open(file);
        }

        public void Dispose()
        {
            normalTexture?.Dispose();
            occlusionTexture?.Dispose();
        }

        private void Build()
        {
            Encoding.Build(pack, Texture);

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

        private async Task GenerateNormalAsync(CancellationToken token)
        {
            if (normalTexture != null) throw new ApplicationException("Normal texture has already been generated!");

            logger.LogInformation("Generating normal map for texture {Name}.", Texture.Name);

            if (!sourceMap.TryGetValue(EncodingChannel.Height, out var sourceList))
                throw new ApplicationException("No height source textures found!");

            foreach (var source in sourceList) {
                var file = Texture.GetTextureFile(reader, source.Tag);
                if (file == null) continue;

                await using var stream = reader.Open(file);
                using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                var options = new NormalMapProcessor.Options {
                    Source = heightTexture,
                    HeightChannel = source.Channel,
                    Strength = Texture.NormalStrength ?? 1f,
                    Noise = Texture.NormalNoise ?? 0f,
                    Wrap = Texture.Wrap,
                };

                var processor = new NormalMapProcessor(options);
                normalTexture = new Image<Rgba32>(Configuration.Default, heightTexture.Width, heightTexture.Height);
                normalTexture.Mutate(c => c.ApplyProcessor(processor));

                return;
            }

            logger.LogWarning("Failed to generated normal map for {Name}; no height textures found.", Texture.Name);
        }

        private async Task GenerateOcclusionAsync(CancellationToken token)
        {
            if (occlusionTexture != null) throw new ApplicationException("Occlusion texture has already been generated!");

            logger.LogInformation("Generating occlusion map for texture {Name}.", Texture.Name);

            if (!sourceMap.TryGetValue(EncodingChannel.Height, out var sourceList))
                throw new ApplicationException("No height source textures found!");

            foreach (var source in sourceList) {
                var file = Texture.GetTextureFile(reader, source.Tag);
                if (file == null) continue;

                await using var stream = reader.Open(file);
                using var heightTexture = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                var options = new OcclusionProcessor.Options {
                    Source = heightTexture,
                    HeightChannel = source.Channel,
                    StepCount = Texture.OcclusionSteps,
                    Quality = Texture.OcclusionQuality,
                    ZScale = Texture.OcclusionZScale,
                    Wrap = Texture.Wrap,
                };

                var processor = new OcclusionProcessor(options);
                occlusionTexture = new Image<Rgba32>(Configuration.Default, heightTexture.Width, heightTexture.Height);
                occlusionTexture.Mutate(c => c.ApplyProcessor(processor));

                return;
            }

            logger.LogWarning("Failed to generated occlusion map for {Name}; no height textures found.", Texture.Name);
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
