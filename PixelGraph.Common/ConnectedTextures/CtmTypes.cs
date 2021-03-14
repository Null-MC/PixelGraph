using System;

namespace PixelGraph.Common.ConnectedTextures
{
    public class CtmTypes
    {
        public const string Compact = "compact";
        public const string Full = "full";
        public const string Expanded = "expanded";


        public static bool Is(string expected, string actual)
        {
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }
    }
}
