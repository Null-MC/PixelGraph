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
        private Image<Rgba32> normalMap;

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
        }

        public void Build()
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

            MapSource(TextureTags.Specular, ColorChannel.Red, Encoding.SpecularInputR);
            MapSource(TextureTags.Specular, ColorChannel.Green, Encoding.SpecularInputG);
            MapSource(TextureTags.Specular, ColorChannel.Blue, Encoding.SpecularInputB);
            MapSource(TextureTags.Specular, ColorChannel.Alpha, Encoding.SpecularInputA);

            MapSource(TextureTags.Smooth, ColorChannel.Red, Encoding.SmoothInputR);
            MapSource(TextureTags.Smooth, ColorChannel.Green, Encoding.SmoothInputG);
            MapSource(TextureTags.Smooth, ColorChannel.Blue, Encoding.SmoothInputB);
            MapSource(TextureTags.Smooth, ColorChannel.Alpha, Encoding.SmoothInputA);

            // smooth2/rough

            MapSource(TextureTags.Metal, ColorChannel.Red, Encoding.MetalInputR);
            MapSource(TextureTags.Metal, ColorChannel.Green, Encoding.MetalInputG);
            MapSource(TextureTags.Metal, ColorChannel.Blue, Encoding.MetalInputB);
            MapSource(TextureTags.Metal, ColorChannel.Alpha, Encoding.MetalInputA);

            // porosity

            // sss

            MapSource(TextureTags.Occlusion, ColorChannel.Red, Encoding.OcclusionInputR);
            MapSource(TextureTags.Occlusion, ColorChannel.Green, Encoding.OcclusionInputG);
            MapSource(TextureTags.Occlusion, ColorChannel.Blue, Encoding.OcclusionInputB);
            MapSource(TextureTags.Occlusion, ColorChannel.Alpha, Encoding.OcclusionInputA);

            MapSource(TextureTags.Emissive, ColorChannel.Red, Encoding.EmissiveInputR);
            MapSource(TextureTags.Emissive, ColorChannel.Green, Encoding.EmissiveInputG);
            MapSource(TextureTags.Emissive, ColorChannel.Blue, Encoding.EmissiveInputB);
            MapSource(TextureTags.Emissive, ColorChannel.Alpha, Encoding.EmissiveInputA);
        }

        public void MapGeneratedNormal()
        {
            sourceMap.GetOrCreate(EncodingChannel.NormalX, NewSourceMap)
                .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Red));

            sourceMap.GetOrCreate(EncodingChannel.NormalY, NewSourceMap)
                .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Green));

            sourceMap.GetOrCreate(EncodingChannel.NormalZ, NewSourceMap)
                .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Blue));
        }

        public async Task<Image<Rgba32>> BuildFinalImageAsync(string tag, CancellationToken token = default)
        {
            var textureEncoding = TextureEncoding.CreateOutput(Encoding, tag);
            var op = new ImageOperation(provider, this, textureEncoding);
            return await op.CreateImageAsync(token);
        }

        public async Task<Image<Rgba32>> GetGeneratedNormalAsync(CancellationToken token = default)
        {
            if (normalMap == null)
                await BuildNormalMapAsync(token);
            
            return normalMap;
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
            return file != null ? reader.Open(file) : null;
        }

        public void Dispose()
        {
            normalMap?.Dispose();
        }

        private void MapSource(string tag, ColorChannel channel, string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (string.Equals(input, EncodingChannel.None, StringComparison.InvariantCultureIgnoreCase)) return;

            sourceMap.GetOrCreate(input, NewSourceMap)
                .Add(new ChannelSource(tag, channel));
        }

        private async Task BuildNormalMapAsync(CancellationToken token)
        {
            if (normalMap != null) throw new ApplicationException("Normal texture has already been generated!");

            if (!sourceMap.TryGetValue(EncodingChannel.Height, out var sourceList))
                throw new ApplicationException("No height source textures found!");

            logger.LogInformation("Generating normal map for texture {Name}.", Texture.Name);

            foreach (var source in sourceList) {
                var file = Texture.GetTextureFile(reader, source.Tag);
                if (file == null) continue;

                await using var stream = reader.Open(file);
                normalMap = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                var options = new NormalMapProcessor.Options {
                    HeightChannel = source.Channel,
                    Strength = Texture.NormalStrength,
                    Wrap = Texture.Wrap,
                };

                var processor = new NormalMapProcessor(options);
                normalMap.Mutate(c => c.ApplyProcessor(processor));

                //// add to source graph
                //sourceMap.GetOrCreate(EncodingChannel.NormalX, NewSourceMap)
                //    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Red));

                //sourceMap.GetOrCreate(EncodingChannel.NormalY, NewSourceMap)
                //    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Green));

                //sourceMap.GetOrCreate(EncodingChannel.NormalZ, NewSourceMap)
                //    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Blue));

                return;
            }

            //throw new ApplicationException("Failed to generated normal map! No height source textures found!");
            logger.LogWarning("Failed to generated normal map for {Name}; no height source textures found.", Texture.Name);
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
