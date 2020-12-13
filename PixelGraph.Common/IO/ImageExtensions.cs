using System;
using System.Linq;

namespace PixelGraph.Common.IO
{
    public static class ImageExtensions
    {
        public const string Default = "png";

        public static readonly string[] Supported = {
            "bmp",
            "png",
            "tga",
            "gif",
            "jpg",
            "jpeg",
        };


        public static bool Supports(string extension)
        {
            var e = extension.TrimStart('.');
            return Supported.Contains(e, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
