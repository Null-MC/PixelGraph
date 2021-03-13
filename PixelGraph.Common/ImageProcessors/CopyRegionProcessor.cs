using PixelGraph.Common.PixelOperations;
using SixLabors.ImageSharp;
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

        protected override void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> sourceRow)
        {
            var offsetY = context.Y - context.Bounds.Y;
            var destY = options.SourceY + offsetY;
            var destRow = options.SourceImage.GetPixelRowSpan(destY);

            for (var x = 0; x < context.Bounds.Width; x++) {
                var sourceX = context.Bounds.X + x;
                var destX = options.SourceX + x;

                var pixel = sourceRow[sourceX].ToScaledVector4();
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
