using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
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
    internal class TextureGraph
    {
        private readonly PackProperties pack;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly TextureFilter filter;
        private readonly Dictionary<string, ChannelSource> sourceMap;


        public TextureGraph(PackProperties pack, IInputReader reader, IOutputWriter writer)
        {
            this.pack = pack;
            this.reader = reader;
            this.writer = writer;

            filter = new TextureFilter(pack);
            sourceMap = new Dictionary<string, ChannelSource>();
        }

        public async Task BuildAsync(PbrProperties texture, CancellationToken token = default)
        {
            sourceMap.Clear();
            BuildSourceMap(texture);

            if (sourceMap.Keys.Contains(EncodingChannel.Height)) {
                // TODO: pre-generate normal map
            }

            await ProcessTextureAsync(texture, pack.AlbedoOutputEncoding, $"{texture.Name}.png", token);
            await ProcessTextureAsync(texture, pack.NormalOutputEncoding, $"{texture.Name}_n.png", token);
            await ProcessTextureAsync(texture, pack.SpecularOutputEncoding, $"{texture.Name}_s.png", token);
            await ProcessTextureAsync(texture, pack.EmissiveOutputEncoding, $"{texture.Name}_e.png", token);
        }

        private async Task ProcessTextureAsync(PbrProperties texture, TextureEncoding encoding, string name, CancellationToken token)
        {
            if (pack.EmissiveIncluded && encoding.Any()) {
                using var image = await BuildSourceImageAsync(texture, encoding, token);

                filter.Apply(image, texture, encoding);

                await SaveImageAsync(texture, image, name, token);
            }
        }

        private void BuildSourceMap(PbrProperties texture)
        {
            MapSource(TextureTags.Albedo, ColorChannel.Red, pack.AlbedoInputR, texture.AlbedoInputR);
            MapSource(TextureTags.Albedo, ColorChannel.Green, pack.AlbedoInputG, texture.AlbedoInputG);
            MapSource(TextureTags.Albedo, ColorChannel.Blue, pack.AlbedoInputB, texture.AlbedoInputB);
            MapSource(TextureTags.Albedo, ColorChannel.Alpha, pack.AlbedoInputA, texture.AlbedoInputA);

            MapSource(TextureTags.Normal, ColorChannel.Red, pack.NormalInputR, texture.NormalInputR);
            MapSource(TextureTags.Normal, ColorChannel.Green, pack.NormalInputG, texture.NormalInputG);
            MapSource(TextureTags.Normal, ColorChannel.Blue, pack.NormalInputB, texture.NormalInputB);
            MapSource(TextureTags.Normal, ColorChannel.Alpha, pack.NormalInputA, texture.NormalInputA);

            MapSource(TextureTags.Specular, ColorChannel.Red, pack.SpecularInputR, texture.SpecularInputR);
            MapSource(TextureTags.Specular, ColorChannel.Green, pack.SpecularInputG, texture.SpecularInputG);
            MapSource(TextureTags.Specular, ColorChannel.Blue, pack.SpecularInputB, texture.SpecularInputB);
            MapSource(TextureTags.Specular, ColorChannel.Alpha, pack.SpecularInputA, texture.SpecularInputA);

            MapSource(TextureTags.Emissive, ColorChannel.Red, pack.EmissiveInputR, texture.EmissiveInputR);
            MapSource(TextureTags.Emissive, ColorChannel.Green, pack.EmissiveInputG, texture.EmissiveInputG);
            MapSource(TextureTags.Emissive, ColorChannel.Blue, pack.EmissiveInputB, texture.EmissiveInputB);
            MapSource(TextureTags.Emissive, ColorChannel.Alpha, pack.EmissiveInputA, texture.EmissiveInputA);
        }

        private void MapSource(string tag, ColorChannel channel, string packInput, string textureInput)
        {
            var input = textureInput ?? packInput;
            if (string.IsNullOrEmpty(input)) return;
            if (string.Equals(input, EncodingChannel.None, StringComparison.InvariantCultureIgnoreCase)) return;
            sourceMap[input] = new ChannelSource(tag, channel);
        }

        private async Task<Image> BuildSourceImageAsync(PbrProperties texture, TextureEncoding outputEncoding, CancellationToken token = default)
        {
            var optionsMap = new Dictionary<string, OverlayOptions>();
            static OverlayOptions NewOptions() => new OverlayOptions();
            var sourceColor = new Rgba32();

            if (outputEncoding.R != null) {
                if (byte.TryParse(outputEncoding.R, out var value)) sourceColor.R = value;
                else if (sourceMap.TryGetValue(outputEncoding.R, out var source))
                    optionsMap.GetOrCreate(source.Tag, NewOptions).RedSource = source.Channel;
                else {
                    // Handle conversion of Smooth <> Smooth2
                    var isSmooth = string.Equals(outputEncoding.R, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                    var isSmooth2 = string.Equals(outputEncoding.R, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);

                    if (isSmooth2 && sourceMap.TryGetValue(EncodingChannel.Smooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.RedSource = source.Channel;
                        opt.RedPower = -1;
                    }
                    else if (isSmooth && sourceMap.TryGetValue(EncodingChannel.PerceptualSmooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.RedSource = source.Channel;
                        opt.RedPower = 1;
                    }
                }
            }

            if (outputEncoding.G != null) {
                if (byte.TryParse(outputEncoding.G, out var value)) sourceColor.G = value;
                else if (sourceMap.TryGetValue(outputEncoding.G, out var source))
                    optionsMap.GetOrCreate(source.Tag, NewOptions).GreenSource = source.Channel;
                else {
                    // Handle conversion of Smooth <> Smooth2
                    var isSmooth = string.Equals(outputEncoding.G, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                    var isSmooth2 = string.Equals(outputEncoding.G, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);

                    if (isSmooth2 && sourceMap.TryGetValue(EncodingChannel.Smooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.GreenSource = source.Channel;
                        opt.GreenPower = -1;
                    }
                    else if (isSmooth && sourceMap.TryGetValue(EncodingChannel.PerceptualSmooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.GreenSource = source.Channel;
                        opt.GreenPower = 1;
                    }
                }
            }

            if (outputEncoding.B != null) {
                if (byte.TryParse(outputEncoding.B, out var value)) sourceColor.B = value;
                else if (sourceMap.TryGetValue(outputEncoding.B, out var source))
                    optionsMap.GetOrCreate(source.Tag, NewOptions).BlueSource = source.Channel;
                else {
                    // Handle conversion of Smooth <> Smooth2
                    var isSmooth = string.Equals(outputEncoding.B, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                    var isSmooth2 = string.Equals(outputEncoding.B, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);

                    if (isSmooth2 && sourceMap.TryGetValue(EncodingChannel.Smooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.BlueSource = source.Channel;
                        opt.BluePower = -1;
                    }
                    else if (isSmooth && sourceMap.TryGetValue(EncodingChannel.PerceptualSmooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.BlueSource = source.Channel;
                        opt.BluePower = 1;
                    }
                }
            }

            if (outputEncoding.A != null) {
                if (byte.TryParse(outputEncoding.A, out var value)) sourceColor.A = value;
                else if (sourceMap.TryGetValue(outputEncoding.A, out var source))
                    optionsMap.GetOrCreate(source.Tag, NewOptions).AlphaSource = source.Channel;
                else {
                    // Handle conversion of Smooth <> Smooth2
                    var isSmooth = string.Equals(outputEncoding.A, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                    var isSmooth2 = string.Equals(outputEncoding.A, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);

                    if (isSmooth2 && sourceMap.TryGetValue(EncodingChannel.Smooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.AlphaSource = source.Channel;
                        opt.AlphaPower = -1;
                    }
                    else if (isSmooth && sourceMap.TryGetValue(EncodingChannel.PerceptualSmooth, out source)) {
                        var opt = optionsMap.GetOrCreate(source.Tag, NewOptions);
                        opt.AlphaSource = source.Channel;
                        opt.AlphaPower = 1;
                    }
                }
            }

            Image targetImage = null;
            try {
                foreach (var tag in optionsMap.Keys) {
                    var file = texture.GetTextureFile(reader, tag);
                    await using var sourceStream = reader.Open(file);
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

                return targetImage ?? new Image<Rgba32>(1, 1, sourceColor);
            }
            catch {
                targetImage?.Dispose();
                throw;
            }
        }

        private async Task SaveImageAsync(PbrProperties texture, Image image, string destFileName, CancellationToken token)
        {
            var destFile = Path.Combine(texture.Path, destFileName);
            await using var stream = writer.WriteFile(destFile);
            await image.SaveAsPngAsync(stream, token);
        }

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
    }
}
