namespace PixelGraph.UI.Internal.Utilities;

internal static class ZoomHelper
{
    public static double Parse(string zoom)
    {
        if (double.TryParse(zoom.Trim('%', ' '), out var value))
            return value * 0.01f;

        return 1f;
    }

    public static string Format(double value)
    {
        return $"{value * 100:N2}%";
    }
}