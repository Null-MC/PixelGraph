using PixelGraph.Common.Samplers;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class SamplerValues : List<SamplerValues.Item>
    {
        public SamplerValues()
        {
            Add(new Item {Text = "Nearest", Value = Samplers.Nearest});
            Add(new Item {Text = "Bilinear", Value = Samplers.Bilinear});
            Add(new Item {Text = "Bicubic", Value = Samplers.Bicubic});
            Add(new Item {Text = "Average", Value = Samplers.Average});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
