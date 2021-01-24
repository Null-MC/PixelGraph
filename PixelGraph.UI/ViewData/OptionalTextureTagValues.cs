using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class OptionalTextureTagValues : List<OptionalTextureTagValues.Item>
    {
        public OptionalTextureTagValues()
        {
            Add(new Item {Text = "None", Value = TextureTags.None});
            //Add(new Item {Text = "White", Value = TextureTags.White});
            Add(new Item {Text = "Alpha", Value = TextureTags.Alpha});
            Add(new Item {Text = "Diffuse", Value = TextureTags.Diffuse});
            Add(new Item {Text = "Albedo", Value = TextureTags.Albedo});
            Add(new Item {Text = "Height", Value = TextureTags.Height});
            Add(new Item {Text = "Normal", Value = TextureTags.Normal});
            Add(new Item {Text = "Occlusion", Value = TextureTags.Occlusion});
            Add(new Item {Text = "Specular", Value = TextureTags.Specular});
            Add(new Item {Text = "Rough", Value = TextureTags.Rough});
            Add(new Item {Text = "Smooth", Value = TextureTags.Smooth});
            Add(new Item {Text = "Metal", Value = TextureTags.Metal});
            Add(new Item {Text = "F0", Value = TextureTags.F0});
            Add(new Item {Text = "Porosity", Value = TextureTags.Porosity});
            Add(new Item {Text = "SubSurface Scattering", Value = TextureTags.SubSurfaceScattering});
            Add(new Item {Text = "Emissive", Value = TextureTags.Emissive});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
