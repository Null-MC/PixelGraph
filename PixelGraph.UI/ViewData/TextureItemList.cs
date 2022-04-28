using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    public class TextureItemList : List<TextureItemList.Item>
    {
        public TextureItemList()
        {
            Add(new Item("General", TextureTags.General));
            Add(new Item("Color", TextureTags.Color));
            Add(new Item("Opacity", TextureTags.Opacity));
            Add(new Item("Height", TextureTags.Height));
            Add(new Item("Normal", TextureTags.Normal));
            Add(new Item("Occlusion", TextureTags.Occlusion));
            Add(new Item("Specular", TextureTags.Specular));
            Add(new Item("Smoothness", TextureTags.Smooth));
            Add(new Item("Roughness", TextureTags.Rough));
            Add(new Item("Metal", TextureTags.Metal));
            Add(new Item("HCM", TextureTags.HCM));
            Add(new Item("F0", TextureTags.F0));
            Add(new Item("Porosity", TextureTags.Porosity));
            Add(new Item("SSS", TextureTags.SubSurfaceScattering));
            Add(new Item("Emissive", TextureTags.Emissive));
        }

        public class Item
        {
            public string Name {get;}
            public string Key {get;}

            public Item(string name, string key)
            {
                Name = name;
                Key = key;
            }
        }
    }
}
