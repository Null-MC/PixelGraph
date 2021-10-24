using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class BlendModeValues : List<BlendModeValues.Item>
    {
        public BlendModeValues()
        {
            Add(new Item {Text = "Opaque", Value = BlendMode.Opaque});
            Add(new Item {Text = "Cutout", Value = BlendMode.Cutout});
            Add(new Item {Text = "Transparent", Value = BlendMode.Transparent});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }

    internal static class BlendMode
    {
        public const string Opaque = "opaque";
        public const string Cutout = "cutout";
        public const string Transparent = "transparent";
    }
}
