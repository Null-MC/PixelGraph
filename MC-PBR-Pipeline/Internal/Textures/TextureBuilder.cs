using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Extensions;
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
    internal class TextureBuilder
    {
        private readonly TextureGraph graph;
        private readonly TextureEncoding encoding;
        private readonly Dictionary<string, OverlayProcessor.Options> optionsMap;
        private readonly PixelProcessor.Options filterOptions;
        private readonly TextureFilter filter;
        private readonly ILogger logger;
        private Rgba32 sourceColor;
        private bool restoreNormalZ;


        public TextureBuilder(IServiceProvider provider, TextureGraph graph, TextureEncoding encoding)
        {
            this.graph = graph;
            this.encoding = encoding;
            logger = provider.GetRequiredService<ILogger<TextureBuilder>>();

            optionsMap = new Dictionary<string, OverlayProcessor.Options>(StringComparer.InvariantCultureIgnoreCase);
            filterOptions = new PixelProcessor.Options();
            filter = new TextureFilter(graph.Texture);
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

            var isOutputHeight = EncodingChannel.Is(outputChannel, EncodingChannel.Height);
            var isOutputNormalZ = EncodingChannel.Is(outputChannel, EncodingChannel.NormalZ);
            var isOutputSmooth = EncodingChannel.Is(outputChannel, EncodingChannel.Smooth);
            var isOutputSmooth2 = EncodingChannel.Is(outputChannel, EncodingChannel.PerceptualSmooth);
            var isOutputRough = EncodingChannel.Is(outputChannel, EncodingChannel.Rough);
            var isOutputOcclusion = EncodingChannel.Is(outputChannel, EncodingChannel.Occlusion);
            var isOutputEmissive = EncodingChannel.Is(outputChannel, EncodingChannel.Emissive);
            var isOutputEmissiveClipped = EncodingChannel.Is(outputChannel, EncodingChannel.EmissiveClipped);
            var isOutputEmissiveInverse = EncodingChannel.Is(outputChannel, EncodingChannel.EmissiveInverse);

            if (isOutputHeight) filterOptions.SetPost(color, filter.Invert);
            if (isOutputOcclusion) filterOptions.SetPost(color, filter.Invert);
            if (isOutputRough) filterOptions.SetPost(color, filter.Invert);
            if (isOutputSmooth2) filterOptions.SetPost(color, filter.SmoothToPerceptualSmooth);
            if (isOutputEmissiveClipped) filterOptions.SetPost(color, filter.EmissiveToEmissiveClipped);
            if (isOutputEmissiveInverse) filterOptions.SetPost(color, filter.Invert);

            if (byte.TryParse(outputChannel, out var value))
                SetSourceColor(color, value);

            else if (filter.TryGetChannelValue(outputChannel, out value))
                SetSourceColor(color, value);

            else if (graph.TryGetSources(outputChannel, out var sourceList)) {
                PixelAction action = null;
                if (isOutputHeight) action = filter.Invert;
                if (isOutputOcclusion) action = filter.Invert;
                if (isOutputRough) action = filter.Invert;
                if (isOutputSmooth2) action = filter.PerceptualSmoothToSmooth;
                if (isOutputEmissiveClipped) action = filter.EmissiveClippedToEmissive;
                if (isOutputEmissiveInverse) action = filter.Invert;

                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions)
                        .Set(source.Channel, color, action);
            }

            else {
                // restore normal-z
                if (isOutputNormalZ) restoreNormalZ = true;

                // Smooth > *
                if ((isOutputRough || isOutputSmooth2) && graph.TryGetSources(EncodingChannel.Smooth, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);
                }

                // Rough > Smooth
                if ((isOutputSmooth || isOutputSmooth2) && graph.TryGetSources(EncodingChannel.Rough, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.Invert);
                }

                // Smooth2 > Smooth
                if ((isOutputRough || isOutputSmooth) && graph.TryGetSources(EncodingChannel.PerceptualSmooth, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.PerceptualSmoothToSmooth);
                }

                // Emissive > *
                if ((isOutputEmissiveClipped || isOutputEmissiveInverse) && graph.TryGetSources(EncodingChannel.Emissive, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);
                }

                // EmissiveClipped > Emissive
                if ((isOutputEmissive || isOutputEmissiveInverse) && graph.TryGetSources(EncodingChannel.EmissiveClipped, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.EmissiveClippedToEmissive);
                }

                // EmissiveInverse > Emissive
                if ((isOutputEmissive || isOutputEmissiveClipped) && graph.TryGetSources(EncodingChannel.EmissiveInverse, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.Invert);
                }
            }
        }

        public async Task<Image<Rgba32>> CreateImageAsync(CancellationToken token = default)
        {
            Image<Rgba32> image = null;

            try {
                image = await CreateSourceImageAsync(token);

                var redScale = filter.GetScale(encoding.R);
                if (Math.Abs(redScale - 1f) > float.Epsilon)
                    filterOptions.Append(ColorChannel.Red, (ref byte v) => filter.GenericScaleFilter(ref v, in redScale));

                var greenScale = filter.GetScale(encoding.G);
                if (Math.Abs(greenScale - 1f) > float.Epsilon)
                    filterOptions.Append(ColorChannel.Green, (ref byte v) => filter.GenericScaleFilter(ref v, in greenScale));

                var blueScale = filter.GetScale(encoding.B);
                if (Math.Abs(blueScale - 1f) > float.Epsilon)
                    filterOptions.Append(ColorChannel.Blue, (ref byte v) => filter.GenericScaleFilter(ref v, in blueScale));

                var alphaScale = filter.GetScale(encoding.A);
                if (Math.Abs(alphaScale - 1f) > float.Epsilon)
                    filterOptions.Append(ColorChannel.Alpha, (ref byte v) => filter.GenericScaleFilter(ref v, in alphaScale));

                // TODO: additional filtering operations

                var processor = new PixelProcessor(filterOptions);
                image.Mutate(context => context.ApplyProcessor(processor));
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
                    if (TextureTags.Is(tag, TextureTags.NormalGenerated)) continue;

                    var options = optionsMap[tag];

                    await using var sourceStream = graph.OpenTexture(tag);
                    Image<Rgba32> sourceImage = null;
                    var disposeSource = false;

                    try {
                        if (sourceStream != null) {
                            disposeSource = true;
                            sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);
                        }
                        else if (TextureTags.Is(tag, TextureTags.Normal) && optionsMap.ContainsKey(TextureTags.NormalGenerated)) {
                            sourceImage = await graph.GetGeneratedNormalAsync(token);
                            options = optionsMap[TextureTags.NormalGenerated];
                        }

                        if (sourceImage == null) {
                            logger.LogDebug("No source found for texture {Name} tag {tag}.", graph.Texture.Name, tag);
                            continue;
                        }

                        if (targetImage == null) {
                            var (width, height) = sourceImage.Size();
                            targetImage = new Image<Rgba32>(width, height, sourceColor);
                        }

                        options.Source = sourceImage;
                        var processor = new OverlayProcessor(options);
                        targetImage.Mutate(context => context.ApplyProcessor(processor));
                    }
                    finally {
                        if (disposeSource) sourceImage?.Dispose();
                    }
                }

                if (targetImage != null && restoreNormalZ) RestoreNormalZ(targetImage);

                return targetImage ?? new Image<Rgba32>(1, 1, sourceColor);
            }
            catch {
                targetImage?.Dispose();
                throw;
            }
        }

        private void RestoreNormalZ(Image<Rgba32> image)
        {
            var options = new NormalRestoreProcessor.Options {
                NormalX = encoding.GetChannel(EncodingChannel.NormalX),
                NormalY = encoding.GetChannel(EncodingChannel.NormalY),
                NormalZ = encoding.GetChannel(EncodingChannel.NormalZ),
            };

            if (options.HasAllMappings()) {
                var processor = new NormalRestoreProcessor(options);
                image.Mutate(c => c.ApplyProcessor(processor));
            }
            else {
                // TODO: use custom exception instead
                logger.LogWarning("Unable to restore normal-z for texture {Name}.", graph.Texture.Name);
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

        private static OverlayProcessor.Options NewOptions() => new OverlayProcessor.Options();
    }
}
