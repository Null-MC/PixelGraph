using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialAlphaProperties
    {
        public string Texture {get; set;}

        public ResourcePackAlphaChannelProperties Input {get; set;}
        public decimal? Value {get; set;}
        public decimal? Scale {get; set;}
    }
}
