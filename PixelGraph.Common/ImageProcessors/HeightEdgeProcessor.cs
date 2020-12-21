using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.ImageProcessors
{
    internal class HeightEdgeProcessor : PixelProcessor
    {
        private readonly Options options;


        public HeightEdgeProcessor(in Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            var right = context.Bounds.Right - 1;
            var bottom = context.Bounds.Bottom - 1;

            var skipX = context.X > context.Bounds.Left + options.Size && context.X < right - options.Size;
            var skipY = context.Y > context.Bounds.Top + options.Size && context.Y < bottom - options.Size;
            if (skipX && skipY) return;

            var fLeft = 1f - Math.Clamp((context.X - context.Bounds.Left) / (float) options.Size, 0f, 1f);
            var fTop = 1f - Math.Clamp((context.Y - context.Bounds.Top) / (float) options.Size, 0f, 1f);

            var fRight = 1f - Math.Clamp((right - context.X) / (float) options.Size, 0f, 1f);
            var fBottom = 1f - Math.Clamp((bottom - context.Y) / (float) options.Size, 0f, 1f);

            var fx = Math.Max(fLeft, fRight);
            var fy = Math.Max(fTop, fBottom);
            var f = Math.Max(fx, fy);

            MathEx.Saturate(f, out var fValue);

            foreach (var color in options.Colors) {
                pixelOut.GetChannelValue(in color, out var value);
                if (value < fValue) pixelOut.SetChannelValue(in color, in fValue);
            }
        }

        public class Options
        {
            public ColorChannel[] Colors;
            public int Size;
        }
    }
}
