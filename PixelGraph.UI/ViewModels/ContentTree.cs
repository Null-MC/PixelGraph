using MahApps.Metro.IconPacks;
using PixelGraph.UI.ViewData;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels;

public class ContentTreeNode : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public string Name {get; set;}
    public string LocalPath {get; set;}
    public ContentTreeNode Parent {get;}

    public ObservableCollection<ContentTreeNode> Nodes {get;}

    //set {
    //    _nodes = value;
    //    OnPropertyChanged();
    //}
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

        Nodes = new ObservableCollection<ContentTreeNode>();
        Visible = true;
    }

    public virtual void UpdateVisibility(ISearchParameters search)
    {
        Visible = true;

        foreach (var node in Nodes)
            node.UpdateVisibility(search);
    }

    public void SetVisibility(bool visible)
    {
        Visible = visible;

        foreach (var node in Nodes)
            node.SetVisibility(visible);
    }

    //public ContentTreeNode FindNode(Func<ContentTreeNode, bool> predicate)
    //{
    //    if (predicate(this)) return this;

    //    foreach (var node in _nodes) {
    //        var result = node.FindNode(predicate);
    //        if (result != null) return result;
    //    }

    //    return null;
    //}

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class ContentTreeDirectory : ContentTreeNode
{
    public ContentTreeDirectory(ContentTreeNode parent) : base(parent) {}

    public override void UpdateVisibility(ISearchParameters search)
    {
        var anyVisible = false;

        if (!string.IsNullOrEmpty(search.SearchText)) {
            if (Name != null && Name.Contains(search.SearchText, StringComparison.InvariantCultureIgnoreCase)) {
                SetVisibility(true);
                return;
            }
        }

        foreach (var node in Nodes) {
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
    public PackIconFontAwesomeKind Icon {get; set;}


    public ContentTreeFile(ContentTreeNode parent) : base(parent)
    {
        Icon = PackIconFontAwesomeKind.FileSolid;
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
    //Model,
}