using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.Models.Tabs;

public interface ITabModel
{
    Guid Id {get;}
    string DisplayName {get;}
    bool IsPreview {get; set;}
    bool IsLoading {get; set;}
    //bool IsSelected {get; set;}
    //bool IsMouseOver {get; set;}
}

public abstract class TabModelBase : ModelBase, ITabModel
{
    private string _displayName {get; set;}
    private bool _isPreview {get; set;}
    private bool _isLoading {get; set;}
    //private bool _isSelected {get; set;}
    //private bool _isMouseOver {get; set;}

    public Guid Id {get;}

    public string DisplayName {
        get => _displayName;
        set {
            _displayName = value;
            OnPropertyChanged();
        }
    }

    public bool IsPreview {
        get => _isPreview;
        set {
            _isPreview = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading {
        get => _isLoading;
        set {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    //public bool IsSelected {
    //    get => _isSelected;
    //    set {
    //        _isSelected = value;
    //        OnPropertyChanged();
    //    }
    //}

    //public bool IsMouseOver {
    //    get => _isMouseOver;
    //    set {
    //        _isMouseOver = value;
    //        OnPropertyChanged();
    //    }
    //}


    protected TabModelBase()
    {
        Id = Guid.NewGuid();
    }
}