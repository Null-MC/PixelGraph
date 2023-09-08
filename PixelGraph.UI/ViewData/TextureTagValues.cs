using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData;

internal class TextureTagValues : List<TextureTagValues.Item>
{
    public TextureTagValues()
    {
        Add(new Item {Text = "Opacity", Value = TextureTags.Opacity});
        Add(new Item {Text = "Color", Value = TextureTags.Color});
        Add(new Item {Text = "Height", Value = TextureTags.Height});
        Add(new Item {Text = "Normal", Value = TextureTags.Normal});
        Add(new Item {Text = "Occlusion", Value = TextureTags.Occlusion});
        Add(new Item {Text = "Specular", Value = TextureTags.Specular});
        Add(new Item {Text = "Rough", Value = TextureTags.Rough});
        Add(new Item {Text = "Smooth", Value = TextureTags.Smooth});
        Add(new Item {Text = "Metal", Value = TextureTags.Metal});
        Add(new Item {Text = "HCM", Value = TextureTags.HCM});
        Add(new Item {Text = "F0", Value = TextureTags.F0});
        Add(new Item {Text = "Porosity", Value = TextureTags.Porosity});
        Add(new Item {Text = "SSS", Value = TextureTags.SubSurfaceScattering});
        Add(new Item {Text = "Emissive", Value = TextureTags.Emissive});
        Add(new Item {Text = "MER", Value = TextureTags.MER});
    }

    public class Item
    {
        public string Text {get; set;}
        public string Value {get; set;}
    }
}