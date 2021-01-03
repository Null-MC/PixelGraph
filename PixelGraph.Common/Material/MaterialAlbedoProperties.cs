using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialAlbedoProperties
    {
        public string Texture {get; set;}

        public ResourcePackAlbedoRedChannelProperties InputRed {get; set;}
        public decimal? ValueRed {get; set;}
        public decimal? ScaleRed {get; set;}

        public ResourcePackAlbedoGreenChannelProperties InputGreen {get; set;}
        public decimal? ValueGreen {get; set;}
        public decimal? ScaleGreen {get; set;}

        public ResourcePackAlbedoBlueChannelProperties InputBlue {get; set;}
        public decimal? ValueBlue {get; set;}
        public decimal? ScaleBlue {get; set;}
    }
}
