using System.Windows.Media.Imaging;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureSource
    {
        public string Tag {get; set;}
        public string Name {get; set;}
        //public string Filename {get; set;}
        public BitmapSource Thumbnail {get; set;}
        public BitmapSource Image {get; set;}
    }
}
