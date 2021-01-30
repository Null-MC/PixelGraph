using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common
{
    public class MaterialContext : ResourcePackContext
    {
        public MaterialProperties Material {get; set;}
        public TextureTypes Type {get; set;}
    }
}
