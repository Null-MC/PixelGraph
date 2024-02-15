using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewData;

internal class NormalMethodValues : List<NormalMethodValues.Item>
{
    public NormalMethodValues()
    {
        Add(new Item {Text = "Sobel 3x3", Value = NormalMapMethod.Sobel3});
        Add(new Item {Text = "Sobel 5x5", Value = NormalMapMethod.Sobel5});
        Add(new Item {Text = "Sobel 9x9", Value = NormalMapMethod.Sobel9});
        Add(new Item {Text = "Sobel-High", Value = NormalMapMethod.SobelHigh});
        Add(new Item {Text = "Sobel-Low", Value = NormalMapMethod.SobelLow});
        Add(new Item {Text = "Variance", Value = NormalMapMethod.Variance});
    }

    public class Item
    {
        public string? Text {get; set;}
        public string? Value {get; set;}
    }
}