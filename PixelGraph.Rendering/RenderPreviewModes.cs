using System;
using System.Collections.Generic;

namespace PixelGraph.Rendering;

public enum RenderPreviewModes
{
    Diffuse,
    Normals,
    PbrFilament,
    PbrJessie,
    PbrNull,
}

public static class RenderPreviewMode
{
    public static readonly string Diffuse = "diffuse";
    public static readonly string Normals = "normals";
    public static readonly string PbrFilament = "pbr-filament";
    public static readonly string PbrJessie = "pbr-jessie";
    public static readonly string PbrNull = "pbr-null";


    private static readonly Dictionary<string, RenderPreviewModes> parseMap = new(StringComparer.InvariantCultureIgnoreCase) {
        [Diffuse] = RenderPreviewModes.Diffuse,
        [Normals] = RenderPreviewModes.Normals,
        [PbrFilament] = RenderPreviewModes.PbrFilament,
        [PbrJessie] = RenderPreviewModes.PbrJessie,
        [PbrNull] = RenderPreviewModes.PbrNull,
    };


    public static string GetString(RenderPreviewModes mode)
    {
        return mode switch {
            RenderPreviewModes.Diffuse => Diffuse,
            RenderPreviewModes.Normals => Normals,
            RenderPreviewModes.PbrFilament => PbrFilament,
            RenderPreviewModes.PbrJessie => PbrJessie,
            RenderPreviewModes.PbrNull => PbrNull,
            _ => null,
        };
    }

    public static bool TryParse(string value, out RenderPreviewModes mode)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        return parseMap.TryGetValue(value, out mode);
    }
}