using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels
{
    internal class ContentTreeNode : INotifyPropertyChanged
    {
        protected ObservableCollection<ContentTreeNode> _nodes;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name {get; set;}
        public string LocalPath {get; set;}
        public ContentTreeNode Parent {get;}

        public ObservableCollection<ContentTreeNode> Nodes {
            get => _nodes;
            set {
                _nodes = value;
                OnPropertyChanged();
            }
        }

        private bool _visible;
        public bool Visible {
            get => _visible;
            set {
                _visible = value;
                OnPropertyChanged();
            }
        }


        public ContentTreeNode(ContentTreeNode parent)
        {
            Parent = parent;

            _nodes = new ObservableCollection<ContentTreeNode>();
            Visible = true;
        }

        public virtual void UpdateVisibility(ISearchParameters search)
        {
            Visible = true;

            foreach (var node in _nodes)
                node.UpdateVisibility(search);
        }

        public virtual void SetVisibility(bool visible)
        {
            Visible = visible;

            foreach (var node in _nodes)
                node.SetVisibility(visible);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ContentTreeDirectory : ContentTreeNode
    {
        //public string Path {get; set;}


        public ContentTreeDirectory(ContentTreeNode parent) : base(parent) {}

        public override void UpdateVisibility(ISearchParameters search)
        {
            var anyVisible = false;

            if (!string.IsNullOrEmpty(search.SearchText)) {
                if (Name.Contains(search.SearchText, StringComparison.InvariantCultureIgnoreCase)) {
                    SetVisibility(true);
                    return;
                }
            }

            foreach (var node in _nodes) {
                node.UpdateVisibility(search);
                anyVisible |= node.Visible;
            }

            Visible = anyVisible;
        }
    }

    internal class ContentTreeMaterialDirectory : ContentTreeDirectory
    {
        public string MaterialFilename {get; set;}


        public ContentTreeMaterialDirectory(ContentTreeNode parent) : base(parent) {}

        public override void UpdateVisibility(ISearchParameters search)
        {
            Visible = true;

            if (!string.IsNullOrEmpty(search.SearchText)) {
                Visible = Name.Contains(search.SearchText, StringComparison.InvariantCultureIgnoreCase);
            }

            foreach (var node in _nodes) {
                if (!search.ShowAllFiles)
                    node.SetVisibility(false);
                else
                    node.UpdateVisibility(search);
            }
        }
    }

    internal class ContentTreeFile : ContentTreeNode
    {
        public string Filename {get; set;}
        public ContentNodeType Type {get; set;}
        public PackIconKind Icon {get; set;}


        public ContentTreeFile(ContentTreeNode parent) : base(parent)
        {
            Icon = PackIconKind.File;
        }

        public override void UpdateVisibility(ISearchParameters search)
        {
            if (search.ShowAllFiles) Visible = true;
            else Visible = Type == ContentNodeType.Material;
            if (!Visible) return;

            if (!string.IsNullOrEmpty(search.SearchText)) {
                Visible = Name.Contains(search.SearchText, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }

    public enum ContentNodeType
    {
        Unknown,
        PackInput,
        PackProfile,
        Material,
        Texture,
        Model,
    }
}
