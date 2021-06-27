using PixelGraph.Common.Material;

namespace PixelGraph.UI.Models.Tabs
{
    public class MaterialTabModel : TabModelBase
    {
        public string MaterialFilename {get; set;}
        public MaterialProperties Material {get; set;}
    }
}
