using PixelGraph.Rendering;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class RenderModeValues : List<RenderModeValues.Item>
    {
        public RenderModeValues()
        {
            Add(new Item {Text = "Diffuse", Value = RenderPreviewModes.Diffuse});
            Add(new Item {Text = "Normals", Value = RenderPreviewModes.Normals});

#if DEBUG
            Add(new Item {Text = "PBR Filament (OldPbr)", Value = RenderPreviewModes.PbrFilament});
            Add(new Item {Text = "PBR Jessie (LabPbr)", Value = RenderPreviewModes.PbrJessie});
            Add(new Item {Text = "PBR Null (LabPbr)", Value = RenderPreviewModes.PbrNull});
#else
            Add(new Item {Text = "PBR Metal", Value = RenderPreviewModes.PbrFilament});
            Add(new Item {Text = "PBR Specular", Value = RenderPreviewModes.PbrNull});
#endif
        }

        public class Item
        {
            public string Text {get; set;}
            public RenderPreviewModes Value {get; set;}
        }
    }
}
