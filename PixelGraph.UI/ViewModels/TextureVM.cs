using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureVM : ViewModelBase
    {
        private string _search;
        private TextureSource _selected;
        private DiffuseMaterial _albedoMaterial;

        //public string Name {get; set;}
        public ObservableCollection<TextureSource> Textures {get;}
        public TextureTreeNode TreeRoot {get;}

        public string Search {
            get => _search;
            set {
                _search = value;
                OnPropertyChanged();
            }
        }

        public TextureSource Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public DiffuseMaterial AlbedoMaterial {
            get => _albedoMaterial;
            set {
                _albedoMaterial = value;
                OnPropertyChanged();
            }
        }


        public TextureVM()
        {
            TreeRoot = new TextureTreeNode();
            Textures = new ObservableCollection<TextureSource>();
        }

        public TextureTreeNode GetTreeNode(string path)
        {
            var parts = path.Split('/', '\\');
            var parent = TreeRoot;

            foreach (var part in parts) {
                var node = parent.Nodes.FirstOrDefault(x => string.Equals(x.Name, part, StringComparison.InvariantCultureIgnoreCase));

                if (node == null) {
                    node = new TextureTreeDirectory {Name = part};
                    parent.Nodes.Add(node);
                }

                parent = node;
            }

            return parent;
        }
    }

    internal class TextureSource
    {
        public string Tag {get; set;}
        public ImageSource Thumbnail {get; set;}
        public ImageSource Image {get; set;}
        public string Name {get; set;}
        //public string Filename {get; set;}
    }
}
