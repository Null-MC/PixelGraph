#if !NORENDER
using PixelGraph.Rendering.Materials;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class BlendModeValues : List<BlendModeValues.Item>
    {
        public BlendModeValues()
        {
            Add(new Item {Text = "Opaque", Value = BlendModes.OpaqueText});
            Add(new Item {Text = "Cutout", Value = BlendModes.CutoutText});
            Add(new Item {Text = "Transparent", Value = BlendModes.TransparentText});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
#endif
