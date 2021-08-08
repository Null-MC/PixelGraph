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

        protected override void ProcessPixel<TPixel>(ref TPixel pixelOut, in PixelContext context)
        {
            float fx = 0f, fy = 0f;
            if (!TryGetFactors(ref fx, ref fy, in context)) return;

            var fadeValue = Math.Max(fx, fy);

            var tp = pixelOut.ToScaledVector4();

            var count = options.Colors.Length;

            var hasChanges = false;
            float srcValue, outVal;
            for (var i = 0; i < count; i++) {
                tp.GetChannelValue(in options.Colors[i], out srcValue);

                //avg = (srcValue + fValue) * 0.5f;
                //min = MathF.Max(srcValue, avg);
                //max = MathF.Max(min, fValue);
                MathEx.Lerp(in srcValue, in fadeValue, in options.Strength, out outVal);
                outVal = MathF.Max(srcValue, outVal);

                if (srcValue.NearEqual(outVal)) continue;

                tp.SetChannelValue(in options.Colors[i], in outVal);
                hasChanges = true;
            }

            if (hasChanges) pixelOut.FromScaledVector4(tp);
        }

        protected override void ProcessPixel(ref Rgba32 pixelOut, in PixelContext context)
        {
            float fx = 0f, fy = 0f;
            if (!TryGetFactors(ref fx, ref fy, in context)) return;

            var fadeValue = Math.Max(fx, fy);

            var count = options.Colors.Length;

            float srcValue, outVal;
            for (var i = 0; i < count; i++) {
                pixelOut.GetChannelValueScaled(in options.Colors[i], out srcValue);

                MathEx.Lerp(in srcValue, in fadeValue, in options.Strength, out outVal);
                outVal = MathF.Max(srcValue, outVal);

                //avg = (srcValue + fadeValue) * 0.5f;


                //min = MathF.Max(srcValue, avg);
                //max = MathF.Max(min, fadeValue);

                //var m2 = MathF.Max(srcValue, fadeValue);

                if (srcValue.NearEqual(outVal)) continue;

                pixelOut.SetChannelValueScaled(in options.Colors[i], in outVal);
            }
        }

        private bool TryGetFactors(ref float fx, ref float fy, in PixelContext context)
        {
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

            return hasChanges;
        }

        public class Options
        {
            public ColorChannel[] Colors;
            public float SizeX, SizeY;
            public float Strength = 1f;
            public bool EnableSmartEdges;
        }
    }
}
