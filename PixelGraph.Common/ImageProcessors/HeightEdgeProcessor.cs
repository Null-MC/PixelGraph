using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors;

internal class HeightEdgeProcessor
{
    public Rectangle Bounds;
    public ColorChannel[]? Colors;
    public float SizeTop, SizeBottom;
    public float SizeLeft, SizeRight;
    public float Strength = 1f;
    public bool IsGrayscale;


    public void Apply(Image image)
    {
        image.Mutate(c => c.ProcessPixelRowsAsVector4(ProcessRow, Bounds));
    }

    private void ProcessRow(Span<Vector4> row, Point pos)
    {
        ArgumentNullException.ThrowIfNull(Colors);

        var count = Colors.Length;

        for (var x = 0; x < Bounds.Width; x++) {
            float fx = 0f, fy = 0f;
            if (!TryGetFactors(in x, pos.Y - Bounds.Y, ref fx, ref fy)) return;

            var fadeValue = Math.Max(fx, fy);

            for (var i = 0; i < count; i++) {
                row[x].GetChannelValue(in Colors[i], out var srcValue);
                    
                MathEx.Lerp(in srcValue, in fadeValue, in Strength, out var outVal);
                outVal = MathF.Max(srcValue, outVal);

                if (srcValue.NearEqual(outVal)) continue;

                if (IsGrayscale)
                    row[x].X = row[x].Y = row[x].Z = outVal;
                else
                    row[x].SetChannelValue(in Colors[i], in outVal);
            }
        }
    }

    private bool TryGetFactors(in int x, in int y, ref float fx, ref float fy)
    {
        var hasChanges = false;

        //if (SizeX > float.Epsilon) {
        //    var right = Bounds.Width - 1;
        //    var sizeX = (int) MathF.Ceiling(SizeX * Bounds.Width);
        //    var skipX = x > sizeX && x < right - sizeX;

        //    if (!skipX) {
        //        var fLeft = 1f - Math.Clamp(x / (float)sizeX, 0f, 1f);
        //        var fRight = 1f - Math.Clamp((right - x) / (float)sizeX, 0f, 1f);
        //        fx = MathF.Max(fLeft, fRight);
        //        hasChanges = true;
        //    }
        //}

        if (SizeLeft > float.Epsilon) {
            //var right = Bounds.Width - 1;
            var sizeLeft = (int) MathF.Ceiling(SizeLeft * Bounds.Width);

            if (x <= sizeLeft) {
                var fLeft = 1f - Math.Clamp(x / (float)sizeLeft, 0f, 1f);
                fx = MathF.Max(fLeft, fx);
                hasChanges = true;
            }
        }

        // ERROR: There seems to be a bug with only the right edge! test: right=0.01
        if (SizeRight > float.Epsilon) {
            var right = Bounds.Width - 1;
            var sizeRight = (int)MathF.Ceiling(SizeRight * Bounds.Width);

            //if (x >= right - sizeRight) {
            var fRight = 1f - Math.Clamp((right - x) / (float)sizeRight, 0f, 1f);
            fx = MathF.Max(fx, fRight);
            hasChanges = true;
            //}
        }

        //if (SizeY > float.Epsilon) {
        //    var bottom = Bounds.Height - 1;
        //    var sizeY = (int) MathF.Ceiling(SizeY * Bounds.Height);
        //    var skipY = y > 0 + sizeY && y < bottom - sizeY;

        //    if (!skipY) {
        //        var fTop = 1f - Math.Clamp(y / (float) sizeY, 0f, 1f);
        //        var fBottom = 1f - Math.Clamp((bottom - y) / (float) sizeY, 0f, 1f);
        //        fy = MathF.Max(fTop, fBottom);
        //        hasChanges = true;
        //    }
        //}

        if (SizeTop > float.Epsilon) {
            var sizeTop = (int) MathF.Ceiling(SizeTop * Bounds.Height);
            var skipTop = y > sizeTop;

            if (!skipTop) {
                var fTop = 1f - Math.Clamp(y / (float) sizeTop, 0f, 1f);
                fy = MathF.Max(fTop, fy);
                hasChanges = true;
            }
        }

        if (SizeBottom > float.Epsilon) {
            var bottom = Bounds.Height - 1;
            var sizeBottom = (int) MathF.Ceiling(SizeBottom * Bounds.Height);
            var skipBottom = y < bottom - sizeBottom;

            if (!skipBottom) {
                var fBottom = 1f - Math.Clamp((bottom - y) / (float) sizeBottom, 0f, 1f);
                fy = MathF.Max(fy, fBottom);
                hasChanges = true;
            }
        }

        return hasChanges;
    }
}