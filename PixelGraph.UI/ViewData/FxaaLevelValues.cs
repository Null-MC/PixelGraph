using HelixToolkit.SharpDX.Core;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{

    internal class FxaaLevelValues : List<FxaaLevelValues.Item>
    {
        public FxaaLevelValues()
        {
            Add(new Item {Text = "Ultra", Value = FXAALevel.Ultra});
            Add(new Item {Text = "High", Value = FXAALevel.High});
            Add(new Item {Text = "Medium", Value = FXAALevel.Medium});
            Add(new Item {Text = "Low", Value = FXAALevel.Low});
            Add(new Item {Text = "None", Value = FXAALevel.None});
        }

        public class Item
        {
            public string Text {get; set;}
            public FXAALevel Value {get; set;}
        }
    }
}
