using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialBumpProperties
    {
        public ResourcePackBumpChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Scale {get; set;}
    }
}
