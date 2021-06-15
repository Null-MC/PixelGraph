using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview
{
    public enum RenderPreviewModes
    {
        Diffuse,
        PbrFilament,
        PbrJessie,
        PbrNull,
    }

    internal static class RenderPreviewMode
    {
        private static readonly Dictionary<string, RenderPreviewModes> parseMap = new(StringComparer.InvariantCultureIgnoreCase) {
            ["diffuse"] = RenderPreviewModes.Diffuse,
            ["pbr-filament"] = RenderPreviewModes.PbrFilament,
            ["pbr-jessie"] = RenderPreviewModes.PbrJessie,
            ["pbr-null"] = RenderPreviewModes.PbrNull,
        };


        public static string GetString(RenderPreviewModes mode)
        {
            return mode switch {
                RenderPreviewModes.Diffuse => "diffuse",
                RenderPreviewModes.PbrFilament => "pbr-filament",
                RenderPreviewModes.PbrJessie => "pbr-jessie",
                RenderPreviewModes.PbrNull => "pbr-null",
                _ => null,
            };
        }

        public static bool TryParse(string value, out RenderPreviewModes mode)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return parseMap.TryGetValue(value, out mode);
        }
    }
}
