namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackContext
    {
        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}

        public bool AutoMaterial => Input.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;
    }
}
