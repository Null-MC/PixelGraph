using PixelGraph.UI.Internal.Preview;
using System.Collections.Generic;
using PixelGraph.UI.Internal.Preview.Scene;

namespace PixelGraph.UI.ViewData
{
    internal class RenderModeValues : List<RenderModeValues.Item>
    {
        public RenderModeValues()
        {
            Add(new Item {Text = "Diffuse", Value = RenderPreviewModes.Diffuse});
            Add(new Item {Text = "PBR Metal", Value = RenderPreviewModes.PbrMetal});
            Add(new Item {Text = "PBR Specular", Value = RenderPreviewModes.PbrSpecular});
        }

        public class Item
        {
            public string Text {get; set;}
            public RenderPreviewModes Value {get; set;}
        }
    }
}
