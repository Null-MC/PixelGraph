using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.ConnectedTextures
{
    public static class CtmTypes
    {
        public const string Optifine_Full = "of-full";
        public const string Optifine_Compact = "of-compact";
        public const string Optifine_Horizontal = "of-horizontal";
        public const string Optifine_Vertical = "of-vertical";
        public const string Optifine_HorizontalVertical = "of-horizontal-vertical";
        public const string Optifine_VerticalHorizontal = "of-vertical-horizontal";
        public const string Optifine_Top = "of-top";
        public const string Optifine_Random = "of-random";
        public const string Optifine_Repeat = "of-repeat";
        public const string Optifine_Fixed = "of-fixed";

        public const string Optifine_Overlay = "of-overlay";
        public const string Optifine_OverlayFull = "of-overlay-full";
        public const string Optifine_OverlayRandom = "of-overlay-random";
        public const string Optifine_OverlayRepeat = "of-overlay-repeat";
        public const string Optifine_OverlayFixed = "of-overlay-fixed";

        public const string Optifine_Expanded = "of-expanded";


        private static readonly Dictionary<string, CtmDescription> tileSizeMap = new(StringComparer.InvariantCultureIgnoreCase) {
            [Optifine_Full] = new(12, 4, 47),
            [Optifine_Compact] = new(5, 1, 5),
            [Optifine_Horizontal] = new(4, 1, 4),
            [Optifine_Vertical] = new(4, 1, 4),
            //[Optifine_HorizontalVertical] = null,
            //[Optifine_VerticalHorizontal] = null,
            [Optifine_Top] = new(1, 1, 1),
            //[Optifine_Random] = null,
            //[Optifine_Repeat] = null,
            [Optifine_Fixed] = new(1, 1, 1),

            [Optifine_Overlay] = new(7, 3, 17),
            [Optifine_OverlayFull] = new(12, 4, 47),
            //[Optifine_OverlayRandom] = new(),
            //[Optifine_OverlayRepeat] = new(),
            [Optifine_OverlayFixed] = new(1, 1, 1),

            [Optifine_Expanded] = new(12, 4, 47),
        };

        private static string[] _connectedTypes = {
            Optifine_Random, Optifine_Repeat,
            Optifine_OverlayRandom, Optifine_OverlayRepeat,
        };


        public static bool Is(string expected, string actual)
        {
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsConnectedType(string method)
        {
            return _connectedTypes.Contains(method, StringComparer.InvariantCultureIgnoreCase);
        }

        private static readonly string[] fixedTypes = {Optifine_Fixed, Optifine_Top, Optifine_Compact, Optifine_Horizontal, Optifine_Vertical, Optifine_Compact, Optifine_Expanded, Optifine_Full};

        public static bool IsFixedSize(string method)
        {
            return fixedTypes.Contains(method, StringComparer.InvariantCultureIgnoreCase);
        }

        public static CtmDescription GetBounds(MaterialConnectionProperties connections)
        {
            return GetBounds(connections.Method, connections.Width, connections.Height);
        }

        public static CtmDescription GetBounds(string method, int? width, int? height)
        {
            if (method == null) return null;

            var hasDefaultSize = tileSizeMap.TryGetValue(method, out var defaultSize);

            var w = width ?? defaultSize?.Width ?? 1;
            var h = height ?? defaultSize?.Height ?? 1;
            var c = defaultSize?.Total ?? w * h;

            //if (Is(Repeat, method) || Is(Random, method))
            if (width.HasValue && height.HasValue)
                return new CtmDescription(w, h, c);

            return hasDefaultSize ? defaultSize : null;
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
