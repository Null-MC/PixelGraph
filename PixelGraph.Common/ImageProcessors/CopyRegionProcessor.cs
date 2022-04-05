using PixelGraph.Common.PixelOperations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class CopyRegionProcessor<TSource> : PixelRowProcessor
        where TSource : unmanaged, IPixel<TSource>
    {
        private readonly Options options;


        public CopyRegionProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> destRow)
        {
            var offsetY = context.Y - context.Bounds.Y;
            var srcY = options.SourceY + offsetY;
            var srcRow = options.SourceImage.DangerousGetPixelRowMemory(srcY).Span;

            for (var x = 0; x < context.Bounds.Width; x++) {
                var destX = context.Bounds.X + x;
                var srcX = options.SourceX + x;

                var pixel = srcRow[srcX].ToScaledVector4();
                destRow[destX].FromScaledVector4(pixel);
            }
        }

        public class Options
        {
            public Image<TSource> SourceImage;
            public int SourceX;
            public int SourceY;
        }
    }
}
