using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewData;

internal class NormalColorChannelValues : ColorChannelValues
{
    public NormalColorChannelValues()
    {
        Add(new Item {Text = "Magnitude", Value = ColorChannel.Magnitude});
    }
}