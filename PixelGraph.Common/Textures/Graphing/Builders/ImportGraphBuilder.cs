using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures.Graphing.Builders;

public interface IImportGraphBuilder
{
    Task ImportAsync(CancellationToken token = default);
}

internal class ImportGraphBuilder : TextureGraphBuilder, IImportGraphBuilder
{
    private readonly ILogger<ImportGraphBuilder> logger;


    public ImportGraphBuilder(IServiceProvider provider, ILogger<ImportGraphBuilder> logger) : base(provider, logger)
    {
        this.logger = logger;
    }
        
    public async Task ImportAsync(CancellationToken token = default)
    {
        await ProcessAllTexturesAsync(false, token);
        await CopyPropertiesAsync(token);
        await ImportMetaAsync(token);
    }

    protected override async Task<long> SaveImagePartAsync<TPixel>(Image<TPixel> image, TexturePublishPart part, ImageChannels type, string destFile, string textureTag, CancellationToken token)
    {
        var srcWidth = image.Width;
        var srcHeight = image.Height;
        long diskSize;

        if (srcWidth == 1 && srcHeight == 1) {
            // TODO: set material values instead?

            diskSize = await ImageWriter.WriteAsync(image, type, destFile, token);
        }
        else {
            // TODO: set material values instead?

            var firstFrame = part.Frames.First();
            var frameCount = part.Frames.Length;
            var partWidth = (int)((firstFrame.SourceBounds.Right - firstFrame.SourceBounds.Left) * srcWidth);
            var partHeight = (int)((firstFrame.SourceBounds.Bottom - firstFrame.SourceBounds.Top) * srcHeight * frameCount);
            using var regionImage = new Image<TPixel>(partWidth, partHeight);

            foreach (var frame in part.Frames) {
                var sourceX = (int) (frame.SourceBounds.Left * srcWidth);
                var sourceY = (int) (frame.SourceBounds.Top * srcHeight);

                var outBounds = frame.DestBounds.ScaleTo(partWidth, partHeight);

                ImageProcessors.ImageProcessors.CopyRegion(image, sourceX, sourceY, regionImage, outBounds);
            }

            diskSize = await ImageWriter.WriteAsync(regionImage, type, destFile, token);
        }

        logger.LogInformation("Imported material texture '{destFile}'.", destFile);
        return diskSize;
    }
}