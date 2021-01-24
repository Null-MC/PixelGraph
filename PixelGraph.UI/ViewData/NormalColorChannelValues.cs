using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewData
{
    internal class NormalColorChannelValues : List<ColorChannelValues.Item>
    {
        public NormalColorChannelValues()
        {
            Add(new ColorChannelValues.Item {Text = "Red", Value = ColorChannel.Red});
            Add(new ColorChannelValues.Item {Text = "Green", Value = ColorChannel.Green});
            Add(new ColorChannelValues.Item {Text = "Blue", Value = ColorChannel.Blue});
            Add(new ColorChannelValues.Item {Text = "Alpha", Value = ColorChannel.Alpha});
            Add(new ColorChannelValues.Item {Text = "Magnitude", Value = ColorChannel.Magnitude});
        }
    }
}
