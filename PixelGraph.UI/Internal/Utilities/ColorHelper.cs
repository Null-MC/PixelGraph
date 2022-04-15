using System.Windows.Media;

namespace PixelGraph.UI.Internal.Utilities
{
    internal static class ColorHelper
    {
        public static Color? RGBFromHex(string color)
        {
            if (color == null) return null;

            if (color.Length == 6 && !color.StartsWith('#')) color = '#'+color;
            return MahApps.Metro.Controls.ColorHelper.ColorFromString(color);
        }

        public static string ToHexRGB(Color color)
        {
            return ToHexRGB(color.R, color.G, color.B);
        }

        public static string ToHexRGB(in byte red, in byte green, in byte blue)
        {
            return $"#{red:X2}{green:X2}{blue:X2}";
        }
    }
}
