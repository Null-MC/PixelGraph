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
            var right = context.Width - 1;
            var bottom = context.Height - 1;

            var skipX = context.X > options.Size && context.X < right - options.Size;
            var skipY = context.Y > options.Size && context.Y < bottom - options.Size;
            if (skipX && skipY) return;

            var fLeft = 1f - Math.Clamp(context.X / (float) options.Size, 0f, 1f);
            var fTop = 1f - Math.Clamp(context.Y / (float) options.Size, 0f, 1f);

            var fRight = 1f - Math.Clamp((right - context.X) / (float) options.Size, 0f, 1f);
            var fBottom = 1f - Math.Clamp((bottom - context.Y) / (float) options.Size, 0f, 1f);

            var fx = Math.Max(fLeft, fRight);
            var fy = Math.Max(fTop, fBottom);
            var f = Math.Max(fx, fy);

            MathEx.Saturate(f, out var fValue);
            pixelOut.GetChannelValue(in options.Color, out var value);
            if (value < fValue) pixelOut.SetChannelValue(in options.Color, in fValue);
        }

        public class Options
        {
            public ColorChannel Color;
            public int Size;
        }
    }
}
