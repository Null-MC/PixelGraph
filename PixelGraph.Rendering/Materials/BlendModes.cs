using System;
using System.Collections.Generic;

namespace PixelGraph.Rendering.Materials
{
    public static class BlendModes
    {
        public const int Opaque = 0;
        public const int Cutout = 1;
        public const int Transparent = 2;

        public const string OpaqueText = "opaque";
        public const string CutoutText = "cutout";
        public const string TransparentText = "transparent";


        public static bool TryParse(string text, out int value)
        {
            if (text == null) {
                value = 0;
                return false;
            }

            return parseMap.TryGetValue(text, out value);
        }

        public static string ToString(int value)
        {
            return value switch {
                Opaque => OpaqueText,
                Cutout => CutoutText,
                Transparent => TransparentText,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        private static readonly Dictionary<string, int> parseMap = new(StringComparer.InvariantCultureIgnoreCase) {
            [OpaqueText] = Opaque,
            [CutoutText] = Cutout,
            [TransparentText] = Transparent,
        };
    }
}
