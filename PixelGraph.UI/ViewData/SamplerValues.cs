using PixelGraph.Common.Samplers;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class SamplerValues : List<SamplerValues.Item>
    {
        public SamplerValues()
        {
            Add(new Item {Text = "Nearest", Value = Sampler.Nearest});
            Add(new Item {Text = "Bilinear", Value = Sampler.Bilinear});
            Add(new Item {Text = "Average", Value = Sampler.Average});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
