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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureGraph : IDisposable
    {
        private readonly IInputReader reader;
        private readonly PackProperties pack;
        private readonly TextureFilter filter;
        private readonly Dictionary<string, List<ChannelSource>> sourceMap;
        private readonly ILogger logger;
        private Image<Rgba32> normalMap;

        public PbrProperties Texture {get;}
        public EncodingProperties Encoding {get;}


        public TextureGraph(IServiceProvider provider, IInputReader reader, PackProperties pack, PbrProperties texture)
        {
            this.reader = reader;
            this.pack = pack;
            Texture = texture;

            logger = provider.GetRequiredService<ILogger<TextureGraph>>();

            filter = new TextureFilter(pack);
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

        public async Task BuildNormalMapAsync(CancellationToken token = default)
        {
            if (normalMap != null) throw new ApplicationException("Normal texture has already been generated!");

            if (!sourceMap.TryGetValue(EncodingChannel.Height, out var sourceList))
                throw new ApplicationException("No height source textures found!");

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

                // add to source graph
                sourceMap.GetOrCreate(EncodingChannel.NormalX, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Red));

                sourceMap.GetOrCreate(EncodingChannel.NormalY, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Green));

                sourceMap.GetOrCreate(EncodingChannel.NormalZ, NewSourceMap)
                    .Add(new ChannelSource(TextureTags.NormalGenerated, ColorChannel.Blue));

                return;
            }

            //throw new ApplicationException("Failed to generated normal map! No height source textures found!");
            logger.LogWarning($"Failed to generated normal map for '{Texture.Name}'; no height source textures found.");
        }

        public async Task<Image<Rgba32>> BuildFinalImageAsync(string tag, CancellationToken token = default)
        {
            var textureEncoding = TextureEncoding.CreateOutput(Encoding, tag);
            var op = new ImageOperation(this, textureEncoding);

            op.MapColor(ColorChannel.Red);
            op.MapColor(ColorChannel.Green);
            op.MapColor(ColorChannel.Blue);
            op.MapColor(ColorChannel.Alpha);

            Image<Rgba32> image = null;
            try {
                image = await op.CreateImageAsync(token);

                filter.Apply(image, Texture, textureEncoding);

                return image;
            }
            catch {
                image?.Dispose();
                throw;
            }
        }

        public bool ContainsSource(string encodingChannel)
        {
            return sourceMap.Keys.Contains(encodingChannel, StringComparer.InvariantCultureIgnoreCase);
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

        private static List<ChannelSource> NewSourceMap() => new List<ChannelSource>();

        private class ChannelSource
        {
            public string Tag {get;}
            public ColorChannel Channel {get;}


            public ChannelSource(string tag, ColorChannel channel)
            {
                Tag = tag;
                Channel = channel;
            }
        }

        private class ImageOperation
        {
            private readonly TextureGraph graph;
            private readonly TextureEncoding encoding;
            private readonly Dictionary<string, OverlayProcessor.Options> optionsMap;
            private Rgba32 sourceColor;
            private bool restoreNormalZ;


            public ImageOperation(TextureGraph graph, TextureEncoding encoding)
            {
                this.graph = graph;
                this.encoding = encoding;

                optionsMap = new Dictionary<string, OverlayProcessor.Options>(StringComparer.InvariantCultureIgnoreCase);
                sourceColor = new Rgba32();
            }

            public void MapColor(ColorChannel color)
            {
                var outputChannel = encoding.Get(color);
                if (outputChannel == null) return;

                if (byte.TryParse(outputChannel, out var value))
                    SetSourceColor(color, value);

                else if (TryGetChannelValue(outputChannel, out value))
                    SetSourceColor(color, value);

                else if (graph.sourceMap.TryGetValue(outputChannel, out var sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions)
                            .Set(source.Channel, color);
                }

                else {
                    // Smooth2 > Smooth
                    var isSmooth2 = string.Equals(outputChannel, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);
                    if (isSmooth2 && graph.sourceMap.TryGetValue(EncodingChannel.Smooth, out sourceList)) {
                        foreach (var source in sourceList) {
                            optionsMap.GetOrCreate(source.Tag, NewOptions)
                                .Set(color, source.Channel, -1);
                        }
                    }

                    // Smooth > Smooth2
                    var isSmooth = string.Equals(outputChannel, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                    if (isSmooth && graph.sourceMap.TryGetValue(EncodingChannel.PerceptualSmooth, out sourceList)) {
                        foreach (var source in sourceList) {
                            optionsMap.GetOrCreate(source.Tag, NewOptions)
                                .Set(color, source.Channel, 1);
                        }
                    }

                    // TODO: Smooth > Rough

                    // TODO: Rough > Smooth

                    // TODO: Smooth2 > Rough; Rough > Smooth2

                    // restore normal-z
                    if (string.Equals(outputChannel, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;
                }
            }

            public async Task<Image<Rgba32>> CreateImageAsync(CancellationToken token = default)
            {
                Image<Rgba32> targetImage = null;

                try {
                    foreach (var tag in optionsMap.Keys.Reverse()) {
                        if (string.Equals(tag, TextureTags.NormalGenerated, StringComparison.InvariantCultureIgnoreCase)) {
                            if (targetImage == null) {
                                var (width, height) = graph.normalMap.Size();
                                targetImage = new Image<Rgba32>(width, height, sourceColor);
                            }

                            var options = optionsMap[tag];
                            options.Source = graph.normalMap;

                            var processor = new OverlayProcessor(options);
                            targetImage.Mutate(context => context.ApplyProcessor(processor));
                        }
                        else {
                            var file = graph.Texture.GetTextureFile(graph.reader, tag);

                            if (file == null) {
                                // TODO: replace with local logger!
                                graph.logger.LogWarning($"No '{tag}' source found for texture '{graph.Texture.Name}'.");
                                continue;
                            }

                            await using var sourceStream = graph.reader.Open(file);
                            using var sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);

                            if (targetImage == null) {
                                var (width, height) = sourceImage.Size();
                                targetImage = new Image<Rgba32>(width, height, sourceColor);
                            }

                            var options = optionsMap[tag];
                            options.Source = sourceImage;

                            var processor = new OverlayProcessor(options);
                            targetImage.Mutate(context => context.ApplyProcessor(processor));
                        }
                    }

                    if (targetImage != null && restoreNormalZ) {
                        var options = new NormalRestoreProcessor.Options {
                            NormalX = encoding.GetChannel(EncodingChannel.NormalX),
                            NormalY = encoding.GetChannel(EncodingChannel.NormalY),
                            NormalZ = encoding.GetChannel(EncodingChannel.NormalZ),
                        };

                        if (options.HasAllMappings()) {
                            var processor = new NormalRestoreProcessor(options);
                            targetImage.Mutate(c => c.ApplyProcessor(processor));
                        }
                        else {
                            graph.logger.LogWarning($"Unable to restore normal-z for texture '{graph.Texture.Name}'.");
                        }
                    }

                    return targetImage ?? new Image<Rgba32>(1, 1, sourceColor);
                }
                catch {
                    targetImage?.Dispose();
                    throw;
                }
            }

            private void SetSourceColor(ColorChannel color, byte value)
            {
                switch (color) {
                    case ColorChannel.Red:
                        sourceColor.R = value;
                        break;
                    case ColorChannel.Green:
                        sourceColor.G = value;
                        break;
                    case ColorChannel.Blue:
                        sourceColor.B = value;
                        break;
                    case ColorChannel.Alpha:
                        sourceColor.A = value;
                        break;
                }
            }

            private bool TryGetChannelValue(string encodingChannel, out byte value)
            {
                byte? result = null;

                if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                    result = valueFunc(graph.Texture);
                    value = result ?? 0;
                }
                else value = 0;

                return result.HasValue;
            }

            private static OverlayProcessor.Options NewOptions() => new OverlayProcessor.Options();
        }

        private static readonly Dictionary<string, Func<PbrProperties, byte?>> valueMap = new Dictionary<string, Func<PbrProperties, byte?>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.NormalX] = tex => tex.NormalValueX,
            [EncodingChannel.NormalY] = tex => tex.NormalValueY,
            [EncodingChannel.NormalZ] = tex => tex.NormalValueZ,
            [EncodingChannel.Height] = tex => tex.HeightValue,
            [EncodingChannel.Smooth] = tex => tex.SmoothValue,
            [EncodingChannel.PerceptualSmooth] = tex => tex.SmoothValue,
            [EncodingChannel.Rough] = tex => tex.RoughValue,
            [EncodingChannel.Metal] = tex => tex.MetalValue,
            [EncodingChannel.Emissive] = tex => tex.EmissiveValue,
            [EncodingChannel.Occlusion] = tex => tex.OcclusionValue,
            [EncodingChannel.Porosity] = tex => tex.PorosityValue,
        };
    }
}
