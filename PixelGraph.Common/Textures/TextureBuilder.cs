using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.PixelOperations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    internal class TextureBuilder
    {
        private readonly Dictionary<string, OverlayProcessor.Options> optionsMap;
        private readonly Dictionary<ColorChannel, string> encoding;
        private readonly ChannelOptions filterOptions;
        private readonly ILogger logger;
        private TextureFilter filter;
        private TextureGraph graph;
        private Rgba32 sourceColor;
        private bool restoreNormalZ;

        public bool CreateEmpty {get; set;}


        public TextureBuilder(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<TextureBuilder>>();

            optionsMap = new Dictionary<string, OverlayProcessor.Options>(StringComparer.InvariantCultureIgnoreCase);
            encoding = new Dictionary<ColorChannel, string>();
            filterOptions = new ChannelOptions();
            sourceColor = new Rgba32();
        }

        public void Initialize(TextureGraph textureGraph)
        {
            graph = textureGraph;

            filter = new TextureFilter(graph.Context.Material);
        }

        public void MapChannel(ColorChannel color, string encodingChannel)
        {
            if (color == ColorChannel.None) throw new ArgumentOutOfRangeException(nameof(color), "Cannot map to empty color!");
            if (encodingChannel == null) throw new ArgumentNullException(nameof(encodingChannel));

            encoding[color] = encodingChannel;
            MapColor(color);
        }

        private void MapColor(ColorChannel color)
        {
            if (!encoding.TryGetValue(color, out var outputChannel)) return;

            var isOutputHeight = EncodingChannel.Is(outputChannel, EncodingChannel.Height);
            var isOutputNormalZ = EncodingChannel.Is(outputChannel, EncodingChannel.NormalZ);
            var isOutputSmooth = EncodingChannel.Is(outputChannel, EncodingChannel.Smooth);
            var isOutputSmooth2 = EncodingChannel.Is(outputChannel, EncodingChannel.PerceptualSmooth);
            var isOutputRough = EncodingChannel.Is(outputChannel, EncodingChannel.Rough);
            var isOutputOcclusion = EncodingChannel.Is(outputChannel, EncodingChannel.Occlusion);
            var isOutputPorositySSS = EncodingChannel.Is(outputChannel, EncodingChannel.Porosity_SSS);
            var isOutputEmissive = EncodingChannel.Is(outputChannel, EncodingChannel.Emissive);
            var isOutputEmissiveClipped = EncodingChannel.Is(outputChannel, EncodingChannel.EmissiveClipped);
            var isOutputEmissiveInverse = EncodingChannel.Is(outputChannel, EncodingChannel.EmissiveInverse);

            if (isOutputHeight) filterOptions.SetPost(color, filter.Invert);
            if (isOutputOcclusion) filterOptions.SetPost(color, filter.Invert);
            if (isOutputSmooth2) filterOptions.SetPost(color, filter.SmoothToPerceptualSmooth);
            if (isOutputEmissiveClipped) filterOptions.SetPost(color, filter.EmissiveToEmissiveClipped);
            if (isOutputEmissiveInverse) filterOptions.SetPost(color, filter.Invert);

            if (GetSourceValue(outputChannel, out var value)) {
                sourceColor.SetChannelValue(in color, in value);
                return;
            }

            if (graph.TryGetSources(outputChannel, out var sourceList)) {
                ChannelAction action = null;
                if (isOutputHeight) action = filter.Invert;
                if (isOutputOcclusion) action = filter.Invert;
                if (isOutputSmooth2) action = filter.PerceptualSmoothToSmooth;
                if (isOutputEmissiveClipped) action = filter.EmissiveClippedToEmissive;
                if (isOutputEmissiveInverse) action = filter.Invert;

                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions)
                        .Set(source.Channel, color, action);

                return;
            }

            // restore normal-z
            if (isOutputNormalZ) {
                restoreNormalZ = true;
                return;
            }

            // Rough > Smooth/2
            if ((isOutputSmooth || isOutputSmooth2) && graph.TryGetSources(EncodingChannel.Rough, out sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.Invert);

                return;
            }

            // Smooth > Smooth2
            if (isOutputSmooth2 && graph.TryGetSources(EncodingChannel.Smooth, out sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                return;
            }

            // Smooth2 > Smooth
            if (isOutputSmooth && graph.TryGetSources(EncodingChannel.PerceptualSmooth, out sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.PerceptualSmoothToSmooth);

                return;
            }

            if (isOutputRough) {
                // Smooth > Rough
                if (graph.TryGetSources(EncodingChannel.Smooth, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.Invert);

                    return;
                }

                // Smooth2 > Rough
                if (graph.TryGetSources(EncodingChannel.PerceptualSmooth, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, (ref byte v) => {
                            filter.PerceptualSmoothToSmooth(ref v);
                            filter.Invert(ref v);
                        });

                    return;
                }
            }

            if (isOutputPorositySSS) {
                // Porosity > Porosity-SSS
                if (GetSourceValue(EncodingChannel.Porosity, out value)) {
                    sourceColor.SetChannelValue(in color, in value);
                    filterOptions.SetPost(color, filter.PorosityToPSSS);
                    return;
                }

                if (graph.TryGetSources(EncodingChannel.Porosity, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.PorosityToPSSS);

                    return;
                }

                // SSS > Porosity-SSS
                if (GetSourceValue(EncodingChannel.SubSurfaceScattering, out value)) {
                    sourceColor.SetChannelValue(in color, in value);
                    filterOptions.SetPost(color, filter.SSSToPSSS);
                    return;
                }

                if (graph.TryGetSources(EncodingChannel.SubSurfaceScattering, out sourceList)) {
                    foreach (var source in sourceList)
                        optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.SSSToPSSS);

                    return;
                }
            }

            // Emissive > *
            if ((isOutputEmissiveClipped || isOutputEmissiveInverse) && graph.TryGetSources(EncodingChannel.Emissive, out sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color);

                return;
            }

            // EmissiveClipped > Emissive
            if ((isOutputEmissive || isOutputEmissiveInverse) && graph.TryGetSources(EncodingChannel.EmissiveClipped, out sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.EmissiveClippedToEmissive);

                return;
            }

            // EmissiveInverse > Emissive
            if ((isOutputEmissive || isOutputEmissiveClipped) && graph.TryGetSources(EncodingChannel.EmissiveInverse, out sourceList)) {
                foreach (var source in sourceList)
                    optionsMap.GetOrCreate(source.Tag, NewOptions).Set(source.Channel, color, filter.Invert);
            }
        }

        private bool GetSourceValue(in string channel, out byte value)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return byte.TryParse(channel, out value) || filter.TryGetChannelValue(channel, out value);
        }

        public async Task<Image<Rgba32>> CreateImageAsync(CancellationToken token = default)
        {
            Image<Rgba32> image = null;

            try {
                image = await CreateSourceImageAsync(token);
                if (image == null) return null;

                ScaleColor(ColorChannel.Red);
                ScaleColor(ColorChannel.Green);
                ScaleColor(ColorChannel.Blue);
                ScaleColor(ColorChannel.Alpha);

                // TODO: additional filtering operations

                var processor = new ChannelActionProcessor(filterOptions);
                image.Mutate(context => context.ApplyProcessor(processor));
                return image;
            }
            catch {
                image?.Dispose();
                throw;
            }
        }

        private void ScaleColor(ColorChannel color)
        {
            var channel = encoding.Get(color);
            var scale = filter.GetScale(channel);

            if (Math.Abs(scale - 1f) > float.Epsilon)
                filterOptions.Append(color, (ref byte v) => filter.GenericScaleFilter(ref v, in scale));
        }

        private async Task<Image<Rgba32>> CreateSourceImageAsync(CancellationToken token)
        {
            Image<Rgba32> targetImage = null;

            try {
                foreach (var sourceTag in optionsMap.Keys.Reverse()) {
                    if (TextureTags.Is(sourceTag, TextureTags.NormalGenerated)) continue;
                    if (TextureTags.Is(sourceTag, TextureTags.OcclusionGenerated)) continue;

                    var options = optionsMap[sourceTag];

                    await using var sourceStream = graph.OpenTexture(sourceTag);
                    Image<Rgba32> sourceImage = null;
                    var disposeSource = false;

                    try {
                        if (sourceStream != null) {
                            disposeSource = true;
                            sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);
                        }
                        else if (TextureTags.Is(sourceTag, TextureTags.Normal) && optionsMap.ContainsKey(TextureTags.NormalGenerated)) {
                            try {
                                sourceImage = await graph.GetGeneratedNormalAsync(token);
                                options = optionsMap[TextureTags.NormalGenerated];
                            }
                            catch {
                                sourceImage = null;
                            }
                        }
                        else if (TextureTags.Is(sourceTag, TextureTags.Occlusion) && optionsMap.ContainsKey(TextureTags.OcclusionGenerated)) {
                            try {
                                sourceImage = await graph.GetGeneratedOcclusionAsync(token);
                                options = optionsMap[TextureTags.OcclusionGenerated];
                            }
                            catch {
                                sourceImage = null;
                            }
                        }

                        if (sourceImage == null) {
                            logger.LogDebug("No source found for texture {DisplayName} tag {tag}.", graph.Context.Material.DisplayName, sourceTag);
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

                if (targetImage == null && CreateEmpty)
                    targetImage = new Image<Rgba32>(1, 1, sourceColor);

                return targetImage;
            }
            catch {
                targetImage?.Dispose();
                throw;
            }
        }

        private ColorChannel GetChannelColor(string channel)
        {
            foreach (var color in encoding.Keys) {
                if (EncodingChannel.Is(encoding[color], channel))
                    return color;
            }

            return ColorChannel.None;
        }

        private void RestoreNormalZ(Image<Rgba32> image)
        {
            var options = new NormalRestoreProcessor.Options {
                NormalX = GetChannelColor(EncodingChannel.NormalX),
                NormalY = GetChannelColor(EncodingChannel.NormalY),
                NormalZ = GetChannelColor(EncodingChannel.NormalZ),
            };

            if (options.HasAllMappings()) {
                var processor = new NormalRestoreProcessor(options);
                image.Mutate(c => c.ApplyProcessor(processor));
            }
            else {
                // TODO: use custom exception instead
                logger.LogWarning("Unable to restore normal-z for texture {DisplayName}.", graph.Context.Material.DisplayName);
            }
        }

        private static OverlayProcessor.Options NewOptions() => new OverlayProcessor.Options();
    }
}
