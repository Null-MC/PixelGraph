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
            float fx = 0f, fy = 0f;
            var hasChanges = false;

            if (options.SizeX > float.Epsilon) {
                var right = context.Bounds.Right - 1;
                var sizeX = (int) MathF.Ceiling(options.SizeX * context.Bounds.Width);
                var skipX = context.X > context.Bounds.Left + sizeX && context.X < right - sizeX;

                if (!skipX) {
                    if (options.EnableSmartEdges) {
                        //var pLeft = options.
                    }

                    var fLeft = 1f - Math.Clamp((context.X - context.Bounds.Left) / (float) sizeX, 0f, 1f);
                    var fRight = 1f - Math.Clamp((right - context.X) / (float) sizeX, 0f, 1f);
                    fx = MathF.Max(fLeft, fRight);
                    hasChanges = true;
                }
            }

            if (options.SizeY > float.Epsilon) {
                var bottom = context.Bounds.Bottom - 1;
                var sizeY = (int) MathF.Ceiling(options.SizeY * context.Bounds.Height);
                var skipY = context.Y > context.Bounds.Top + sizeY && context.Y < bottom - sizeY;

                if (!skipY) {
                    var fTop = 1f - Math.Clamp((context.Y - context.Bounds.Top) / (float) sizeY, 0f, 1f);
                    var fBottom = 1f - Math.Clamp((bottom - context.Y) / (float) sizeY, 0f, 1f);
                    fy = MathF.Max(fTop, fBottom);
                    hasChanges = true;
                }
            }

            if (!hasChanges) return;

            var fValue = Math.Max(fx, fy);

            var count = options.Colors.Length;
            for (var i = 0; i < count; i++) {
                pixelOut.GetChannelValueScaled(in options.Colors[i], out var srcValue);

                var avg = (srcValue + fValue) * 0.5f;
                var min = MathF.Max(srcValue, avg);
                var max = MathF.Max(min, fValue);
                if (srcValue.Equal(max)) continue;

                pixelOut.SetChannelValueScaled(in options.Colors[i], in max);
            }
        }

        public class Options
        {
            public ColorChannel[] Colors;
            public float SizeX, SizeY;
            public bool EnableSmartEdges;
        }
    }
}
