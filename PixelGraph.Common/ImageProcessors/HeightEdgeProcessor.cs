using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class HeightEdgeProcessor
    {
        public Rectangle Bounds;
        public ColorChannel[] Colors;
        public float SizeX, SizeY;
        public float Strength = 1f;


        public void Apply(Image image)
        {
            image.Mutate(c => c.ProcessPixelRowsAsVector4(ProcessPixel));
        }

        private void ProcessPixel(Span<Vector4> row, Point pos)
        {
            var count = Colors.Length;

            for (var x = 0; x < Bounds.Width; x++) {
                float fx = 0f, fy = 0f;
                if (!TryGetFactors(in x, pos.Y, ref fx, ref fy)) return;

                var fadeValue = Math.Max(fx, fy);

                for (var i = 0; i < count; i++) {
                    row[x].GetChannelValue(in Colors[i], out var srcValue);
                    
                    MathEx.Lerp(in srcValue, in fadeValue, in Strength, out var outVal);
                    outVal = MathF.Max(srcValue, outVal);

                    if (srcValue.NearEqual(outVal)) continue;
                    row[x].SetChannelValue(in Colors[i], in outVal);
                }
            }
        }

        private bool TryGetFactors(in int x, in int y, ref float fx, ref float fy)
        {
            var hasChanges = false;

            if (SizeX > float.Epsilon) {
                var right = Bounds.Right - 1;
                var sizeX = (int) MathF.Ceiling(SizeX * Bounds.Width);
                var skipX = x > Bounds.Left + sizeX && x < right - sizeX;

                if (!skipX) {
                    var fLeft = 1f - Math.Clamp((x - Bounds.Left) / (float) sizeX, 0f, 1f);
                    var fRight = 1f - Math.Clamp((right - x) / (float) sizeX, 0f, 1f);
                    fx = MathF.Max(fLeft, fRight);
                    hasChanges = true;
                }
            }

            if (SizeY > float.Epsilon) {
                var bottom = Bounds.Bottom - 1;
                var sizeY = (int) MathF.Ceiling(SizeY * Bounds.Height);
                var skipY = y > Bounds.Top + sizeY && y < bottom - sizeY;

                if (!skipY) {
                    var fTop = 1f - Math.Clamp((y - Bounds.Top) / (float) sizeY, 0f, 1f);
                    var fBottom = 1f - Math.Clamp((bottom - y) / (float) sizeY, 0f, 1f);
                    fy = MathF.Max(fTop, fBottom);
                    hasChanges = true;
                }
            }

            return hasChanges;
        }
    }
}
