using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialOpacityProperties : IHaveData
    {
        public ResourcePackOpacityChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Shift {get; set;}
        public decimal? Scale {get; set;}


        public bool HasAnyData()
        {
            if (Input != null && Input.HasAnyData()) return true;
            if (Texture != null) return true;
            if (Value.HasValue) return true;
            if (Shift.HasValue) return true;
            if (Scale.HasValue) return true;
            return false;
        }
    }
}
