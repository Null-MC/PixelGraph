using PixelGraph.Common.Encoding;

namespace PixelGraph.Common.Material
{
    public class MaterialDiffuseProperties
    {
        public TextureEncoding Input {get; set;}
        public string Texture {get; set;}
        public byte? ValueRed {get; set;}
        public decimal? ScaleRed {get; set;}
        public byte? ValueGreen {get; set;}
        public decimal? ScaleGreen {get; set;}
        public byte? ValueBlue {get; set;}
        public decimal? ScaleBlue {get; set;}
        public byte? ValueAlpha {get; set;}
        public decimal? ScaleAlpha {get; set;}
    }
}
