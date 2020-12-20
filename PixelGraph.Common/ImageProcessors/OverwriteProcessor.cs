using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.ImageProcessors
{
    internal class OverwriteProcessor : PixelProcessor
    {
        private readonly Options options;


        public OverwriteProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            pixelOut.SetChannelValue(in options.Color, in options.Value);
        }

        public class Options
        {
            public ColorChannel Color;
            public byte Value;
        }
    }
}
