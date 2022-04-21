using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ResizeProcessor<TPixel> : PixelRowProcessor
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Options options;


        public ResizeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TPixel2>(in PixelRowContext context, Span<TPixel2> row)
        {
            var srcBounds = context.Bounds;
            if (srcBounds.IsEmpty) return;

            GetTexCoordY(in context, out double rfy);
            var rowSampler = options.Sampler.ForRow(in rfy);

            double fx, fy;
            Vector4 pixel;
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                GetTexCoord(in context, in x, out fx, out fy);
                rowSampler.SampleScaled(in fx, in fy, out pixel);
                row[x].FromScaledVector4(pixel);
            }
        }

        public class Options
        {
            public ISampler<TPixel> Sampler {get; set;}
        }
    }
}
