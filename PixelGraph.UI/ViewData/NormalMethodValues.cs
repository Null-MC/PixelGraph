using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class NormalMethodValues : List<NormalMethodValues.Item>
    {
        public NormalMethodValues()
        {
            Add(new Item {Text = "Sobel 3x3", Value = NormalMapMethod.Sobel3});
            Add(new Item {Text = "Sobel-High", Value = NormalMapMethod.SobelHigh});
            Add(new Item {Text = "Sobel-Low", Value = NormalMapMethod.SobelLow});
            Add(new Item {Text = "Variance", Value = NormalMapMethod.Variance});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
