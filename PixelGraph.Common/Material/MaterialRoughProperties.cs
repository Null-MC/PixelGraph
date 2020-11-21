using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Material
{
    public class MaterialRoughProperties
    {
        public TextureEncoding Input {get; set;}
        public string Texture {get; set;}
        public byte? Value {get; set;}
        public decimal? Scale {get; set;}
    }
}
