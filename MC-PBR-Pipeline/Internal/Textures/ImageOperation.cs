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
    internal class ImageOperation
    {
        private readonly TextureGraph graph;
        private readonly TextureEncoding encoding;
        private readonly Dictionary<string, OverlayProcessor.Options> optionsMap;
        private readonly PixelProcessor.Options filterOptions;
        private readonly TextureFilter filter;
        private readonly ILogger logger;
        private Rgba32 sourceColor;
        private bool restoreNormalZ;


        public ImageOperation(IServiceProvider provider, TextureGraph graph, TextureEncoding encoding)
        {
            this.graph = graph;
            this.encoding = encoding;
            logger = provider.GetRequiredService<ILogger<ImageOperation>>();

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

            if (byte.TryParse(outputChannel, out var value)) {
                SetSourceColor(color, value);

                if (string.Equals(EncodingChannel.EmissiveClipped, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                    filterOptions.SetPost(color, filter.EmissiveToEmissiveClipped);

                if (string.Equals(EncodingChannel.EmissiveInverse, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                    filterOptions.SetPost(color, filter.Invert);
            }

            else if (filter.TryGetChannelValue(outputChannel, out value)) {
                SetSourceColor(color, value);

                if (string.Equals(EncodingChannel.EmissiveClipped, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                    filterOptions.SetPost(color, filter.EmissiveToEmissiveClipped);

                if (string.Equals(EncodingChannel.EmissiveInverse, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                    filterOptions.SetPost(color, filter.Invert);
            }

            else if (graph.TryGetSources(outputChannel, out var sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions)
                        .Set(source.Channel, color);

                if (string.Equals(EncodingChannel.Height, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                    filterOptions
                        .Append(color, filter.Invert)
                        .SetPost(color, filter.Invert);

                if (string.Equals(EncodingChannel.EmissiveInverse, outputChannel, StringComparison.InvariantCultureIgnoreCase))
                    filterOptions
                        .Append(color, filter.Invert)
                        .SetPost(color, filter.Invert);
            }

            else {
                // restore normal-z
                if (string.Equals(outputChannel, EncodingChannel.NormalZ, StringComparison.InvariantCultureIgnoreCase)) restoreNormalZ = true;

                var isSmooth = string.Equals(outputChannel, EncodingChannel.Smooth, StringComparison.InvariantCultureIgnoreCase);
                var isSmooth2 = string.Equals(outputChannel, EncodingChannel.PerceptualSmooth, StringComparison.InvariantCultureIgnoreCase);
                var isEmissive = string.Equals(outputChannel, EncodingChannel.Emissive, StringComparison.InvariantCultureIgnoreCase);
                var isEmissiveClipped = string.Equals(outputChannel, EncodingChannel.EmissiveClipped, StringComparison.InvariantCultureIgnoreCase);
                var isEmissiveInv = string.Equals(outputChannel, EncodingChannel.EmissiveInverse, StringComparison.InvariantCultureIgnoreCase);

                // Smooth2 > Smooth
                if (isSmooth2 && graph.TryGetSources(EncodingChannel.Smooth, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                    filterOptions.Append(color, filter.PerceptualSmoothToSmooth);
                }

                // Smooth > Smooth2
                if (isSmooth && graph.TryGetSources(EncodingChannel.PerceptualSmooth, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                    filterOptions.SetPost(color, filter.SmoothToPerceptualSmooth);
                }

                // TODO: Smooth > Rough

                // TODO: Rough > Smooth

                // TODO: Smooth2 > Rough; Rough > Smooth2

                // Emissive > EmissiveClipped
                if (isEmissiveClipped && graph.TryGetSources(EncodingChannel.Emissive, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                    filterOptions.SetPost(color, filter.EmissiveToEmissiveClipped);
                }

                // EmissiveClipped > Emissive
                if (isEmissive && graph.TryGetSources(EncodingChannel.EmissiveClipped, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                    filterOptions.Append(color, filter.EmissiveClippedToEmissive);
                }

                // Emissive > EmissiveInv
                if (isEmissiveInv && graph.TryGetSources(EncodingChannel.Emissive, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                    filterOptions.SetPost(color, filter.Invert);
                }

                // EmissiveInv > Emissive
                if (isEmissive && graph.TryGetSources(EncodingChannel.EmissiveInverse, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                    filterOptions.Append(color, filter.Invert);
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
