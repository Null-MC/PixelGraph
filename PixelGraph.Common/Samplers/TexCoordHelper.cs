using SixLabors.ImageSharp;

namespace PixelGraph.Common.Samplers;

internal static class TexCoordHelper
{
    public static void WrapCoordX(ref int x, in Rectangle bounds)
    {
        if (bounds.Width <= 0) {
            x = bounds.X;
            return;
        }

        while (x < bounds.Left) x += bounds.Width;
        while (x >= bounds.Right) x -= bounds.Width;
    }

    public static void WrapCoordY(ref int y, in Rectangle bounds)
    {
        if (bounds.Height <= 0) {
            y = bounds.Y;
            return;
        }

        while (y < bounds.Top) y += bounds.Height;
        while (y >= bounds.Bottom) y -= bounds.Height;
    }

    public static void ClampCoordX(ref int x, in Rectangle bounds)
    {
        if (x < bounds.Left) x = bounds.Left;
        if (x >= bounds.Right) x = bounds.Right-1;
    }

    public static void ClampCoordY(ref int y, in Rectangle bounds)
    {
        if (y < bounds.Top) y = bounds.Top;
        if (y >= bounds.Bottom) y = bounds.Bottom-1;
    }
}