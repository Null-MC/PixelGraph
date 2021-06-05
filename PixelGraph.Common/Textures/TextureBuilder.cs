using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
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
    public interface ITextureBuilder
    {
        ResourcePackChannelProperties[] InputChannels {get; set;}
        ResourcePackChannelProperties[] OutputChannels {get; set;}
        bool HasMappedSources {get;}
        int FrameCount {get;}
        int? TargetFrame {get; set;}
        int? TargetPart {get; set;}

        Task MapAsync(bool createEmpty, CancellationToken token = default);
        Task<Image<TPixel>>  BuildAsync<TPixel>(bool createEmpty, CancellationToken token = default) where TPixel : unmanaged, IPixel<TPixel>;
    }

    internal class TextureBuilder : ITextureBuilder
    {
        private readonly IInputReader reader;
        private readonly ITextureGraphContext context;
        private readonly ITextureRegionEnumerator regions;
        private readonly ITextureSourceGraph sourceGraph;
        private readonly ITextureNormalGraph normalGraph;
        private readonly ITextureOcclusionGraph occlusionGraph;
        private readonly List<TextureChannelMapping> mappings;
        private Rgba32 defaultValues;
        private Size bufferSize;
        private bool isGrayscale;

        public ResourcePackChannelProperties[] InputChannels {get; set;}
        public ResourcePackChannelProperties[] OutputChannels {get; set;}
        public bool HasMappedSources {get; private set;}
        public int FrameCount {get; private set;}
        public int? TargetFrame {get; set;}
        public int? TargetPart {get; set;}


        public TextureBuilder(
            IInputReader reader,
            ITextureGraphContext context,
            ITextureRegionEnumerator regions,
            ITextureSourceGraph sourceGraph,
            ITextureNormalGraph normalGraph,
            ITextureOcclusionGraph occlusionGraph)
        {
            this.reader = reader;
            this.context = context;
            this.regions = regions;
            this.sourceGraph = sourceGraph;
            this.normalGraph = normalGraph;
            this.occlusionGraph = occlusionGraph;

            mappings = new List<TextureChannelMapping>();
            defaultValues = new Rgba32();
        }

        public async Task MapAsync(bool createEmpty, CancellationToken token = default)
        {
            defaultValues.R = defaultValues.G = defaultValues.B = defaultValues.A = 0;
            mappings.Clear();

            HasMappedSources = false;
            FrameCount = 1;

            foreach (var channel in OutputChannels) {
                if (channel.Color == ColorChannel.Magnitude) continue;

                if (TryBuildMapping(channel, createEmpty, out var mapping) || createEmpty) {
                    mappings.Add(mapping);

                    if (mapping.SourceFilename != null) {
                        var info = await sourceGraph.GetOrCreateAsync(mapping.SourceFilename, token);
                        if (info != null) {
                            HasMappedSources = true;
                            if (info.FrameCount > FrameCount)
                                FrameCount = info.FrameCount;
                        }
                    }

                    if (TextureTags.Is(mapping.SourceTag, TextureTags.NormalGenerated)) {
                        HasMappedSources = true;
                        if (normalGraph.NormalFrameCount > FrameCount)
                            FrameCount = normalGraph.NormalFrameCount;
                    }

                    if (TextureTags.Is(mapping.SourceTag, TextureTags.MagnitudeBuffer)) {
                        HasMappedSources = true;
                        if (normalGraph.MagnitudeFrameCount > FrameCount)
                            FrameCount = normalGraph.MagnitudeFrameCount;
                    }

                    if (TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated)) {
                        HasMappedSources = true;
                        if (occlusionGraph.FrameCount > FrameCount)
                            FrameCount = occlusionGraph.FrameCount;
                    }
                }

                ApplyDefaultValue(mapping);
            }
        }
        
        public async Task<Image<TPixel>> BuildAsync<TPixel>(bool createEmpty, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (!createEmpty && mappings.Count == 0) return null;

            isGrayscale = mappings.All(x => x.OutputColor == ColorChannel.Red);
            if (isGrayscale) defaultValues.B = defaultValues.G = defaultValues.R;

            var size = await GetBufferSizeAsync(token);
            if (!size.HasValue && !createEmpty) return null;
            bufferSize = size ?? new Size(1);

            var width = bufferSize.Width;
            var height = bufferSize.Height;

            if (!TargetFrame.HasValue && context.MaxFrameCount > 1)
                height *= context.MaxFrameCount;

            if (TargetPart.HasValue) {
                var f = TargetFrame ?? 0;
                var part = regions.GetPublishPartFrame(f, FrameCount, TargetPart.Value);
                width = (int) MathF.Ceiling(part.SourceBounds.Width * width);
                height = (int) MathF.Ceiling(part.SourceBounds.Height * height);
            }

            var pixel = new TPixel();
            pixel.FromRgba32(defaultValues);
            var imageResult = new Image<TPixel>(Configuration.Default, width, height, pixel);

            var mappingsWithSources = mappings
                .Where(m => m.SourceFilename != null)
                .GroupBy(m => m.SourceFilename);

            var mappingsWithoutSources = mappings
                .Where(m => m.SourceFilename == null);

            var mappingsWithOutputOcclusion = mappings
                .Where(m => m.OutputApplyOcclusion).ToArray();

            try {
                foreach (var mappingGroup in mappingsWithSources)
                    await ApplySourceMappingAsync(imageResult, mappingGroup.Key, mappingGroup.ToArray(), token);

                foreach (var mapping in mappingsWithoutSources) {
                    if (TextureTags.Is(mapping.SourceTag, TextureTags.MagnitudeBuffer))
                        ApplyMagnitudeMapping(imageResult, mapping);

                    if (TextureTags.Is(mapping.SourceTag, TextureTags.NormalGenerated))
                        ApplyNormalMapping(imageResult, mapping);

                    if (createEmpty && TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated))
                        await ApplyOcclusionMappingAsync(imageResult, mapping, token);
                }

                if (mappingsWithOutputOcclusion.Any())
                    await ApplyOutputOcclusionAsync(imageResult, mappingsWithOutputOcclusion, token);

                return imageResult;
            }
            catch {
                imageResult.Dispose();
                throw;
            }
        }

        private bool TryBuildMapping(ResourcePackChannelProperties outputChannel, bool createEmpty, out TextureChannelMapping mapping)
        {
            var samplerName = outputChannel.Sampler ?? context.DefaultSampler;
            decimal value;

            mapping = new TextureChannelMapping {
                OutputColor = outputChannel.Color ?? ColorChannel.None,
                OutputMinValue = (float?)outputChannel.MinValue ?? 0f,
                OutputMaxValue = (float?)outputChannel.MaxValue ?? 1f,
                OutputRangeMin = outputChannel.RangeMin ?? 0,
                OutputRangeMax = outputChannel.RangeMax ?? 255,
                OutputShift = outputChannel.Shift ?? 0,
                OutputPower = (float?)outputChannel.Power ?? 1,
                OutputInverted = outputChannel.Invert ?? false,
                OutputSampler = samplerName,

                ValueShift = (float)context.Material.GetChannelShift(outputChannel.ID),
                OutputScale = (float)context.Material.GetChannelScale(outputChannel.ID),
            };

            var inputChannel = InputChannels.FirstOrDefault(i
                => EncodingChannel.Is(i.ID, outputChannel.ID));

            if (context.Material.TryGetChannelValue(outputChannel.ID, out value)) {
                mapping.InputValue = (float)value;
                mapping.ApplyInputChannel(inputChannel);
                return true;
            }

            if (inputChannel?.Texture != null) {
                if (TextureTags.Is(inputChannel.Texture, TextureTags.MagnitudeBuffer)) {
                    mapping.SourceTag = TextureTags.MagnitudeBuffer;
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (TextureTags.Is(inputChannel.Texture, TextureTags.NormalGenerated)) {
                    mapping.SourceTag = TextureTags.NormalGenerated;
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (TryGetSourceFilename(inputChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (createEmpty && TextureTags.Is(inputChannel.Texture, TextureTags.Occlusion)) {
                    mapping.SourceTag = TextureTags.OcclusionGenerated;
                    mapping.InputColor = ColorChannel.Red;
                    mapping.InputMinValue = 0f;
                    mapping.InputMaxValue = 1f;
                    mapping.InputRangeMin = 0;
                    mapping.InputRangeMax = 255;
                    mapping.InputShift = 0;
                    mapping.InputPower = 1;
                    mapping.InputInverted = true;
                    return true;
                }
            }

            if (createEmpty) {
                var isOutputDiffuseRed = EncodingChannel.Is(outputChannel.ID, EncodingChannel.DiffuseRed);
                var isOutputDiffuseGreen = EncodingChannel.Is(outputChannel.ID, EncodingChannel.DiffuseGreen);
                var isOutputDiffuseBlue = EncodingChannel.Is(outputChannel.ID, EncodingChannel.DiffuseBlue);

                // Albedo Red > Diffuse Red
                if (isOutputDiffuseRed && context.InputEncoding.TryGetChannel(EncodingChannel.AlbedoRed, out var albedoRedChannel)) {
                    TryGetSourceFilename(albedoRedChannel.Texture, out mapping.SourceFilename);
                    if (context.Material.TryGetChannelValue(EncodingChannel.AlbedoRed, out value)) mapping.InputValue = (float)value;

                    mapping.ApplyInputChannel(albedoRedChannel);
                    mapping.OutputApplyOcclusion = true;
                    return true;
                }

                // Albedo Green > Diffuse Green
                if (isOutputDiffuseGreen && context.InputEncoding.TryGetChannel(EncodingChannel.AlbedoGreen, out var albedoGreenChannel)) {
                    TryGetSourceFilename(albedoGreenChannel.Texture, out mapping.SourceFilename);
                    if (context.Material.TryGetChannelValue(EncodingChannel.AlbedoGreen, out value)) mapping.InputValue = (float)value;

                    mapping.ApplyInputChannel(albedoGreenChannel);
                    mapping.OutputApplyOcclusion = true;
                    return true;
                }

                // Albedo Blue > Diffuse Blue
                if (isOutputDiffuseBlue && context.InputEncoding.TryGetChannel(EncodingChannel.AlbedoBlue, out var albedoBlueChannel)) {
                    TryGetSourceFilename(albedoBlueChannel.Texture, out mapping.SourceFilename);
                    if (context.Material.TryGetChannelValue(EncodingChannel.AlbedoBlue, out value)) mapping.InputValue = (float)value;

                    mapping.ApplyInputChannel(albedoBlueChannel);
                    mapping.OutputApplyOcclusion = true;
                    return true;
                }
            }
            
            // Rough > Smooth
            var isOutputSmooth = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Smooth);
            var hasOuputRough = context.OutputEncoding.HasChannel(EncodingChannel.Rough);
            if (isOutputSmooth && !hasOuputRough) {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Rough, out var roughChannel)
                    && TryGetSourceFilename(roughChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(roughChannel);
                    mapping.InputScale = (float) context.Material.GetChannelScale(EncodingChannel.Rough);
                    mapping.Invert = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Rough, out value)) {
                    mapping.ApplyInputChannel(inputChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputScale = (float) context.Material.GetChannelScale(EncodingChannel.Rough);
                    mapping.Invert = true;
                    return true;
                }
            }

            // Smooth > Rough
            var isOutputRough = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Rough);
            var hasOuputSmooth = context.OutputEncoding.HasChannel(EncodingChannel.Smooth);
            if (isOutputRough && !hasOuputSmooth) {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Smooth, out var smoothChannel)
                    && TryGetSourceFilename(smoothChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InputScale = (float) context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.Invert = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Smooth, out value)) {
                    mapping.ApplyInputChannel(inputChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputScale = (float) context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.Invert = true;
                    return true;
                }
            }

            //var isOutputF0 = EncodingChannel.Is(outputChannel.ID, EncodingChannel.F0);
            //var isOutputMetal = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Metal);

            //// Metal > F0
            //if (isOutputF0 && TryGetInputChannel(EncodingChannel.Metal, out var metalChannel)) {
            //    if (TryGetSourceFilename(metalChannel.Texture, out mapping.SourceFilename)) {
            //        mapping.ApplyInputChannel(metalChannel);
            //        //mapping.Threshold = 0.5f;
            //        mapping.IsMetalToF0 = true;
            //        return true;
            //    }
            //}

            //// F0 > Metal
            //if (isOutputMetal && TryGetInputChannel(EncodingChannel.F0, out var f0Channel)) {
            //    if (TryGetSourceFilename(f0Channel.Texture, out mapping.SourceFilename)) {
            //        mapping.ApplyInputChannel(f0Channel);
            //        //mapping.Threshold = 0.5f;
            //        mapping.IsF0ToMetal = true;
            //        return true;
            //    }
            //}

            var defaultValue = EncodingChannel.GetDefaultValue(outputChannel.ID);
            if (defaultValue.HasValue) mapping.InputValue = defaultValue.Value;

            return false;
        }

        private void ApplyNormalMapping(Image image, TextureChannelMapping mapping)
        {
            var sampler = normalGraph.GetNormalSampler();
            sampler.RangeX = (float)normalGraph.NormalTexture.Width / bufferSize.Width;
            sampler.RangeY = (float)normalGraph.NormalTexture.Height / bufferSize.Height;

            var options = new OverlayProcessor<Rgb24>.Options {
                IsGrayscale = isGrayscale,
                SamplerMap = new Dictionary<TextureChannelMapping, ISampler<Rgb24>> {
                    [mapping] = sampler,
                },
            };

            var processor = new OverlayProcessor<Rgb24>(options);

            foreach (var frame in regions.GetAllRenderRegions(TargetFrame, context.MaxFrameCount)) {
                var srcFrame = regions.GetRenderRegion(frame.Index, normalGraph.NormalFrameCount);

                foreach (var tile in frame.GetAllTiles(TargetPart)) {
                    sampler.Bounds = srcFrame.Tiles[tile.Index].Bounds;

                    var outBounds = TargetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                        : GetOutBounds(tile, frame).ScaleTo(image.Width, image.Height);

                    image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }

        private void ApplyMagnitudeMapping(Image image, TextureChannelMapping mapping)
        {
            var sampler = normalGraph.GetMagnitudeSampler();
            sampler.RangeX = (float)normalGraph.MagnitudeTexture.Width / bufferSize.Width;
            sampler.RangeY = (float)normalGraph.MagnitudeTexture.Height / bufferSize.Height;

            var options = new OverlayProcessor<L8>.Options {
                IsGrayscale = isGrayscale,
                SamplerMap = new Dictionary<TextureChannelMapping, ISampler<L8>> {
                    [mapping] = sampler,
                },
            };

            var processor = new OverlayProcessor<L8>(options);

            foreach (var frame in regions.GetAllRenderRegions(TargetFrame, context.MaxFrameCount)) {
                var srcFrame = regions.GetRenderRegion(frame.Index, normalGraph.MagnitudeFrameCount);

                foreach (var tile in frame.GetAllTiles(TargetPart)) {
                    sampler.Bounds = srcFrame.Tiles[tile.Index].Bounds;

                    var outBounds = TargetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                        : GetOutBounds(tile, frame).ScaleTo(image.Width, image.Height);

                    image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }

        private async Task ApplyOcclusionMappingAsync(Image image, TextureChannelMapping mapping, CancellationToken token)
        {
            var occlusionSampler = await occlusionGraph.GetSamplerAsync(token);
            if (occlusionSampler == null) return;

            var options = new OverlayProcessor<L8>.Options {
                IsGrayscale = isGrayscale,
                SamplerMap = new Dictionary<TextureChannelMapping, ISampler<L8>> {
                    [mapping] = occlusionSampler,
                },
            };
            
            var processor = new OverlayProcessor<L8>(options);

            foreach (var frame in regions.GetAllRenderRegions(TargetFrame, context.MaxFrameCount)) {
                var srcFrame = regions.GetRenderRegion(frame.Index, occlusionGraph.FrameCount);

                foreach (var tile in frame.GetAllTiles(TargetPart)) {
                    occlusionSampler.Bounds = srcFrame.Tiles[tile.Index].Bounds;

                    var outBounds = TargetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                        : GetOutBounds(tile, frame).ScaleTo(image.Width, image.Height);

                    image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }

        private void ApplyDefaultValue(TextureChannelMapping mapping)
        {
            if (!mapping.InputValue.HasValue && mapping.OutputShift == 0 && !mapping.OutputInverted) return;

            var value = mapping.InputValue ?? 0f;
            //if (value < mapping.InputMinValue || value > mapping.InputMaxValue) return;

            if (mapping.Invert) MathEx.Invert(ref value, mapping.InputMinValue, mapping.InputMaxValue);

            // TODO: scale

            mapping.Map(ref value, out byte finalValue);

            if (isGrayscale) {
                defaultValues.R = finalValue;
                defaultValues.G = finalValue;
                defaultValues.B = finalValue;
            }
            else {
                defaultValues.SetChannelValue(in mapping.OutputColor, in finalValue);
            }
        }

        private async Task ApplySourceMappingAsync<TPixel>(Image<TPixel> image, string sourceFilename, TextureChannelMapping[] mappingGroup, CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (sourceFilename == null) throw new ArgumentNullException(nameof(sourceFilename));

            var info = await sourceGraph.GetOrCreateAsync(sourceFilename, token);
            if (info == null) return;

            await using var sourceStream = reader.Open(sourceFilename);
            using var sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);

            var options = new OverlayProcessor<Rgba32>.Options {
                IsGrayscale = isGrayscale,
                SamplerMap = mappingGroup.ToDictionary(m => m, m => {
                    var samplerName = m.OutputSampler ?? context.DefaultSampler;
                    var sampler = context.CreateSampler(sourceImage, samplerName);
                    sampler.RangeX = (float)sourceImage.Width / bufferSize.Width;
                    sampler.RangeY = (float)(sourceImage.Height / info.FrameCount) / bufferSize.Height;
                    return sampler;
                }),
            };

            var processor = new OverlayProcessor<Rgba32>(options);

            foreach (var frame in regions.GetAllRenderRegions(TargetFrame, context.MaxFrameCount)) {
                var srcFrame = regions.GetRenderRegion(frame.Index, info.FrameCount);

                foreach (var tile in frame.GetAllTiles(TargetPart)) {
                    foreach (var sampler in options.SamplerMap.Values)
                        sampler.Bounds = srcFrame.Tiles[tile.Index].Bounds;

                    var outBounds = TargetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                        : GetOutBounds(tile, frame).ScaleTo(image.Width, image.Height);

                    image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }

        private async Task ApplyOutputOcclusionAsync<TPixel>(Image<TPixel> image, IEnumerable<TextureChannelMapping> mappingGroup, CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var occlusionSampler = await occlusionGraph.GetSamplerAsync(token);
            if (occlusionSampler == null) return;

            var options = new PostOcclusionProcessor<L8, Rgba32>.Options {
                MappingColors = mappingGroup.Select(m => m.OutputColor).ToArray(),
                OcclusionSampler = occlusionSampler,
                OcclusionMapping = new TextureChannelMapping(),
            };

            options.OcclusionMapping.ApplyInputChannel(occlusionGraph.Channel);
            options.OcclusionMapping.OutputScale = (float)context.Material.GetChannelScale(EncodingChannel.Occlusion);

            if (context.Profile?.DiffuseOcclusionStrength.HasValue ?? false)
                options.OcclusionMapping.OutputScale *= (float)context.Profile.DiffuseOcclusionStrength.Value;

            Image<Rgba32> emissiveImage = null;
            TextureSource emissiveInfo = null;
            ISampler<Rgba32> emissiveSampler = null;
            try {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Emissive, out var emissiveChannel)
                    && TryGetSourceFilename(emissiveChannel.Texture, out var emissiveFile)) {
                    emissiveInfo = await sourceGraph.GetOrCreateAsync(emissiveFile, token);
                    if (emissiveInfo != null) {
                        await using var stream = reader.Open(emissiveFile);
                        emissiveImage = await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);

                        var samplerName = context.Profile?.Encoding?.Emissive?.Sampler ?? context.DefaultSampler;
                        options.EmissiveSampler = emissiveSampler = context.CreateSampler(emissiveImage, samplerName);

                        // TODO: set these properly
                        emissiveSampler.RangeX = 1;
                        emissiveSampler.RangeY = 1;

                        options.EmissiveMapping = new TextureChannelMapping();
                        options.EmissiveMapping.ApplyInputChannel(emissiveChannel);

                        if (context.Material.TryGetChannelValue(EncodingChannel.Emissive, out var value))
                            options.EmissiveMapping.InputValue = (float)value;
                    }
                }

                var processor = new PostOcclusionProcessor<L8, Rgba32>(options);

                foreach (var frame in regions.GetAllRenderRegions(TargetFrame, context.MaxFrameCount)) {
                    var occlusionFrame = regions.GetRenderRegion(frame.Index, occlusionGraph.FrameCount);
                    var emissiveFrame = regions.GetRenderRegion(frame.Index, emissiveInfo?.FrameCount ?? 1);

                    foreach (var tile in frame.GetAllTiles(TargetPart)) {
                        occlusionSampler.Bounds = occlusionFrame.Tiles[tile.Index].Bounds;

                        if (emissiveSampler != null)
                            emissiveSampler.Bounds = emissiveFrame.Tiles[tile.Index].Bounds;

                        var outBounds = TargetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                            : GetOutBounds(tile, frame).ScaleTo(image.Width, image.Height);

                        image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                    }
                }
            }
            finally {
                emissiveImage?.Dispose();
            }
        }

        private RectangleF GetOutBounds(TextureRenderTile tile, TextureRenderFrame frame)
        {
            var frameBounds = tile.Bounds;
            if (TargetFrame.HasValue) {
                frameBounds.Offset(frame.Bounds.X, frame.Bounds.Y);

                // WARN: Removed for testing, seems to break animations
                //frameBounds.Height *= FrameCount;
            }
            return frameBounds;
        }

        private async Task<Size?> GetBufferSizeAsync(CancellationToken token = default)
        {
            var scale = context.TextureScale;
            var blockSize = context.Profile?.BlockTextureSize;

            // Use multi-part bounds if defined
            if (context.Material.TryGetSourceBounds(in blockSize, in scale, out var partBounds))
                return partBounds;

            var actualBounds = await GetActualBoundsAsync(token);

            float? aspect = null;
            if (actualBounds.HasValue) aspect = (float)actualBounds.Value.Height / actualBounds.Value.Width;

            var textureSize = context.GetTextureSize(aspect);

            // Use texture-size
            if (textureSize.HasValue)
                return textureSize.Value;

            if (actualBounds.HasValue) {
                var size = new Size(actualBounds.Value.Width, actualBounds.Value.Height);

                if (scale.HasValue) {
                    size.Width = (int)MathF.Ceiling(size.Width * scale.Value);
                    size.Height = (int)MathF.Ceiling(size.Height * scale.Value);
                }

                return size;
            }

            return null;
        }

        private async Task<Size?> GetActualBoundsAsync(CancellationToken token = default)
        {
            var hasBounds = false;
            var maxWidth = 1;
            var maxHeight = 1;

            foreach (var mappingGroup in mappings.GroupBy(m => m.SourceFilename)) {
                if (mappingGroup.Key == null) continue;
                if (!sourceGraph.TryGet(mappingGroup.Key, out var source)) continue;

                if (!hasBounds) {
                    maxWidth = source.Width;
                    maxHeight = source.Height;
                    hasBounds = true;
                    continue;
                }

                if (source.Width != maxWidth) {
                    var scale = (float)source.Width / maxWidth;
                    var scaledWidth = (int)MathF.Ceiling(source.Width * scale);
                    var scaledHeight = (int)MathF.Ceiling(source.Height * scale);

                    if (scaledWidth > maxWidth || scaledHeight > maxHeight) {
                        maxWidth = scaledWidth;
                        maxHeight = scaledHeight;
                    }
                }
                else {
                    if (source.Height > maxHeight) {
                        maxHeight = source.Height;
                    }
                }
            }

            foreach (var mapping in mappings.Where(m => m.SourceFilename == null)) {
                if (TextureTags.Is(mapping.SourceTag, TextureTags.NormalGenerated) && normalGraph.HasNormalTexture) {
                    if (!hasBounds) {
                        maxWidth = normalGraph.NormalFrameWidth;
                        maxHeight = normalGraph.NormalFrameHeight;
                        hasBounds = true;
                        continue;
                    }

                    if (normalGraph.NormalFrameWidth == maxWidth && normalGraph.NormalFrameHeight == maxHeight) continue;

                    if (normalGraph.NormalFrameWidth >= maxWidth) {
                        maxWidth = normalGraph.NormalFrameWidth;

                        if (normalGraph.NormalFrameHeight > maxHeight)
                            maxHeight = normalGraph.NormalFrameHeight;
                    }
                    else {
                        var scale = (float)maxWidth / normalGraph.NormalFrameWidth;
                        var scaledHeight = (int)MathF.Ceiling(normalGraph.NormalFrameHeight * scale);

                        if (scaledHeight > maxHeight)
                            maxHeight = scaledHeight;
                    }
                }

                if (TextureTags.Is(mapping.SourceTag, TextureTags.MagnitudeBuffer) && normalGraph.HasMagnitudeTexture) {
                    if (!hasBounds) {
                        maxWidth = normalGraph.MagnitudeFrameWidth;
                        maxHeight = normalGraph.MagnitudeFrameHeight;
                        hasBounds = true;
                        continue;
                    }

                    if (normalGraph.MagnitudeFrameWidth == maxWidth && normalGraph.MagnitudeFrameHeight == maxHeight) continue;

                    if (normalGraph.MagnitudeFrameWidth >= maxWidth) {
                        maxWidth = normalGraph.MagnitudeFrameWidth;

                        if (normalGraph.MagnitudeFrameHeight > maxHeight)
                            maxHeight = normalGraph.MagnitudeFrameHeight;
                    }
                    else {
                        var scale = (float)maxWidth / normalGraph.MagnitudeFrameWidth;
                        var scaledHeight = (int)MathF.Ceiling(normalGraph.MagnitudeFrameHeight * scale);

                        if (scaledHeight > maxHeight)
                            maxHeight = scaledHeight;
                    }
                }

                if (TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated) && occlusionGraph.HasTexture) {
                    var occlusionTex = await occlusionGraph.GetTextureAsync(token);

                    if (occlusionTex != null) {
                        if (!hasBounds) {
                            maxWidth = occlusionGraph.FrameWidth;
                            maxHeight = occlusionGraph.FrameHeight;
                            hasBounds = true;
                            continue;
                        }

                        if (occlusionGraph.FrameWidth == maxWidth && occlusionGraph.FrameHeight == maxHeight) continue;

                        if (occlusionGraph.FrameWidth >= maxWidth) {
                            maxWidth = occlusionGraph.FrameWidth;

                            if (occlusionGraph.FrameHeight > maxHeight)
                                maxHeight = occlusionGraph.FrameHeight;
                        }
                        else {
                            var scale = (float)maxWidth / occlusionGraph.FrameWidth;
                            var scaledHeight = (int)MathF.Ceiling(occlusionGraph.FrameHeight * scale);

                            if (scaledHeight > maxHeight)
                                maxHeight = scaledHeight;
                        }
                    }
                }
            }

            if (!hasBounds) return null;
            return new Size(maxWidth, maxHeight);
        }

        private bool TryGetSourceFilename(string tag, out string filename)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            var textureList = context.IsImport
                ? reader.EnumerateOutputTextures(context.Profile, context.Material.Name, context.Material.LocalPath, tag, true)
                : reader.EnumerateInputTextures(context.Material, tag);

            foreach (var file in textureList) {
                if (!reader.FileExists(file)) continue;

                filename = file;
                return true;
            }

            filename = null;
            return false;
        }
    }
}
