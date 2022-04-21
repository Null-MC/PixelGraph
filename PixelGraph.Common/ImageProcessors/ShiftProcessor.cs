using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class ShiftProcessor<TSource> : PixelRowProcessor
        where TSource : unmanaged, IPixel<TSource>
    {
        private readonly Options options;


        public ShiftProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> row)
        {
            var pixel = new Rgba32();
            for (var x = context.Bounds.Left; x < context.Bounds.Right; x++) {
                row[x].ToRgba32(ref pixel);
                pixel.GetChannelValue(in options.Color, out var value);

                value = MathEx.ByteClamp(value + options.Offset);

                pixel.SetChannelValue(in options.Color, value);
                row[x].FromRgba32(pixel);
            }
        }

        public class Options
        {
            public ColorChannel Color;
            public int Offset;
        }
    }
}
