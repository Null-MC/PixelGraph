using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewData;

internal class ColorChannelValues : List<ColorChannelValues.Item>
{
    public ColorChannelValues()
    {
        Add(new Item {Text = "Red", Value = ColorChannel.Red});
        Add(new Item {Text = "Green", Value = ColorChannel.Green});
        Add(new Item {Text = "Blue", Value = ColorChannel.Blue});
        Add(new Item {Text = "Alpha", Value = ColorChannel.Alpha});
    }

    public class Item
    {
        public string? Text {get; set;}
        public ColorChannel? Value {get; set;}
    }
}