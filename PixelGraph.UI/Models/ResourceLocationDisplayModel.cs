using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO.Resources;

namespace PixelGraph.UI.Models;

public class ResourceLocationDisplayModel : ModelBase
{
    public ResourceLocation DataSource {get;}
    //private bool _isManualSelect {get; set;}

    public string? DisplayName {
        get => DataSource.Name;
        set {
            DataSource.Name = value;
            OnPropertyChanged();
        }
    }

    public string? FileName {
        get => DataSource.File;
        set {
            DataSource.File = value;
            OnPropertyChanged();
        }
    }

    //public bool IsManualSelect {
    //    get => _isManualSelect;
    //    set {
    //        _isManualSelect = value;
    //        OnPropertyChanged();
    //    }
    //}


    //public ResourceLocationDisplayModel()
    //{
    //    DataSource = new ResourceLocation();
    //}

    public ResourceLocationDisplayModel(ResourceLocation location)
    {
        DataSource = location;
    }
}