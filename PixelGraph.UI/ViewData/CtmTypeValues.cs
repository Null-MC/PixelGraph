using PixelGraph.Common.ConnectedTextures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class CtmTypeValues : List<CtmTypeValues.Item>
    {
        public CtmTypeValues()
        {
            Add(new Item {Text = "None", Value = null});
            //Add(new Item {Text = "Multi-Part", Value = CtmTypes.MultiPart});
            Add(new Item {Text = "Optifine Full", Value = CtmTypes.Optifine_Full});
            Add(new Item {Text = "Optifine Compact", Value = CtmTypes.Optifine_Compact});
            Add(new Item {Text = "Optifine Horizontal", Value = CtmTypes.Optifine_Horizontal});
            Add(new Item {Text = "Optifine Vertical", Value = CtmTypes.Optifine_Vertical});
            Add(new Item {Text = "Optifine Horizontal+Vertical", Value = CtmTypes.Optifine_HorizontalVertical});
            Add(new Item {Text = "Optifine Vertical+Horizontal", Value = CtmTypes.Optifine_VerticalHorizontal});
            Add(new Item {Text = "Optifine Top", Value = CtmTypes.Optifine_Top});
            Add(new Item {Text = "Optifine Random", Value = CtmTypes.Optifine_Random});
            Add(new Item {Text = "Optifine Repeat", Value = CtmTypes.Optifine_Repeat});
            Add(new Item {Text = "Optifine Fixed", Value = CtmTypes.Optifine_Fixed});

            Add(new Item {Text = "Optifine Overlay", Value = CtmTypes.Optifine_Overlay});
            Add(new Item {Text = "Optifine Overlay Full", Value = CtmTypes.Optifine_OverlayFull});
            Add(new Item {Text = "Optifine Overlay Random", Value = CtmTypes.Optifine_OverlayRandom});
            Add(new Item {Text = "Optifine Overlay Repeat", Value = CtmTypes.Optifine_OverlayRepeat});
            Add(new Item {Text = "Optifine Overlay Fixed", Value = CtmTypes.Optifine_OverlayFixed});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
