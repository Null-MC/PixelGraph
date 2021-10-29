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
            Add(new Item {Text = "Fixed", Value = CtmTypes.Fixed});
            Add(new Item {Text = "Random", Value = CtmTypes.Random});
            Add(new Item {Text = "Repeat", Value = CtmTypes.Repeat});
            Add(new Item {Text = "Compact", Value = CtmTypes.Compact});
            Add(new Item {Text = "Full", Value = CtmTypes.Full});
            Add(new Item {Text = "Top", Value = CtmTypes.Top});
            Add(new Item {Text = "Horizontal", Value = CtmTypes.Horizontal});
            Add(new Item {Text = "Vertical", Value = CtmTypes.Vertical});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
