using System;
using System.Linq;

namespace PixelGraph.Common.IO
{
    public static class ImageExtensions
    {
        public const string Bmp = "bmp";
        public const string Jpg = "tga";
        public const string Png = "png";
        public const string Tga = "tga";
        public const string WebP = "webp";

        public const string Default = Png;


        public static bool Supports(string extension)
        {
            var e = extension.TrimStart('.');
            return Supported.Contains(e, StringComparer.InvariantCultureIgnoreCase);
        }

        private static readonly string[] Supported = {
            "bmp",
            "png",
            "tga",
            "gif",
            "jpg",
            "jpeg",
            "webp",
        };
    }
}
