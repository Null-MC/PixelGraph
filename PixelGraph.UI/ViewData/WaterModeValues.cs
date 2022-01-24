using PixelGraph.UI.Internal.Preview;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{

    internal class WaterModeValues : List<WaterModeValues.Item>
    {
        public WaterModeValues()
        {
            Add(new Item {Text = "Surface", Value = WaterMode.Surface});
            Add(new Item {Text = "Puddle", Value = WaterMode.Puddle});
            Add(new Item {Text = "Full", Value = WaterMode.Full});
        }

        public class Item
        {
            public string Text {get; set;}
            public int Value {get; set;}
        }
    }
}
