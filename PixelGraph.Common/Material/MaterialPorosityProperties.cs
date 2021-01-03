using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialPorosityProperties
    {
        public ResourcePackPorosityChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Scale {get; set;}
    }
}
