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
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
