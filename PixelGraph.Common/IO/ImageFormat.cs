using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO;

public enum ImageFormats
{
    Unknown,
    Bitmap,
    Png,
    Tga,
    Jpeg,
    Gif,
    WebP,
}

public static class ImageFormat
{
    private static readonly Dictionary<string, ImageFormats> imageFormatMap;


    static ImageFormat()
    {
        imageFormatMap = new Dictionary<string, ImageFormats>(StringComparer.InvariantCultureIgnoreCase) {
            ["bmp"] = ImageFormats.Bitmap,
            ["png"] = ImageFormats.Png,
            ["tga"] = ImageFormats.Tga,
            ["jpg"] = ImageFormats.Jpeg,
            ["jpeg"] = ImageFormats.Jpeg,
            ["gif"] = ImageFormats.Gif,
            ["webp"] = ImageFormats.WebP,
        };
    }

    public static bool TryParse(string ext, out ImageFormats format)
    {
        if (ext == null) throw new ArgumentNullException(nameof(ext));

        return imageFormatMap.TryGetValue(ext.TrimStart('.'), out format);
    }
}