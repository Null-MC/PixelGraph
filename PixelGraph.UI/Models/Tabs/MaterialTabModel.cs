using PixelGraph.Common.Material;
using PixelGraph.UI.Internal.Caching;

namespace PixelGraph.UI.Models.Tabs;

public class MaterialTabModel : TabModelBase
{
    public string MaterialFilename {get; set;}
    //public MaterialProperties Material {get; set;}
    public CacheRegistration<string, MaterialProperties> MaterialRegistration {get; set;}
}