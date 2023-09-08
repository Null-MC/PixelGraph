using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;

namespace PixelGraph.Common.IO.Publishing;

public class GenericTexturePublisher
{
    private readonly ITextureGraphContext context;
    private readonly IPublishSummary summary;
    private readonly IInputReader reader;
    private readonly IOutputWriter writer;


    public GenericTexturePublisher(
        ITextureGraphContext context,
        IPublishSummary summary,
        IInputReader reader,
        IOutputWriter writer)
    {
        this.context = context;
        this.summary = summary;
        this.reader = reader;
        this.writer = writer;
    }

    public async Task PublishAsync(string sourceFile, Rgba32? sourceColor, string destinationFile, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(destinationFile))
            throw new ArgumentException("Value cannot be null or empty!", nameof(destinationFile));

        using var sourceImage = await LoadSourceImageAsync(sourceFile, sourceColor, token);
        using var resizedImage = Resize(sourceImage);
        var img = resizedImage ?? sourceImage;

        long diskSize = 0;
        await writer.OpenWriteAsync(destinationFile, async stream => {
            await img.SaveAsPngAsync(stream, token);
            await stream.FlushAsync(token);
            diskSize = stream.Length;
        }, token);

        summary.IncrementTextureCount();
        summary.AddDiskBytes(diskSize);
        summary.AddRawBytes(img.Width, img.Height);
    }

    private Image Resize<TPixel>(Image<TPixel> source)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (!context.Profile.TextureSize.HasValue && !context.Profile.TextureScale.HasValue) return null;

        var samplerName = context.Profile.Encoding?.Sampler ?? Samplers.Samplers.Nearest;
        var packSampler = Sampler<TPixel>.Create(samplerName);
        packSampler.Image = source;
        packSampler.WrapX = false;
        packSampler.WrapY = false;
        packSampler.SetBounds(UVRegion.Full);

        var (width, height) = source.Size;

        var options = new ResizeProcessor<TPixel>.Options {
            Sampler = packSampler,
        };

        int targetWidth, targetHeight;
        if (context.Profile.TextureSize.HasValue) {
            // Preserve aspect
            if (source.Width == context.Profile.TextureSize.Value) return null;

            var aspect = height / (float) width;
            targetWidth = context.Profile.TextureSize.Value;
            targetHeight = (int)(context.Profile.TextureSize.Value * aspect);
            packSampler.RangeX = source.Width / (float)context.Profile.TextureSize.Value;
            packSampler.RangeY = source.Height / (float)context.Profile.TextureSize.Value;
        }
        else {
            // scale all
            var scale = (float)context.Profile.TextureScale.Value;
            targetWidth = (int)Math.Max(width * scale, 1f);
            targetHeight = (int)Math.Max(height * scale, 1f);
            packSampler.RangeX = 1f / scale;
            packSampler.RangeY = 1f / scale;
        }

        var processor = new ResizeProcessor<TPixel>(options);
        var resizedImage = new Image<Rgba32>(Configuration.Default, targetWidth, targetHeight);

        try {
            resizedImage.Mutate(c => c.ApplyProcessor(processor));
            return resizedImage;
        }
        catch {
            resizedImage.Dispose();
            throw;
        }
    }

    private async Task<Image<Rgba32>> LoadSourceImageAsync(string sourceFile, Rgba32? sourceColor, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(sourceFile)) {
            await using var stream = reader.Open(sourceFile);
            return await Image.LoadAsync<Rgba32>(stream, token);
        }

        if (sourceColor.HasValue) {
            // TODO: support texture sizing?

            return new Image<Rgba32>(Configuration.Default, 1, 1, sourceColor.Value);
        }

        throw new SourceEmptyException("No Source image was found, and no color is defined!");
    }
}