using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PixelGraph.Common.Textures.Graphing;

public interface ITextureOcclusionGraph
{
    PackEncodingChannel? Channel {get;}
    int FrameCount {get;}
    int FrameWidth {get;}
    int FrameHeight {get;}
    bool HasTexture {get;}

    Task<Image<L8>?> GetTextureAsync(CancellationToken token = default);
    Task<Image<L8>?> GenerateAsync(CancellationToken token = default);
    Task<ISampler<L8>?> GetSamplerAsync(CancellationToken token = default);
}
    
internal class TextureOcclusionGraph(
    IServiceProvider provider,
    ITextureGraphContext context,
    ITextureHeightGraph heightGraph)
    : ITextureOcclusionGraph, IDisposable
{
    private Image<L8>? texture;
    private bool isLoaded;

    public PackEncodingChannel? Channel {get; private set;}
    public int FrameCount {get; private set;} = 1;
    public int FrameWidth {get; private set;}
    public int FrameHeight {get; private set;}
    public bool HasTexture => texture != null;


    public void Dispose()
    {
        texture?.Dispose();
    }

    public async Task<Image<L8>?> GetTextureAsync(CancellationToken token = default)
    {
        if (isLoaded) return texture;
        isLoaded = true;

        var bufferChannel = new ResourcePackOcclusionChannelProperties {
            Color = ColorChannel.Red,
        };
        var (image, frames) = await ExtractInputAsync<L8>(EncodingChannel.Occlusion, bufferChannel, token);

        if (image != null) {
            texture = image;
            FrameCount = frames;

            var outputChannel = context.OutputEncoding.GetChannel(EncodingChannel.Occlusion);

            Channel = new ResourcePackOcclusionChannelProperties {
                Sampler = outputChannel?.Sampler,
                Color = ColorChannel.Red,
                //Invert = true,
            };
        }

        if (context.AutoGenerateOcclusion && !context.IsImport)
            texture ??= await GenerateAsync(token);

        if (texture != null) {
            FrameWidth = texture.Width;
            FrameHeight = texture.Height;
            if (FrameCount > 1) FrameHeight /= FrameCount;
        }

        return texture;
    }

    public async Task<ISampler<L8>?> GetSamplerAsync(CancellationToken token = default)
    {
        var occlusionTexture = await GetTextureAsync(token);
        if (occlusionTexture == null) return null;

        var samplerName = Channel?.Sampler ?? context.DefaultSampler;
        var sampler = context.CreateSampler(occlusionTexture, samplerName);

        // TODO: SET THESE PROPERLY!
        sampler.RangeX = 1f;
        sampler.RangeY = 1f;

        return sampler;
    }

    public async Task<Image<L8>?> GenerateAsync(CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        var heightImage = await heightGraph.GetOrCreateAsync(token);
        if (heightImage == null) return null;

        FrameCount = heightGraph.HeightFrameCount;

        var aspect = (float)heightImage.Height / heightImage.Width;
        var bufferSize = context.GetBufferSize(aspect);

        var occlusionWidth = bufferSize?.Width ?? heightImage.Width;
        var occlusionHeight = bufferSize?.Height ?? heightImage.Height;

        var heightScale = 1f;
        if (bufferSize.HasValue)
            heightScale = (float)bufferSize.Value.Width / heightImage.Width;

        var heightSampler = context.CreateSampler(heightImage, Samplers.Samplers.Nearest);

        heightSampler.RangeX = (float)heightImage.Width / occlusionWidth;
        heightSampler.RangeY = (float)heightImage.Height / occlusionHeight;

        var quality = (float) (context.Profile?.OcclusionQuality ?? PublishProfileProperties.DefaultOcclusionQuality);
        var stepDistance = (float)(context.Material.Occlusion.StepDistance ?? MaterialOcclusionProperties.DefaultStepDistance);
        var zScale = (float) (context.Material.Occlusion.ZScale ?? MaterialOcclusionProperties.DefaultZScale);
        var zBias = (float) (context.Material.Occlusion.ZBias ?? MaterialOcclusionProperties.DefaultZBias);
        var hitPower = (float)(context.Profile?.OcclusionPower ?? PublishProfileProperties.DefaultOcclusionPower);

        // adjust volume height with texture scale
        zBias *= heightScale;
        zScale *= heightScale;

        var heightMapping = new TextureChannelMapping();
        heightMapping.ApplyInputChannel(new ResourcePackHeightChannelProperties {
            Texture = TextureTags.Height,
            Color = ColorChannel.Red,
            Invert = true,
        });

        heightMapping.OutputValueScale = (float)context.Material.GetChannelScale(EncodingChannel.Height);

        //OcclusionGpuProcessor<Rgba32>.Options gpuOptions = null;
        //OcclusionGpuProcessor<Rgba32> gpuProcessor = null;

        var size = GetTileSize(in occlusionWidth);

        var options = new OcclusionProcessorOptions {
            HeightInputColor = heightMapping.InputColor,
            HeightMapping = new PixelMapping(heightMapping),
            HeightSampler = heightSampler,
            Quality = quality,
            ZScale = zScale,
            ZBias = zBias,
            HitPower = hitPower,
            Token = token,

            StepCount = (int) MathF.Max(size * stepDistance, 1f),
        };

        // adjust volume height with texture scale
        options.ZBias *= heightScale;
        options.ZScale *= heightScale;

        var processor = new OcclusionProcessor<L16>(options);

        var occlusionTexture = new Image<L8>(Configuration.Default, occlusionWidth, occlusionHeight);
        var outputChannel = context.OutputEncoding.GetChannel(EncodingChannel.Occlusion);

        Channel = new ResourcePackOcclusionChannelProperties {
            Sampler = outputChannel?.Sampler,
            Color = ColorChannel.Red,
            Invert = true,
        };

        var regions = provider.GetRequiredService<TextureRegionEnumerator>();
        regions.SourceFrameCount = FrameCount;
        regions.DestFrameCount = FrameCount;
        //regions.TargetFrame = 0;
        //regions.TargetPart = TargetPart;

        try {
            foreach (var frame in regions.GetAllRenderRegions()) {
                if (frame.Tiles == null) continue;

                foreach (var tile in frame.Tiles) {
                    heightSampler.SetBounds(tile.SourceBounds);

                    var outBounds = tile.DestBounds.ScaleTo(occlusionWidth, occlusionHeight);
                    processor.PopulateNearField(heightImage, outBounds);

                    occlusionTexture.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }

            return occlusionTexture;
        }
        catch (ImageProcessingException) when (token.IsCancellationRequested) {
            throw new OperationCanceledException();
        }
        catch {
            occlusionTexture.Dispose();
            throw;
        }
    }

    private async Task<(Image<TPixel>? image, int frameCount)> ExtractInputAsync<TPixel>(string inputEncodingChannel, PackEncodingChannel outputChannel, CancellationToken token)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var inputChannel = context.InputEncoding.GetChannel(inputEncodingChannel);
        if (!(inputChannel?.HasMapping ?? false)) return (null, 0);

        using var scope = provider.CreateScope();
        var subContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
        var builder = scope.ServiceProvider.GetRequiredService<ITextureBuilder>();

        subContext.Project = context.Project;
        subContext.Profile = context.Profile;
        subContext.Material = context.Material;
        subContext.IsAnimated = context.IsAnimated;
        subContext.InputEncoding.Add(inputChannel);
        //subContext.PackWriteTime = ;

        builder.InputChannels = [inputChannel];
        builder.OutputChannels = [outputChannel];

        await builder.MapAsync(false, token);
        if (!builder.HasMappedSources) return (null, 0);

        Image<TPixel>? image = null;
        try {
            image = await builder.BuildAsync<TPixel>(false, null, token);
            return (image, builder.FrameCount);
        }
        catch {
            image?.Dispose();
            throw;
        }
    }

    private int GetTileSize(in int imageWidth)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        var profileSize = context.Profile?.BlockTextureSize ?? context.Profile?.TextureSize;
        if (profileSize.HasValue) return profileSize.Value;

        if (context.Material.CTM?.Method != null) {
            var bounds = CtmTypes.GetBounds(context.Material.CTM);
            var w = context.Material.CTM.Width ?? bounds?.Width;
            //var h = context.Material.CTM.Height ?? bounds.Height;

            if (w.HasValue) return imageWidth / w.Value;
        }

        return imageWidth;
    }
}