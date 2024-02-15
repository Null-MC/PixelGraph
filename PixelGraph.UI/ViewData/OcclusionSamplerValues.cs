using PixelGraph.Common.Samplers;

namespace PixelGraph.UI.ViewData;

internal class OcclusionSamplerValues : List<OcclusionSamplerValues.Item>
{
    public OcclusionSamplerValues()
    {
        Add(new Item {Text = "Nearest", Value = Samplers.Nearest});
        Add(new Item {Text = "Bilinear", Value = Samplers.Bilinear});
    }

    public class Item
    {
        public string? Text {get; set;}
        public string? Value {get; set;}
    }
}