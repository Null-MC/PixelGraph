using PixelGraph.Common.Projects;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Extensions;

namespace PixelGraph.UI.Models;

public class PublishProfileDisplayRow : ModelBase
{
    public PublishProfileProperties Profile {get;}
    private string? _defaultName;

    public string? DisplayName => Profile.Name?.NullIfWhitespace(_defaultName);

    public string? DefaultName {
        set {
            _defaultName = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(DisplayName));
        }
    }


    public PublishProfileDisplayRow(PublishProfileProperties profile)
    {
        Profile = profile;
    }
}