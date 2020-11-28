using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels
{
    internal class TextureTreeNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name {get; set;}
        public ObservableCollection<TextureTreeNode> Nodes {get; set;}

        private bool _visible;
        public bool Visible {
            get => _visible;
            set {
                _visible = value;
                OnPropertyChanged();
            }
        }


        public TextureTreeNode()
        {
            Nodes = new ObservableCollection<TextureTreeNode>();
            Visible = true;
        }

        public virtual void UpdateVisibility(ISearchParameters search)
        {
            Visible = true;

            foreach (var node in Nodes)
                node.UpdateVisibility(search);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class TextureTreeDirectory : TextureTreeNode
    {
        public string Path {get; set;}

        public override void UpdateVisibility(ISearchParameters search)
        {
            var anyVisible = false;

            foreach (var node in Nodes) {
                node.UpdateVisibility(search);

                //if (node.Visible) anyVisible = true;
                anyVisible |= node.Visible;
            }

            Visible = anyVisible;
        }
    }

    internal class TextureTreeFile : TextureTreeNode
    {
        public string Filename {get; set;}
        public NodeType Type {get; set;}
        public PackIconKind Icon {get; set;}


        public TextureTreeFile()
        {
            Icon = PackIconKind.File;
        }

        public override void UpdateVisibility(ISearchParameters search)
        {
            if (search.ShowAllFiles) Visible = true;
            else Visible = Type == NodeType.Material;
            if (!Visible) return;

            if (!string.IsNullOrEmpty(search.SearchText)) {
                Visible = Name.Contains(search.SearchText, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }

    //internal class TextureTreeMaterial : TextureTreeFile
    //{
    //    //public string MaterialFilename {get; set;}
    //    public MaterialProperties Material {get; set;}
    //    //public ImageSource AlbedoSource {get; set;}
    //}

    internal enum NodeType
    {
        Unknown,
        PackInput,
        PackProfile,
        Material,
        Texture,
        Model,
    }
}
