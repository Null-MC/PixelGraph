using PixelGraph.Common;
using PixelGraph.Common.Textures;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureVM : ViewModelBase
    {
        private string _treeSearch;
        private TextureTreeNode _selectedNode;
        private TextureSource _selectedSource;
        //private DiffuseMaterial _albedoMaterial;
        private PbrProperties _loaded;
        private string _loadedFilename;

        //public event EventHandler DataChanged;

        //public string Name {get; set;}
        public ObservableCollection<TextureSource> Textures {get;}
        public TextureTreeNode TreeRoot {get;}

        public bool HasTreeSelection => _selectedNode is TextureTreeTexture;

        public string TreeSearch {
            get => _treeSearch;
            set {
                _treeSearch = value;
                OnPropertyChanged();
            }
        }

        public TextureTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasTreeSelection));
            }
        }

        public TextureSource SelectedSource {
            get => _selectedSource;
            set {
                _selectedSource = value;
                OnPropertyChanged();
            }
        }

        //public DiffuseMaterial AlbedoMaterial {
        //    get => _albedoMaterial;
        //    set {
        //        _albedoMaterial = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string LoadedFilename {
            get => _loadedFilename;
            set {
                _loadedFilename = value;
                OnPropertyChanged();
            }
        }

        public PbrProperties Loaded {
            get => _loaded;
            set {
                _loaded = value;
                OnPropertyChanged();

                //OnPropertyChanged(nameof(Wrap));
            }
        }

        //public bool Wrap {
        //    get => _loaded?.Wrap ?? true;
        //    set {
        //        if (_loaded == null) return;
        //        _loaded.Wrap = value;
        //        OnPropertyChanged();
        //        OnDataChanged();
        //    }
        //}


        public TextureVM()
        {
            TreeRoot = new TextureTreeNode();
            Textures = new ObservableCollection<TextureSource>();
        }

        internal TextureTreeNode GetTreeNode(string path)
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

        public void SelectFirstTexture()
        {
            var albedo = Textures.FirstOrDefault(x => TextureTags.Is(x.Tag, TextureTags.Albedo));
            SelectedSource = albedo ?? Textures.FirstOrDefault();
        }

        //private void OnDataChanged()
        //{
        //    DataChanged?.Invoke(this, EventArgs.Empty);
        //}
    }

    internal class TextureSource
    {
        public string Tag {get; set;}
        public string Name {get; set;}
        public string Filename {get; set;}
        public BitmapSource Thumbnail {get; set;}
        public BitmapSource Image {get; set;}
    }
}
