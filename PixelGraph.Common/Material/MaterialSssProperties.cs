using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialSssProperties
    {
        public ResourcePackSssChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public byte? Value {get; set;}
        public decimal? Scale {get; set;}
    }
}
