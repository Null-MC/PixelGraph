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
            return await op.CreateImageAsync(token);
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
            private readonly PixelProcessor.Options filterOptions;
            private Rgba32 sourceColor;
            private bool restoreNormalZ;


            public ImageOperation(TextureGraph graph, TextureEncoding encoding)
            {
                this.graph = graph;
                this.encoding = encoding;

                optionsMap = new Dictionary<string, OverlayProcessor.Options>(StringComparer.InvariantCultureIgnoreCase);
                filterOptions = new PixelProcessor.Options();
                sourceColor = new Rgba32();

                MapColor(ColorChannel.Red);
                MapColor(ColorChannel.Green);
                MapColor(ColorChannel.Blue);
                MapColor(ColorChannel.Alpha);
            }

            private void MapColor(ColorChannel color)
            {
                var outputChannel = encoding.Get(color);
                if (outputChannel == null) return;

                if (byte.TryParse(outputChannel, out var value)) {
                    SetSourceColor(color, value);

                    if (string.Equals(EncodingChannel.EmissiveClipped, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                        filterOptions.SetPost(color, EmissiveToEmissiveClipped);

                    if (string.Equals(EncodingChannel.EmissiveInverse, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                        filterOptions.SetPost(color, Invert);
                }

                else if (TryGetChannelValue(outputChannel, out value)) {
                    SetSourceColor(color, value);

                    if (string.Equals(EncodingChannel.EmissiveClipped, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                        filterOptions.SetPost(color, EmissiveToEmissiveClipped);

                    if (string.Equals(EncodingChannel.EmissiveInverse, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                        filterOptions.SetPost(color, Invert);
                }

                else if (graph.sourceMap.TryGetValue(outputChannel, out var sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions)
                            .Set(source.Channel, color);

                    if (string.Equals(EncodingChannel.EmissiveInverse, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                        filterOptions
                            .Append(color, Invert)
                            .SetPost(color, Invert);
                }

                else {
                    // restore normal-z
                    if (string.Equals(outputChannel, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;

                    // Smooth2 > Smooth
                    var isSmooth2 = string.Equals(outputChannel, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);
                    if (isSmooth2 && graph.sourceMap.TryGetValue(EncodingChannel.Smooth, out sourceList)) {
                        foreach (var source in sourceList)
                            optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                        filterOptions.Append(color, PerceptualSmoothToSmooth);
                    }

                    // Smooth > Smooth2
                    var isSmooth = string.Equals(outputChannel, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                    if (isSmooth && graph.sourceMap.TryGetValue(EncodingChannel.PerceptualSmooth, out sourceList)) {
                        foreach (var source in sourceList)
                            optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                        filterOptions.SetPost(color, SmoothToPerceptualSmooth);
                    }

                    // TODO: Smooth > Rough

                    // TODO: Rough > Smooth

                    // TODO: Smooth2 > Rough; Rough > Smooth2

                    // Emissive > EmissiveClipped
                    var isEmissiveClipped = string.Equals(outputChannel, EncodingChannel.EmissiveClipped, StringComparison.InvariantCultureIgnoreCase);
                    if (isEmissiveClipped && graph.sourceMap.TryGetValue(EncodingChannel.Emissive, out sourceList)) {
                        foreach (var source in sourceList)
                            optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                        filterOptions.SetPost(color, EmissiveToEmissiveClipped);
                    }

                    // EmissiveClipped > Emissive
                    var isEmissive = string.Equals(outputChannel, EncodingChannel.Emissive, StringComparison.InvariantCultureIgnoreCase);
                    if (isEmissive && graph.sourceMap.TryGetValue(EncodingChannel.EmissiveClipped, out sourceList)) {
                        foreach (var source in sourceList)
                            optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                        filterOptions.Append(color, EmissiveClippedToEmissive);
                    }

                    // Emissive > EmissiveInv
                    var isEmissiveInv = string.Equals(outputChannel, EncodingChannel.EmissiveInverse, StringComparison.InvariantCultureIgnoreCase);
                    if (isEmissiveInv && graph.sourceMap.TryGetValue(EncodingChannel.Emissive, out sourceList)) {
                        foreach (var source in sourceList)
                            optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                        filterOptions.SetPost(color, Invert);
                    }

                    // EmissiveInv > Emissive
                    //var isEmissive = string.Equals(outputChannel, EncodingChannel.Emissive, StringComparison.InvariantCultureIgnoreCase);
                    if (isEmissive && graph.sourceMap.TryGetValue(EncodingChannel.EmissiveInverse, out sourceList)) {
                        foreach (var source in sourceList)
                            optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                        filterOptions.Append(color, Invert);
                    }
                }
            }

            public async Task<Image<Rgba32>> CreateImageAsync(CancellationToken token = default)
            {
                Image<Rgba32> image = null;

                try {
                    image = await CreateSourceImageAsync(token);

                    // Height-Scale
                    if (Math.Abs(graph.Texture.HeightScale - 1f) > float.Epsilon) {
                        foreach (var color in encoding.GetChannels(EncodingChannel.Height))
                            filterOptions.Append(color, HeightScaleFilter);
                    }

                    // Generic Scaling
                    var redScale = GetScale(encoding.R);
                    if (Math.Abs(redScale - 1f) > float.Epsilon)
                        filterOptions.Append(ColorChannel.Red, (ref byte v) => GenericScaleFilter(ref v, in redScale));

                    var greenScale = GetScale(encoding.G);
                    if (Math.Abs(greenScale - 1f) > float.Epsilon)
                        filterOptions.Append(ColorChannel.Green, (ref byte v) => GenericScaleFilter(ref v, in greenScale));

                    var blueScale = GetScale(encoding.B);
                    if (Math.Abs(blueScale - 1f) > float.Epsilon)
                        filterOptions.Append(ColorChannel.Blue, (ref byte v) => GenericScaleFilter(ref v, in blueScale));

                    var alphaScale = GetScale(encoding.A);
                    if (Math.Abs(alphaScale - 1f) > float.Epsilon)
                        filterOptions.Append(ColorChannel.Alpha, (ref byte v) => GenericScaleFilter(ref v, in alphaScale));

                    // TODO: additional filtering operations

                    var filter = new PixelProcessor(filterOptions);
                    image.Mutate(context => context.ApplyProcessor(filter));
                    return image;
                }
                catch {
                    image?.Dispose();
                    throw;
                }
            }

            private async Task<Image<Rgba32>> CreateSourceImageAsync(CancellationToken token)
            {
                Image<Rgba32> targetImage = null;

                try {
                    foreach (var tag in optionsMap.Keys.Reverse()) {
                        var options = optionsMap[tag];

                        if (string.Equals(tag, TextureTags.NormalGenerated, StringComparison.InvariantCultureIgnoreCase)) {
                            if (targetImage == null) {
                                var (width, height) = graph.normalMap.Size();
                                targetImage = new Image<Rgba32>(width, height, sourceColor);
                            }

                            options.Source = graph.normalMap;

                            var overlay = new OverlayProcessor(options);
                            targetImage.Mutate(context => context.ApplyProcessor(overlay));
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

                            options.Source = sourceImage;

                            var overlay = new OverlayProcessor(options);
                            targetImage.Mutate(context => context.ApplyProcessor(overlay));
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

            private float GetScale(string channel)
            {
                if (EncodingChannel.IsEmpty(channel)) return 1f;
                return scaleMap.TryGetValue(channel, out var value) ? value(graph.Texture) : 1f;
            }

            private static OverlayProcessor.Options NewOptions() => new OverlayProcessor.Options();

            private void GenericScaleFilter(ref byte value, in float scale) =>
                value = MathEx.Saturate(value / 255d * scale);

            private void HeightScaleFilter(ref byte value) =>
                value = MathEx.Saturate(1 - (1 - value / 255d) * graph.Texture.HeightScale);

            private static void SmoothToPerceptualSmooth(ref byte value) =>
                value = MathEx.Saturate(Math.Sqrt(value / 255d));

            private static void PerceptualSmoothToSmooth(ref byte value) =>
                value = MathEx.Saturate(Math.Pow(value / 255d, 2));

            private static void EmissiveToEmissiveClipped(ref byte value) => value = (byte)(value - 1);

            private static void EmissiveClippedToEmissive(ref byte value) => value = (byte)(value + 1);

            private static void Invert(ref byte value) => value = (byte)(255 - value);
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
            [EncodingChannel.EmissiveClipped] = tex => tex.EmissiveValue,
            [EncodingChannel.EmissiveInverse] = tex => tex.EmissiveValue,
            [EncodingChannel.Occlusion] = tex => tex.OcclusionValue,
            [EncodingChannel.Porosity] = tex => tex.PorosityValue,
        };

        private static readonly Dictionary<string, Func<PbrProperties, float>> scaleMap = new Dictionary<string, Func<PbrProperties, float>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.AlbedoR] = t => t.AlbedoScaleR,
            [EncodingChannel.AlbedoG] = t => t.AlbedoScaleG,
            [EncodingChannel.AlbedoB] = t => t.AlbedoScaleB,
            [EncodingChannel.AlbedoA] = t => t.AlbedoScaleA,
            // AO
            [EncodingChannel.Smooth] = t => t.SmoothScale,
            [EncodingChannel.PerceptualSmooth] = t => t.SmoothScale,
            [EncodingChannel.Rough] = t => t.RoughScale,
            [EncodingChannel.Metal] = t => t.MetalScale,
            // Porosity-SSS
            [EncodingChannel.Emissive] = t => t.EmissiveScale,
            [EncodingChannel.EmissiveClipped] = t => t.EmissiveScale,
            [EncodingChannel.EmissiveInverse] = t => t.EmissiveScale,
        };
    }
}
