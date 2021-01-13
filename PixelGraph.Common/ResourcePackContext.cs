using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common
{
    public class ResourcePackContext
    {
        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}


        public bool AutoMaterial => Profile.AutoMaterial ?? ResourcePackProfileProperties.AutoMaterialDefault;
    }
}
