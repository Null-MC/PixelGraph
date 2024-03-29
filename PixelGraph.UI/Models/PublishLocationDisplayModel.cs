﻿using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO.Publishing;

namespace PixelGraph.UI.Models;

public class PublishLocationDisplayModel : ModelBase
{
    public PublishLocation DataSource {get;}
    private bool _isManualSelect {get; set;}

    public string? DisplayName {
        get => DataSource.Name;
        set {
            DataSource.Name = value;
            OnPropertyChanged();
        }
    }

    public string? Path {
        get => DataSource.Path;
        set {
            DataSource.Path = value;
            OnPropertyChanged();
        }
    }

    public bool Archive {
        get => DataSource.Archive;
        set {
            DataSource.Archive = value;
            OnPropertyChanged();
        }
    }

    public bool IsManualSelect {
        get => _isManualSelect;
        set {
            _isManualSelect = value;
            OnPropertyChanged();
        }
    }


    public PublishLocationDisplayModel()
    {
        DataSource = new PublishLocation();
    }

    public PublishLocationDisplayModel(PublishLocation location)
    {
        DataSource = location;
    }
}