using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material
{
    public class MaterialHcmProperties
    {
        public ResourcePackHcmChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}


        public bool HasAnyData()
        {
            if (Input != null && Input.HasAnyData()) return true;
            if (Texture != null) return true;
            if (Value.HasValue) return true;
            return false;
        }
    }
}
