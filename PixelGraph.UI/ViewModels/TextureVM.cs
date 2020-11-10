using PixelGraph.Common;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureVM : ViewModelBase
    {
        private PbrProperties _properties;
        private string _search;

        public string Name {get; set;}
        public ObservableCollection<TextureSource> Textures {get;}
        public TextureTreeNode TreeRoot {get;}
        //public ImageSource AlbedoSource {get; private set;}

        public string Search {
            get => _search;
            set {
                _search = value;
                OnPropertyChanged();
            }
        }

        private TextureSource _selected;
        public TextureSource Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged();
            }
        }


        public TextureVM()
        {
            TreeRoot = new TextureTreeNode();
            Textures = new ObservableCollection<TextureSource>();
        }

        public void SetTexture(PbrProperties texture)
        {
            _properties = texture;
        }
    }

    internal class TextureSource
    {
        public string Tag {get; set;}
        public ImageSource Thumbnail {get; set;}
        public ImageSource Image {get; set;}
        public string Name {get; set;}
        public string Filename {get; set;}
    }
}
