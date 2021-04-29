using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class NormalMethodValues : List<NormalMethodValues.Item>
    {
        public NormalMethodValues()
        {
            Add(new Item {Text = "Sobel-3", Value = NormalMapMethods.Sobel3});
            Add(new Item {Text = "Sobel-High", Value = NormalMapMethods.SobelHigh});
            Add(new Item {Text = "Sobel-Low", Value = NormalMapMethods.SobelLow});
            Add(new Item {Text = "Variance", Value = NormalMapMethods.Variance});
        }

        public class Item
        {
            public string Text {get; set;}
            public NormalMapMethods Value {get; set;}
        }
    }
}
