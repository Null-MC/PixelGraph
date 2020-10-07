using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using Serilog;
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

            if (pack.OutputAlbedo) await ProcessTextureAsync(texture, pack.AlbedoOutputEncoding, $"{texture.Name}.png", token);

            if (pack.OutputHeight) await ProcessTextureAsync(texture, pack.HeightOutputEncoding, $"{texture.Name}_h.png", token);

            if (pack.OutputNormal) await ProcessTextureAsync(texture, pack.NormalOutputEncoding, $"{texture.Name}_n.png", token);

            if (pack.OutputSpecular) await ProcessTextureAsync(texture, pack.SpecularOutputEncoding, $"{texture.Name}_s.png", token);

            if (pack.OutputEmissive) await ProcessTextureAsync(texture, pack.EmissiveOutputEncoding, $"{texture.Name}_e.png", token);

            if (pack.OutputOcclusion) await ProcessTextureAsync(texture, pack.OcclusionOutputEncoding, $"{texture.Name}_ao.png", token);

            // smooth/smooth2/rough

            // reflect

            // porosity

            // sss
        }

        private void BuildSourceMap(PbrProperties texture)
        {
            MapSource(TextureTags.Albedo, ColorChannel.Red, texture.AlbedoInputR ?? pack.AlbedoInputR);
            MapSource(TextureTags.Albedo, ColorChannel.Green, texture.AlbedoInputG ?? pack.AlbedoInputG);
            MapSource(TextureTags.Albedo, ColorChannel.Blue, texture.AlbedoInputB ?? pack.AlbedoInputB);
            MapSource(TextureTags.Albedo, ColorChannel.Alpha, texture.AlbedoInputA ?? pack.AlbedoInputA);

            MapSource(TextureTags.Height, ColorChannel.Red, texture.HeightInputR ?? pack.HeightInputR);
            MapSource(TextureTags.Height, ColorChannel.Green, texture.HeightInputG ?? pack.HeightInputG);
            MapSource(TextureTags.Height, ColorChannel.Blue, texture.HeightInputB ?? pack.HeightInputB);
            MapSource(TextureTags.Height, ColorChannel.Alpha, texture.HeightInputA ?? pack.HeightInputA);

            MapSource(TextureTags.Normal, ColorChannel.Red, texture.NormalInputR ?? pack.NormalInputR);
            MapSource(TextureTags.Normal, ColorChannel.Green, texture.NormalInputG ?? pack.NormalInputG);
            MapSource(TextureTags.Normal, ColorChannel.Blue, texture.NormalInputB ?? pack.NormalInputB);
            MapSource(TextureTags.Normal, ColorChannel.Alpha, texture.NormalInputA ?? pack.NormalInputA);

            MapSource(TextureTags.Specular, ColorChannel.Red, texture.SpecularInputR ?? pack.SpecularInputR);
            MapSource(TextureTags.Specular, ColorChannel.Green, texture.SpecularInputG ?? pack.SpecularInputG);
            MapSource(TextureTags.Specular, ColorChannel.Blue, texture.SpecularInputB ?? pack.SpecularInputB);
            MapSource(TextureTags.Specular, ColorChannel.Alpha, texture.SpecularInputA ?? pack.SpecularInputA);

            MapSource(TextureTags.Emissive, ColorChannel.Red, texture.EmissiveInputR ?? pack.EmissiveInputR);
            MapSource(TextureTags.Emissive, ColorChannel.Green, texture.EmissiveInputG ?? pack.EmissiveInputG);
            MapSource(TextureTags.Emissive, ColorChannel.Blue, texture.EmissiveInputB ?? pack.EmissiveInputB);
            MapSource(TextureTags.Emissive, ColorChannel.Alpha, texture.EmissiveInputA ?? pack.EmissiveInputA);

            MapSource(TextureTags.Occlusion, ColorChannel.Red, texture.OcclusionInputR ?? pack.OcclusionInputR);
            MapSource(TextureTags.Occlusion, ColorChannel.Green, texture.OcclusionInputG ?? pack.OcclusionInputG);
            MapSource(TextureTags.Occlusion, ColorChannel.Blue, texture.OcclusionInputB ?? pack.OcclusionInputB);
            MapSource(TextureTags.Occlusion, ColorChannel.Alpha, texture.OcclusionInputA ?? pack.OcclusionInputA);

            // smooth/smooth2/rough

            // reflect

            // porosity

            // sss
        }

        private void MapSource(string tag, ColorChannel channel, string input)
        {
            //var input = textureInput ?? packInput;
            if (string.IsNullOrEmpty(input)) return;
            if (string.Equals(input, EncodingChannel.None, StringComparison.InvariantCultureIgnoreCase)) return;
            sourceMap[input] = new ChannelSource(tag, channel);
        }

        private async Task<Image> BuildSourceImageAsync(PbrProperties texture, TextureEncoding outputEncoding, CancellationToken token = default)
        {
            var optionsMap = new Dictionary<string, OverlayOptions>();
            static OverlayOptions NewOptions() => new OverlayOptions();
            var sourceColor = new Rgba32();
            var restoreNormalZ = false;

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

                    // restore normal-z
                    if (string.Equals(outputEncoding.R, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;
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

                    // restore normal-z
                    if (string.Equals(outputEncoding.G, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;
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

                    // restore normal-z
                    if (string.Equals(outputEncoding.B, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;
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

                    // restore normal-z
                    if (string.Equals(outputEncoding.A, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;
                }
            }

            Image targetImage = null;
            try {
                foreach (var tag in optionsMap.Keys) {
                    var file = texture.GetTextureFile(reader, tag);

                    if (file == null) {
                        // TODO: replace with local logger!
                        Log.Warning($"No '{tag}' source found for texture '{texture.Name}'.");
                        continue;
                    }

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

                if (targetImage != null && restoreNormalZ) {
                    var options = new NormalRestoreProcessor.Options {
                        NormalX = outputEncoding.GetChannel(EncodingChannel.NormalX),
                        NormalY = outputEncoding.GetChannel(EncodingChannel.NormalY),
                        NormalZ = outputEncoding.GetChannel(EncodingChannel.NormalZ),
                    };

                    if (options.HasAllMappings()) {
                        var processor = new NormalRestoreProcessor(options);
                        targetImage.Mutate(c => c.ApplyProcessor(processor));
                    }
                    else {
                        Log.Warning($"Unable to restore normal-z for texture '{texture.Name}'.");
                    }
                }

                return targetImage ?? new Image<Rgba32>(1, 1, sourceColor);
            }
            catch {
                targetImage?.Dispose();
                throw;
            }
        }

        private async Task ProcessTextureAsync(PbrProperties texture, TextureEncoding encoding, string name, CancellationToken token)
        {
            if (!encoding.Any()) return;

            using var image = await BuildSourceImageAsync(texture, encoding, token);

            filter.Apply(image, texture, encoding);

            var destFile = PathEx.Join(texture.Path, name);
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
