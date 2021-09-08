using PixelGraph.Common.Textures;
using PixelGraph.UI.Controls;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    public class TextureItemList : List<MaterialPropertiesControl.TextureItem>
    {
        public TextureItemList()
        {
            Add(new MaterialPropertiesControl.TextureItem {Name = "General", Key = TextureTags.General});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Color", Key = TextureTags.Color});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Opacity", Key = TextureTags.Opacity});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Height", Key = TextureTags.Height});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Normal", Key = TextureTags.Normal});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Occlusion", Key = TextureTags.Occlusion});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Specular", Key = TextureTags.Specular});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Smoothness", Key = TextureTags.Smooth});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Roughness", Key = TextureTags.Rough});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Metal", Key = TextureTags.Metal});
            Add(new MaterialPropertiesControl.TextureItem {Name = "F0", Key = TextureTags.F0});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Porosity", Key = TextureTags.Porosity});
            Add(new MaterialPropertiesControl.TextureItem {Name = "SSS", Key = TextureTags.SubSurfaceScattering});
            Add(new MaterialPropertiesControl.TextureItem {Name = "Emissive", Key = TextureTags.Emissive});
        }
    }
}
