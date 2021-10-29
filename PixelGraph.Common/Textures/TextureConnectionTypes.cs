using System;

namespace PixelGraph.Common.Textures
{
    public static class TextureConnectionTypes
    {
        public const string MultiPart = "multipart";
        public const string Random = "random";
        public const string Repeat = "repeat";
        public const string Compact = "compact";
        public const string Full = "full";


        public static bool Is(string expectedType, string actualType) =>
            string.Equals(expectedType, actualType, StringComparison.InvariantCultureIgnoreCase);
    }
}
