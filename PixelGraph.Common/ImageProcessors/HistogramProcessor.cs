using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;

namespace PixelGraph.Common.ImageProcessors
{
    internal class HistogramProcessor<TSource> : PixelRowProcessor
        where TSource : unmanaged, IPixel<TSource>
    {
        private readonly Options options;


        public HistogramProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessRow<TPixel>(in PixelRowContext context, Span<TPixel> row)
        {
            var pixel = new Rgba32();
            byte min = 0, max = 0;

            for (var x = 0; x < context.Bounds.Width; x++) {
                row[context.Bounds.Left + x].ToRgba32(ref pixel);
                pixel.GetChannelValue(in options.Color, out var value);

                if (x == 0)
                    min = max = value;
                else {
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
            }

            options.SetRow(in context.Y, in max);
        }

        public class Options
        {
            private readonly byte[] rowMax;

            public ColorChannel Color;

            public byte Max => rowMax.Max();


            public Options(int height)
            {
                rowMax = new byte[height];
            }

            public void SetRow(in int y, in byte max)
            {
                rowMax[y] = max;
            }
        }
    }
}
