using PixelGraph.Common;
using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureVM : ViewModelBase
    {
        private readonly PbrProperties texture;

        public string Name {get; set;}
        public ImageSource AlbedoSource {get; set;}


        public TextureVM(PbrProperties texture)
        {
            this.texture = texture;
        }
    }
}
