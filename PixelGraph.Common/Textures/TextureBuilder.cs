using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace PixelGraph.Common.Textures;

public interface ITextureBuilder
{
    PackEncodingChannel[]? InputChannels {get; set;}
    PackEncodingChannel[]? OutputChannels {get; set;}
    bool HasMappedSources {get;}
    int FrameCount {get;}
    int? TargetFrame {get; set;}
    int? TargetPart {get; set;}

    Task MapAsync(bool createEmpty, CancellationToken token = default);
    Task<Image<TPixel>?> BuildAsync<TPixel>(bool createEmpty, Size? targetSize = null, CancellationToken token = default) where TPixel : unmanaged, IPixel<TPixel>;
}

internal class TextureBuilder(
    IServiceProvider provider,
    IInputReader reader,
    ITextureReader matReader,
    ITextureGraphContext context,
    ITextureSourceGraph sourceGraph,
    ITextureNormalGraph normalGraph,
    ITextureOcclusionGraph occlusionGraph) : ITextureBuilder
{
    private readonly List<TextureChannelMapping> mappings = new();
    private readonly Dictionary<ColorChannel, int> defaultPriorityValues = new();
    private Vector4 defaultValues;
    private Size bufferSize;
    private bool isGrayscale;

    public PackEncodingChannel[]? InputChannels {get; set;}
    public PackEncodingChannel[]? OutputChannels {get; set;}
    public bool HasMappedSources {get; private set;}
    public int FrameCount {get; private set;}
    public int? TargetFrame {get; set;}
    public int? TargetPart {get; set;}


    public async Task MapAsync(bool createEmpty, CancellationToken token = default)
    {
        //defaultPriorityValues = new Dictionary<ColorChannel, int>();
        defaultPriorityValues.Clear();
        defaultValues.X = defaultValues.Y = defaultValues.Z = defaultValues.W = 0;
        mappings.Clear();

        HasMappedSources = false;
        FrameCount = 1;

        if (OutputChannels == null) throw new ApplicationException("No output channels defined!");

        foreach (var channel in OutputChannels.OrderBy(c => c.Priority ?? 0)) {
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
        
    public async Task<Image<TPixel>?> BuildAsync<TPixel>(bool createEmpty, Size? targetSize = null, CancellationToken token = default)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (!createEmpty && mappings.Count == 0) return null;

        isGrayscale = mappings.All(x => x.OutputColor == ColorChannel.Red);
        if (isGrayscale) defaultValues.Z = defaultValues.Y = defaultValues.X;

        var autoLevel = context.Material?.Height?.AutoLevel
                        ?? context.Profile?.AutoLevelHeight
                        ?? PublishProfileProperties.AutoLevelHeightDefault;

        if (targetSize.HasValue) {
            bufferSize = targetSize.Value;
        }
        else {
            var size = await GetBufferSizeAsync(token);
            if (!size.HasValue && !createEmpty) return null;

            var defaultSize = 16;
            if (!size.HasValue && context.Profile != null) {
                defaultSize = context.GetFinalMaterialType() switch {
                    MaterialType.Block => context.Profile.BlockTextureSize ?? defaultSize,
                    MaterialType.Item => context.Profile.ItemTextureSize ?? defaultSize,
                    _ => defaultSize,
                };
            }

            bufferSize = size ?? new Size(defaultSize);
        }

        var width = bufferSize.Width;
        var height = bufferSize.Height;

        if (!TargetFrame.HasValue) {
            // WARN: ONLY KEEP ONE OF THESE! WHICH ONE??
            //if (context.MaxFrameCount > 1)
            //    height *= context.MaxFrameCount;

            // WARN: this one doesn't work for empty textures
            //if (FrameCount > 1)
            //    height *= FrameCount;

            var maxFrames = Math.Max(context.MaxFrameCount, FrameCount);
            if (maxFrames > 1) height *= maxFrames;
        }

        if (context.IsMaterialMultiPart && TargetPart.HasValue) {
            var regions = provider.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = FrameCount;
            regions.DestFrameCount = context.MaxFrameCount;

            var f = TargetFrame ?? 0;
            var part = regions.GetPublishPartFrame(f, TargetPart.Value);
            width = (int) Math.Ceiling(part.SourceBounds.Width * width);
            height = (int) Math.Ceiling(part.SourceBounds.Height * height);
        }

        // WARN: throw exception instead?
        if (width == 0 || height == 0) return null;

        var pixel = new TPixel();
        pixel.FromScaledVector4(defaultValues);
        var imageResult = new Image<TPixel>(Configuration.Default, width, height, pixel);

        var mappingsWithSources = mappings
            .Where(m => m.SourceFilename != null)
            .OrderBy(m => m.Priority)
            .GroupBy(m => m.SourceFilename);

        var mappingsWithoutSources = mappings
            .Where(m => m.SourceFilename == null)
            .OrderBy(m => m.Priority);

        var mappingsWithOutputOcclusion = mappings
            .Where(m => m.OutputApplyOcclusion)
            .ToArray();

        var mappingsWithHeight = mappings
            .Where(m => TextureTags.Is(m.OutputChannelID, TextureTags.Height))
            .ToArray();

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

            if (autoLevel && mappingsWithHeight.Any())
                foreach (var color in mappingsWithHeight.Select(m => m.OutputColor))
                    AutoLevelHeight(imageResult, color);

            return imageResult;
        }
        catch {
            imageResult.Dispose();
            throw;
        }
    }

    private bool TryBuildMapping(PackEncodingChannel outputChannel, bool createEmpty, out TextureChannelMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        mapping = new TextureChannelMapping {
            Sampler = outputChannel.Sampler ?? context.Profile?.Encoding?.GetSampler(outputChannel.ID),
        };

        mapping.ApplyOutputChannel(outputChannel);

        if (context.ApplyPostProcessing) {
            mapping.OutputValueScale = (float)context.Material.GetChannelScale(outputChannel.ID);
            mapping.OutputValueShift = (float)context.Material.GetChannelShift(outputChannel.ID);
        }
            
        // TODO: Override encoding with material?
        //if (context.Material.TryGetChannelDefaultValue(outputChannel.ID, out var defaultValue))
        //    mapping.OutputValueDefault = (float?)defaultValue;

        //if (context.Material.GetChannelClipValue(outputChannel.ID, out var clipValue))
        //    mapping.OutputClipValue = (float?)clipValue;
            
        if (context.BakeOcclusionToColor) {
            if (EncodingChannel.IsColor(outputChannel.ID))
                mapping.OutputApplyOcclusion = true;
        }

        var inputChannel = InputChannels?.FirstOrDefault(i => EncodingChannel.Is(i.ID, outputChannel.ID))
            ?? throw new ApplicationException("Input channel is undefined!");

        if (context.Material.TryGetChannelValue(outputChannel.ID, out var value)) {
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
                mapping.InputMinValue = -1f;
                mapping.InputMaxValue = 1f;
                return true;
            }

            //var path = context.Material.LocalPath;
            if (inputChannel.__Filename != null && matReader.TryGetByName(inputChannel.__Filename, out mapping.SourceFilename)) {
                mapping.ApplyInputChannel(inputChannel);
                return true;
            }
                
            if (matReader.TryGetByTag(inputChannel.Texture, out mapping.SourceFilename)) {
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
                mapping.InputChannelShift = 0;
                mapping.InputChannelPower = 1;
                mapping.InputChannelInverted = true;
                return true;
            }
        }
            
        var isOutputSmooth = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Smooth);
        if (isOutputSmooth) {
            // Rough > Smooth
            var hasOuputRough = context.OutputEncoding.HasChannel(EncodingChannel.Rough);
            if (!hasOuputRough) {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Rough, out var roughChannel) && roughChannel?.Texture != null
                    && matReader.TryGetByTag(roughChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(roughChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Rough);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Rough);
                    mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Rough, out value)) {
                    mapping.ApplyInputChannel(inputChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Rough);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Rough);
                    mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    return true;
                }
            }

            // Specular > Smooth
            var hasOuputSpecular = context.OutputEncoding.HasChannel(EncodingChannel.Specular);
            if (!hasOuputSpecular) {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Specular, out var specularChannel) && specularChannel?.Texture != null
                    && matReader.TryGetByTag(specularChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(specularChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Specular);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Specular);
                    //mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    //mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    //mapping.Convert_SpecularToSmooth = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Specular, out value)) {
                    mapping.ApplyInputChannel(inputChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Specular);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Specular);
                    //mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    //mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    mapping.InputValue = (float)value;
                    return true;
                }
            }
        }

        var isOutputRough = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Rough);
        if (isOutputRough) {
            // Smooth > Rough
            var hasOuputSmooth = context.OutputEncoding.HasChannel(EncodingChannel.Smooth);
            if (!hasOuputSmooth) {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Smooth, out var smoothChannel) && smoothChannel?.Texture != null
                    && matReader.TryGetByTag(smoothChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InputValueScale = (float)context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.InputValueShift = (float)context.Material.GetChannelShift(EncodingChannel.Smooth);
                    mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    //mapping.Convert_SmoothToRough = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Smooth, out value)) {
                    mapping.ApplyInputChannel(inputChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputValueScale = (float)context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.InputValueShift = (float)context.Material.GetChannelShift(EncodingChannel.Smooth);
                    mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;

                    return true;
                }
            }

            // Specular > Rough
            var hasOuputSpecular = context.OutputEncoding.HasChannel(EncodingChannel.Specular);
            if (!hasOuputSpecular) {
                if (context.InputEncoding.TryGetChannel(EncodingChannel.Specular, out var specularChannel) && specularChannel?.Texture != null
                    && matReader.TryGetByTag(specularChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(specularChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Specular);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Specular);
                    mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    //mapping.Convert_SpecularToRough = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Specular, out value)) {
                    mapping.ApplyInputChannel(inputChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Specular);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Specular);
                    //mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    //mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    //mapping.InputValue = MathEx.Invert((float)value,
                    //    (float?)outputChannel.MinValue ?? 0f,
                    //    (float?)outputChannel.MaxValue ?? 1f);

                    return true;
                }
            }
        }

        var isOutputSpecular = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Specular);
        if (isOutputSpecular && !context.IsImport) {
            // Smooth > Specular
            if (context.InputEncoding.TryGetChannel(EncodingChannel.Smooth, out var smoothChannel) && smoothChannel != null) {
                if (smoothChannel.Texture != null && matReader.TryGetByTag(smoothChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Smooth);
                    //mapping.Convert_SmoothToSpecular = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Smooth, out value)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Smooth);
                    return true;
                }
            }

            // Rough > Specular
            if (context.InputEncoding.TryGetChannel(EncodingChannel.Rough, out var roughChannel) && roughChannel != null) {
                if (roughChannel.Texture != null && matReader.TryGetByTag(roughChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(roughChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Rough);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Rough);
                    //mapping.Convert_RoughToSpecular = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Smooth, out value)) {
                    mapping.ApplyInputChannel(roughChannel);
                    mapping.InputValue = (float)value;
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Smooth);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Smooth);
                    mapping.OutputMinValue = (float?)outputChannel.MaxValue ?? 1f;
                    mapping.OutputMaxValue = (float?)outputChannel.MinValue ?? 0f;
                    return true;
                }
            }

            // Metal > Specular
            if (context.InputEncoding.TryGetChannel(EncodingChannel.Metal, out var metalChannel) && metalChannel != null) {
                if (metalChannel.Texture != null && matReader.TryGetByTag(metalChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(metalChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Metal);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Metal);
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Metal, out value)) {
                    var f = value > 0.5m ? 1f : 0f;
                    var min = (float?)metalChannel.MinValue ?? 0f;
                    var max = (float?)metalChannel.MaxValue ?? 1f;

                    mapping.ApplyInputChannel(metalChannel);
                    mapping.InputValue = min + f * (max - min);
                    return true;
                }
            }
        }

        var isOutputMetal = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Metal);
        if (isOutputMetal && !context.IsImport) {
            // HCM > Metal
            if (context.InputEncoding.TryGetChannel(EncodingChannel.HCM, out var hcmChannel) && hcmChannel != null) {
                if (hcmChannel.Texture != null && matReader.TryGetByTag(hcmChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(hcmChannel);
                    mapping.Convert_HcmToMetal = true;
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.HCM, out value)) {
                    mapping.ApplyInputChannel(hcmChannel);

                    mapping.InputValue = (float)value >= 230f
                        ? (float?)outputChannel.MaxValue ?? 1f
                        : (float?)outputChannel.MinValue ?? 0f;
                    
                    return true;
                }
            }

            // F0 > Metal
            if (context.InputEncoding.TryGetChannel(EncodingChannel.F0, out var f0Channel) && f0Channel != null) {
                if (f0Channel.Texture != null && matReader.TryGetByTag(f0Channel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(f0Channel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.F0);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.F0);
                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.F0, out value)) {
                    var f = value > 0.5m ? 1f : 0f;
                    var min = (float?)f0Channel.MinValue ?? 0f;
                    var max = (float?)f0Channel.MaxValue ?? 0.9f;

                    mapping.ApplyInputChannel(f0Channel);
                    mapping.InputValue = min + f * (max - min);
                    return true;
                }
            }
        }

        var isOutputHCM = EncodingChannel.Is(outputChannel.ID, EncodingChannel.HCM);
        if (isOutputHCM && !context.IsImport) {
            // Metal > HCM
            if (context.InputEncoding.TryGetChannel(EncodingChannel.Metal, out var metalChannel) && metalChannel != null) {
                if (metalChannel.Texture != null && matReader.TryGetByTag(metalChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(metalChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Metal);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Metal);
                    mapping.Convert_MetalToHcm = true;

                    return true;
                }

                if (context.Material.TryGetChannelValue(EncodingChannel.Metal, out value)) {
                    mapping.ApplyInputChannel(metalChannel);
                    mapping.InputValueScale = (float) context.Material.GetChannelScale(EncodingChannel.Metal);
                    mapping.InputValueShift = (float) context.Material.GetChannelShift(EncodingChannel.Metal);

                    var threshold = (mapping.InputMinValue + mapping.InputMaxValue) * 0.5f;
                    if ((float)value < threshold) return false;
                    mapping.InputValue = mapping.OutputMaxValue;
                    return true;
                }
            }
        }

        return false;
    }

    private void ApplyNormalMapping(Image image, TextureChannelMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(normalGraph.NormalTexture);

        var sampler = normalGraph.GetNormalSampler()
            ?? throw new ApplicationException("Sampler is undefined!");

        sampler.RangeX = (float)normalGraph.NormalTexture.Width / bufferSize.Width;
        sampler.RangeY = (float)normalGraph.NormalTexture.Height / bufferSize.Height / normalGraph.NormalFrameCount;

        var options = new OverlayProcessor<Rgb24>.Options {
            IsGrayscale = isGrayscale,
            Samplers = [
                new OverlayProcessor<Rgb24>.SamplerOptions {
                    PixelMap = new PixelMapping(mapping),
                    InputColor = mapping.InputColor,
                    OutputColor = mapping.OutputColor,
                    //Invert = mapping.Invert,
                    //ValueShift = mapping.ValueShift,
                    Sampler = sampler,
                }
            ],
        };

        var processor = new OverlayProcessor<Rgb24>(options);

        var regions = provider.GetRequiredService<TextureRegionEnumerator>();
        regions.SourceFrameCount = normalGraph.NormalFrameCount;
        regions.DestFrameCount = context.MaxFrameCount;
        regions.TargetFrame = TargetFrame;
        regions.TargetPart = TargetPart;

        foreach (var frame in regions.GetAllRenderRegions()) {
            if (frame.Tiles == null) continue;

            foreach (var tile in frame.Tiles) {
                sampler.SetBounds(tile.SourceBounds);

                var outBounds = tile.DestBounds.ScaleTo(image.Width, image.Height);

                image.Mutate(c => c.ApplyProcessor(processor, outBounds));
            }
        }
    }

    private void ApplyMagnitudeMapping(Image image, TextureChannelMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(normalGraph.MagnitudeTexture);

        var sampler = normalGraph.GetMagnitudeSampler()
            ?? throw new ApplicationException("Sampler is undefined!");

        sampler.RangeX = (float)normalGraph.MagnitudeTexture.Width / bufferSize.Width;
        sampler.RangeY = (float)normalGraph.MagnitudeTexture.Height / bufferSize.Height;

        var options = new OverlayProcessor<L8>.Options {
            IsGrayscale = isGrayscale,
            Samplers = [
                new OverlayProcessor<L8>.SamplerOptions {
                    PixelMap = new PixelMapping(mapping),
                    InputColor = mapping.InputColor,
                    OutputColor = mapping.OutputColor,
                    //Invert = mapping.Invert,
                    //ValueShift = mapping.ValueShift,
                    Sampler = sampler,
                }
            ],
        };

        var processor = new OverlayProcessor<L8>(options);

        var regions = provider.GetRequiredService<TextureRegionEnumerator>();
        regions.SourceFrameCount = normalGraph.MagnitudeFrameCount;
        regions.DestFrameCount = context.MaxFrameCount;
        regions.TargetFrame = TargetFrame;
        regions.TargetPart = TargetPart;

        foreach (var frame in regions.GetAllRenderRegions()) {
            if (frame.Tiles == null) continue;

            foreach (var tile in frame.Tiles) {
                sampler.SetBounds(tile.SourceBounds);

                var outBounds = tile.DestBounds.ScaleTo(image.Width, image.Height);

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
            Samplers = [
                new OverlayProcessor<L8>.SamplerOptions {
                    PixelMap = new PixelMapping(mapping),
                    InputColor = mapping.InputColor,
                    OutputColor = mapping.OutputColor,
                    //Invert = mapping.Invert,
                    //ValueShift = mapping.ValueShift,
                    Sampler = occlusionSampler,
                }
            ],
        };
            
        var processor = new OverlayProcessor<L8>(options);

        var regions = provider.GetRequiredService<TextureRegionEnumerator>();
        regions.SourceFrameCount = occlusionGraph.FrameCount;
        regions.DestFrameCount = context.MaxFrameCount;
        regions.TargetFrame = TargetFrame;
        regions.TargetPart = TargetPart;

        foreach (var frame in regions.GetAllRenderRegions()) {
            if (frame.Tiles == null) continue;

            foreach (var tile in frame.Tiles) {
                occlusionSampler.SetBounds(tile.SourceBounds);

                var outBounds = tile.DestBounds.ScaleTo(image.Width, image.Height);

                image.Mutate(c => c.ApplyProcessor(processor, outBounds));
            }
        }
    }

    private void ApplyDefaultValue(TextureChannelMapping mapping)
    {
        //if (!mapping.InputValue.HasValue && mapping.OutputChannelShift == 0 && !mapping.OutputChannelInverted) return;
            
        // WARN: This was disabled because it broke something
        // TODO: having this on breaks normal maps and vanilla pbr
        // TODO: having this off breaks LabPbr layering
        //if (mapping.OutputRangeMax > mapping.OutputRangeMin) {
        if (!mapping.InputValue.HasValue && !mapping.OutputValueDefault.HasValue) return;
        //}

        var value = mapping.InputValue ?? mapping.OutputValueDefault ?? 0f;
        if (mapping.OutputClipValue.HasValue && value.NearEqual(mapping.OutputClipValue.Value)) return;
        //if (value < mapping.InputMinValue || value > mapping.InputMaxValue) return;

        //if (mapping.Invert) MathEx.Invert(ref value, mapping.InputMinValue, mapping.InputMaxValue);

        // TODO: scale

        var pixelMap = new PixelMapping(mapping);

        if (!pixelMap.TryMap(ref value, out var finalValue)) return;

        if (isGrayscale) {
            defaultValues.X = finalValue;
            defaultValues.Y = finalValue;
            defaultValues.Z = finalValue;

            if (mapping.InputValue.HasValue) {
                defaultPriorityValues[ColorChannel.Red] = mapping.Priority;
                defaultPriorityValues[ColorChannel.Green] = mapping.Priority;
                defaultPriorityValues[ColorChannel.Blue] = mapping.Priority;
            }
        }
        else {
            defaultValues.SetChannelValue(in mapping.OutputColor, in finalValue);

            if (mapping.InputValue.HasValue)
                defaultPriorityValues[mapping.OutputColor] = mapping.Priority;
        }
    }

    private async Task<Image<TPixel>> GetSourceImageAsync<TPixel>(string filename, CancellationToken token)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (string.Equals(filename, "<missing>", StringComparison.InvariantCultureIgnoreCase))
            return BuildMissingImage<TPixel>(2);

        await using var sourceStream = reader.Open(filename)
            ?? throw new ApplicationException("Failed to open file stream!");

        var sourceImage = await Image.LoadAsync<TPixel>(sourceStream, token);
        return sourceImage;
    }

    private static Image<TPixel> BuildMissingImage<TPixel>(int size)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        // TODO: Use a loop to populate full bounds

        var defaultValue = new TPixel();
        defaultValue.FromRgba32(new Rgba32(0, 0, 0, 255));

        var pixel = new TPixel();
        pixel.FromRgb24(new Rgb24(248, 0, 248));

        return new Image<TPixel>(Configuration.Default, size, size, defaultValue) {
            [1, 0] = pixel,
            [0, 1] = pixel,
        };
    }

    private async Task ApplySourceMappingAsync<TPixel>(Image<TPixel> image, string sourceFilename, IEnumerable<TextureChannelMapping> mappingGroup, CancellationToken token)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (sourceFilename == null) throw new ArgumentNullException(nameof(sourceFilename));

        TextureSource? info;
        if (string.Equals(sourceFilename, "<missing>", StringComparison.InvariantCultureIgnoreCase)) {
            info = new TextureSource {
                FrameCount = 1,
                Width = 2,
                Height = 2,
            };
        }
        else {
            info = await sourceGraph.GetOrCreateAsync(sourceFilename, token);
            if (info == null) return;
        }

        async Task DoItAsync<T>()
            where T : unmanaged, IPixel<T>
        {
            using var sourceImage = await GetSourceImageAsync<T>(sourceFilename, token);
            ApplySourceMappingInternal(image, sourceImage, info, mappingGroup);
        }

        var map = new Dictionary<Type, Func<Task>> {
            [typeof(Rgba64)] = DoItAsync<Rgba64>,
            [typeof(Rgba32)] = DoItAsync<Rgba32>,
            [typeof(Rgb48)] = DoItAsync<Rgb48>,
            [typeof(Rgb24)] = DoItAsync<Rgb24>,
            [typeof(La32)] = DoItAsync<La32>,
            [typeof(La16)] = DoItAsync<La16>,
            [typeof(L16)] = DoItAsync<L16>,
            [typeof(L8)] = DoItAsync<L8>,
        };

        await map[info.GetPixelType()]();
    }

    private void ApplySourceMappingInternal<TPixel, TSource>(Image<TPixel> image, Image<TSource> sourceImage, TextureSource info, IEnumerable<TextureChannelMapping> mappingGroup)
        where TPixel : unmanaged, IPixel<TPixel>
        where TSource : unmanaged, IPixel<TSource>
    {
        var options = new OverlayProcessor<TSource>.Options {
            IsGrayscale = isGrayscale,
            Samplers = mappingGroup
                .Where(m => {
                    if (!defaultPriorityValues.TryGetValue(m.OutputColor, out var priority)) return true;
                    return m.Priority >= priority;
                })
                .OrderBy(m => m.Priority)
                .Select(m => {
                    var samplerName = m.Sampler ?? context.DefaultSampler;
                    var sampler = context.CreateSampler(sourceImage, samplerName);
                    sampler.RangeX = (float)sourceImage.Width / bufferSize.Width;
                    sampler.RangeY = (float)(sourceImage.Height / info.FrameCount) / bufferSize.Height;

                    return new OverlayProcessor<TSource>.SamplerOptions {
                        PixelMap = new PixelMapping(m),
                        InputColor = m.InputColor,
                        OutputColor = m.OutputColor,
                        //Invert = m.Invert,
                        //ValueShift = m.ValueShift,
                        Sampler = sampler,
                    };
                }).ToArray(),
        };

        var processor = new OverlayProcessor<TSource>(options);
        var regions = provider.GetRequiredService<TextureRegionEnumerator>();
        regions.SourceFrameCount = info.FrameCount;

        //regions.DestFrameCount = info.FrameCount;
        regions.DestFrameCount = TargetFrame.HasValue ? 1 :
            Math.Max(context.MaxFrameCount, info.FrameCount);

        regions.TargetFrame = TargetFrame;
        regions.TargetPart = TargetPart;

        foreach (var frame in regions.GetAllRenderRegions()) {
            if (frame.Tiles == null) continue;

            foreach (var tile in frame.Tiles) {
                foreach (var samplerOptions in options.Samplers)
                    samplerOptions.Sampler?.SetBounds(tile.SourceBounds);

                var outBounds = tile.DestBounds.ScaleTo(image.Width, image.Height);

                image.Mutate(c => c.ApplyProcessor(processor, outBounds));
            }
        }
    }

    private async Task ApplyOutputOcclusionAsync<TPixel>(Image<TPixel> image, IEnumerable<TextureChannelMapping> mappingGroup, CancellationToken token)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var occlusionSampler = await occlusionGraph.GetSamplerAsync(token);
        if (occlusionSampler == null) return;

        ArgumentNullException.ThrowIfNull(context.Material);

        var occlusionMap = new TextureChannelMapping();
        occlusionMap.ApplyInputChannel(occlusionGraph.Channel);
        occlusionMap.OutputValueScale = (float)context.Material.GetChannelScale(EncodingChannel.Occlusion);

        if (context.Profile?.DiffuseOcclusionStrength.HasValue ?? false)
            occlusionMap.OutputValueScale *= (float)context.Profile.DiffuseOcclusionStrength.Value;

        var options = new PostOcclusionProcessor<L8, Rgba32>.Options {
            MappingColors = mappingGroup.Select(m => m.OutputColor).ToArray(),
            OcclusionInputColor = occlusionMap.InputColor,
            OcclusionMapping = new PixelMapping(occlusionMap),
            OcclusionSampler = occlusionSampler,
        };

        Image<Rgba32>? emissiveImage = null;
        ISampler<Rgba32>? emissiveSampler = null;
        try {
            if (context.InputEncoding.TryGetChannel(EncodingChannel.Emissive, out var emissiveChannel) && emissiveChannel?.Texture != null
                && matReader.TryGetByTag(emissiveChannel.Texture, out var emissiveFile) && emissiveFile != null) {
                var emissiveInfo = await sourceGraph.GetOrCreateAsync(emissiveFile, token);

                if (emissiveInfo != null) {
                    await using var stream = reader.Open(emissiveFile)
                        ?? throw new ApplicationException("Failed to open file stream!");

                    emissiveImage = await Image.LoadAsync<Rgba32>(stream, token);

                    var samplerName = context.Profile?.Encoding?.Emissive.Sampler ?? context.DefaultSampler;
                    options.EmissiveSampler = emissiveSampler = context.CreateSampler(emissiveImage, samplerName);

                    // TODO: set these properly
                    emissiveSampler.RangeX = 1;
                    emissiveSampler.RangeY = 1;

                    var emissiveMap = new TextureChannelMapping();
                    emissiveMap.ApplyInputChannel(emissiveChannel);

                    if (context.Material.TryGetChannelValue(EncodingChannel.Emissive, out var value))
                        emissiveMap.InputValue = (float)value;

                    options.EmissiveInputColor = emissiveMap.InputColor;
                    options.EmissiveMapping = new PixelMapping(emissiveMap);
                }
            }

            var processor = new PostOcclusionProcessor<L8, Rgba32>(options);

            var regions = provider.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = occlusionGraph.FrameCount;
            regions.DestFrameCount = context.MaxFrameCount;
            regions.TargetFrame = TargetFrame;
            regions.TargetPart = TargetPart;

            foreach (var frame in regions.GetAllRenderRegions()) {
                if (frame.Tiles == null) continue;

                foreach (var tile in frame.Tiles) {
                    emissiveSampler?.SetBounds(tile.SourceBounds);

                    occlusionSampler.SetBounds(tile.SourceBounds);

                    var outBounds = tile.DestBounds.ScaleTo(image.Width, image.Height);

                    image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }
        }
        finally {
            emissiveImage?.Dispose();
        }
    }

    private static void AutoLevelHeight<TPixel>(Image<TPixel> image, ColorChannel color)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var offset = 1f - ImageProcessors.ImageProcessors.GetMaxValue(image, color);
        if (offset < float.Epsilon) return;

        ImageProcessors.ImageProcessors.Shift(image, color, offset);
    }

    private async Task<Size?> GetBufferSizeAsync(CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        var scale = context.TextureScale;
        var blockSize = context.Profile?.BlockTextureSize;

        if (blockSize.HasValue && context.IsMaterialCtm && TargetPart.HasValue)
            return new Size(blockSize.Value);

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
            if (!sourceGraph.TryGet(mappingGroup.Key, out var source) || source == null) continue;

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

    //private bool TryGetSourceFilename(string tag, out string filename)
    //{
    //    if (tag == null) throw new ArgumentNullException(nameof(tag));

    //    var textureList = context.IsImport
    //        ? reader.EnumerateOutputTextures(context.Material.Name, context.Material.LocalPath, tag, true)
    //        : reader.EnumerateInputTextures(context.Material, tag);

    //    foreach (var file in textureList) {
    //        if (!reader.FileExists(file)) continue;

    //        filename = file;
    //        return true;
    //    }

    //    filename = null;
    //    return false;
    //}
}