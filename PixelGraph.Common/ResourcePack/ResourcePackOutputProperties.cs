using PixelGraph.Common.IO;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackOutputProperties : ResourcePackEncoding
    {
        public const string ImageDefault = ImageExtensions.Png;

        public string Image {get; set;}

        public string Format {get; set;}

        public string Sampler {get; set;}
    }
}
