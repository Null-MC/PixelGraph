namespace PixelGraph.UI.ViewData;

internal class ImageEncodingValues : List<ImageEncodingValues.Item>
{
    public ImageEncodingValues()
    {
        Add(new Item {Text = "Bitmap", Value = "bmp"});
        Add(new Item {Text = "Jpeg", Value = "jpg"});
        Add(new Item {Text = "Png", Value = "png"});
        Add(new Item {Text = "Tga", Value = "tga"});
    }

    public class Item
    {
        public string? Text {get; set;}
        public string? Value {get; set;}
    }
}