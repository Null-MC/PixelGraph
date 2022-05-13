using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{

    internal class FxaaLevelValues : List<FxaaLevelValues.Item>
    {
        public FxaaLevelValues()
        {
            Add(new Item {Text = "Ultra", Value = 4});
            Add(new Item {Text = "High", Value = 3});
            Add(new Item {Text = "Medium", Value = 2});
            Add(new Item {Text = "Low", Value = 1});
            Add(new Item {Text = "None", Value = 0});
        }

        public class Item
        {
            public string Text {get; set;}
            public int Value {get; set;}
        }
    }
}
