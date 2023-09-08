using PixelGraph.Common.Extensions;
using SharpDX;

namespace PixelGraph.Rendering.Extensions;

internal static class ColorExtensions
{
    public static void Lerp(in Color4 min, in Color4 max, in float mix, out Color4 result)
    {
        MathEx.Lerp(in min.Red, in max.Red, in mix, out result.Red);
        MathEx.Lerp(in min.Green, in max.Green, in mix, out result.Green);
        MathEx.Lerp(in min.Blue, in max.Blue, in mix, out result.Blue);
        MathEx.Lerp(in min.Alpha, in max.Alpha, in mix, out result.Alpha);
    }
}