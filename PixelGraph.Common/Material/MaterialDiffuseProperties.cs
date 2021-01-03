using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialDiffuseProperties
    {
        public string Texture {get; set;}

        public ResourcePackDiffuseRedChannelProperties InputRed {get; set;}
        public decimal? ValueRed {get; set;}
        public decimal? ScaleRed {get; set;}

        public ResourcePackDiffuseGreenChannelProperties InputGreen {get; set;}
        public decimal? ValueGreen {get; set;}
        public decimal? ScaleGreen {get; set;}

        public ResourcePackDiffuseBlueChannelProperties InputBlue {get; set;}
        public decimal? ValueBlue {get; set;}
        public decimal? ScaleBlue {get; set;}
    }
}
