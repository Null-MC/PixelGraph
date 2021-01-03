using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialHeightProperties
    {
        public ResourcePackHeightChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Shift {get; set;}
        public decimal? Scale {get; set;}
        public int? EdgeFadeSizeX {get; set;}
        public int? EdgeFadeSizeY {get; set;}
    }
}
