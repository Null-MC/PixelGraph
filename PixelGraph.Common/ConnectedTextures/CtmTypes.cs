using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.ConnectedTextures
{
    public static class CtmTypes
    {
        public const string Fixed = "fixed";
        public const string Top = "top";
        public const string Full = "full";
        public const string Compact = "compact";
        public const string Expanded = "expanded";
        public const string Random = "random";
        public const string Repeat = "repeat";
        public const string Horizontal = "horizontal";
        public const string Vertical = "vertical";

        private static readonly Dictionary<string, CtmDescription> tileSizeMap = new(StringComparer.InvariantCultureIgnoreCase) {
            [Fixed] = new(1, 1, 1),
            [Top] = new(1, 1, 1),
            [Horizontal] = new(4, 1, 4),
            [Vertical] = new(4, 1, 4),
            [Compact] = new(5, 1, 5),
            [Expanded] = new(12, 4, 47),
            [Full] = new(12, 4, 47),
        };


        public static bool Is(string expected, string actual)
        {
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly string[] fixedTypes = {Fixed, Top, Compact, Horizontal, Vertical, Compact, Expanded, Full};

        public static bool IsFixedSize(string method)
        {
            return fixedTypes.Contains(method, StringComparer.InvariantCultureIgnoreCase);
        }

        public static CtmDescription GetBounds(MaterialConnectionProperties connections)
        {
            return GetBounds(connections.Method, connections.Width ?? 1, connections.Height ?? 1);
        }

        public static CtmDescription GetBounds(string method, int width, int height)
        {
            if (method == null) return null;

            if (Is(Repeat, method) || Is(Random, method))
                return new CtmDescription(width, height, width * height);

            return tileSizeMap.TryGetValue(method, out var size) ? size : null;
        }
    }

    public class CtmDescription
    {
        public int Width {get;}
        public int Height {get;}
        public int Total {get;}


        public CtmDescription(int width, int height, int total)
        {
            Width = width;
            Height = height;
            Total = total;
        }
    }
}
