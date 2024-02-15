using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.ImageProcessors;

internal class ResizeProcessor<TPixel>(ResizeProcessor<TPixel>.Options options) : PixelRowProcessor
    where TPixel : unmanaged, IPixel<TPixel>
{
    protected override void ProcessRow<TPixel2>(in PixelRowContext context, Span<TPixel2> row)
    {
        ArgumentNullException.ThrowIfNull(options.Sampler);

        var srcBounds = context.Bounds;
        if (srcBounds.IsEmpty) return;

        GetTexCoordY(in context, out var rfy);
        var rowSampler = options.Sampler.ForRow(in rfy);

        for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
            GetTexCoord(in context, in x, out var fx, out var fy);
            rowSampler.SampleScaled(in fx, in fy, out var pixel);
            row[x].FromScaledVector4(pixel);
        }
    }

    public class Options
    {
        public ISampler<TPixel>? Sampler {get; set;}
    }
}
