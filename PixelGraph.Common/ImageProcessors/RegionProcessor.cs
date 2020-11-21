using PixelGraph.Common.PixelOperations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.ImageProcessors
{
    internal class RegionProcessor : PixelComposeProcessor
    {
        private readonly Options options;


        public RegionProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var pixelIn = options.Source[
                options.Offset.X + context.X,
                options.Offset.Y + context.Y];

            pixelOut.FromRgba32(pixelIn);
        }

        public class Options
        {
            public Image<Rgba32> Source {get; set;}
            public Point Offset;
        }
    }
}
