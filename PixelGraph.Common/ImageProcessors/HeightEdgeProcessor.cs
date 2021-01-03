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

            if (options.SizeX > 0) {
                var right = context.Bounds.Right - 1;

                var skipX = context.X > context.Bounds.Left + options.SizeX && context.X < right - options.SizeX;

                if (!skipX) {
                    if (options.EnableSmartEdges) {
                        //var pLeft = options.
                    }

                    var fLeft = 1f - Math.Clamp((context.X - context.Bounds.Left) / (float) options.SizeX, 0f, 1f);
                    var fRight = 1f - Math.Clamp((right - context.X) / (float) options.SizeX, 0f, 1f);
                    fx = MathF.Max(fLeft, fRight);
                    hasChanges = true;
                }
            }

            if (options.SizeY > 0) {
                var bottom = context.Bounds.Bottom - 1;

                var skipY = context.Y > context.Bounds.Top + options.SizeY && context.Y < bottom - options.SizeY;

                if (!skipY) {
                    var fTop = 1f - Math.Clamp((context.Y - context.Bounds.Top) / (float) options.SizeY, 0f, 1f);
                    var fBottom = 1f - Math.Clamp((bottom - context.Y) / (float) options.SizeY, 0f, 1f);
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
            public int SizeX, SizeY;
            public bool EnableSmartEdges;
        }
    }
}
