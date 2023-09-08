using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData;

internal class OptionalColorChannelValues : List<OptionalColorChannelValues.Item>
{
    public OptionalColorChannelValues()
    {
        Add(new Item {Text = string.Empty});
        Add(new Item {Text = "Red", Value = ColorChannel.Red});
        Add(new Item {Text = "Green", Value = ColorChannel.Green});
        Add(new Item {Text = "Blue", Value = ColorChannel.Blue});
        Add(new Item {Text = "Alpha", Value = ColorChannel.Alpha});
    }

    public class Item
    {
        public string Text {get; set;}
        public ColorChannel? Value {get; set;}
    }
}