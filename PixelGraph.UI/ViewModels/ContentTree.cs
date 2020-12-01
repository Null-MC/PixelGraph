using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels
{
    internal class ContentTreeNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name {get; set;}
        public ObservableCollection<ContentTreeNode> Nodes {get; set;}

        private bool _visible;
        public bool Visible {
            get => _visible;
            set {
                _visible = value;
                OnPropertyChanged();
            }
        }


        public ContentTreeNode()
        {
            Nodes = new ObservableCollection<ContentTreeNode>();
            Visible = true;
        }

        public virtual void UpdateVisibility(ISearchParameters search)
        {
            Visible = true;

            foreach (var node in Nodes)
                node.UpdateVisibility(search);
        }

        public virtual void SetVisibility(bool visible)
        {
            Visible = visible;

            foreach (var node in Nodes)
                node.SetVisibility(visible);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ContentTreeDirectory : ContentTreeNode
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

    internal class ContentTreeMaterialDirectory : ContentTreeDirectory
    {
        public string MaterialFilename {get; set;}


        public override void UpdateVisibility(ISearchParameters search)
        {
            Visible = true;

            if (!string.IsNullOrEmpty(search.SearchText)) {
                Visible = Name.Contains(search.SearchText, StringComparison.InvariantCultureIgnoreCase);
            }

            foreach (var node in Nodes) {
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


        public ContentTreeFile()
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
