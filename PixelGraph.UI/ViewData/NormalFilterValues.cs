using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class NormalFilterValues : List<NormalFilterValues.Item>
    {
        public NormalFilterValues()
        {
            Add(new Item {Text = "Sobel-3", Value = NormalMapFilters.Sobel3});
            Add(new Item {Text = "Sobel-High", Value = NormalMapFilters.SobelHigh});
            Add(new Item {Text = "Sobel-Low", Value = NormalMapFilters.SobelLow});
            Add(new Item {Text = "Variance", Value = NormalMapFilters.Variance});
        }

        public class Item
        {
            public string Text {get; set;}
            public NormalMapFilters Value {get; set;}
        }
    }
}
