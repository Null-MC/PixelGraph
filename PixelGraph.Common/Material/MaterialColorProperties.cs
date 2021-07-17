using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialColorProperties
    {
        public string Texture {get; set;}

        public ResourcePackColorRedChannelProperties InputRed {get; set;}
        public decimal? ValueRed {get; set;}
        public decimal? ScaleRed {get; set;}

        public ResourcePackColorGreenChannelProperties InputGreen {get; set;}
        public decimal? ValueGreen {get; set;}
        public decimal? ScaleGreen {get; set;}

        public ResourcePackColorBlueChannelProperties InputBlue {get; set;}
        public decimal? ValueBlue {get; set;}
        public decimal? ScaleBlue {get; set;}


        public bool HasAnyData()
        {
            if (Texture != null) return true;

            if (InputRed != null && InputRed.HasAnyData()) return true;
            if (InputGreen != null && InputGreen.HasAnyData()) return true;
            if (InputBlue != null && InputBlue.HasAnyData()) return true;

            if (ValueRed.HasValue) return true;
            if (ValueGreen.HasValue) return true;
            if (ValueBlue.HasValue) return true;

            if (ScaleRed.HasValue) return true;
            if (ScaleGreen.HasValue) return true;
            if (ScaleBlue.HasValue) return true;

            return false;
        }
    }
}
