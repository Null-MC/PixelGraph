using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO.Resources;
using PixelGraph.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels;

public class ResourceLocationsViewModel : ModelBase
{
    private readonly IResourceLocationManager resourceLocationMgr;
    private ObservableCollection<ResourceLocationDisplayModel> _locations;
    private ResourceLocationDisplayModel _selectedLocationItem;

    public bool HasChanges {get; set;}

    public bool HasSelectedLocation => _selectedLocationItem != null;

    public string EditName {
        get => _selectedLocationItem?.DisplayName;
        set {
            if (_selectedLocationItem == null) return;
            _selectedLocationItem.DisplayName = value;
            OnPropertyChanged();

            HasChanges = true;
        }
    }

    public string EditFile {
        get => _selectedLocationItem?.FileName;
        set {
            if (_selectedLocationItem == null) return;
            _selectedLocationItem.FileName = value;
            OnPropertyChanged();

            HasChanges = true;
        }
    }

    public ObservableCollection<ResourceLocationDisplayModel> Locations {
        get => _locations;
        set {
            _locations = value;
            OnPropertyChanged();
        }
    }

    public ResourceLocationDisplayModel SelectedLocationItem {
        get => _selectedLocationItem;
        set {
            _selectedLocationItem = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(HasSelectedLocation));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditFile));
        }
    }


    public ResourceLocationsViewModel(IServiceProvider provider)
    {
        if (provider == null) return;

        resourceLocationMgr = provider.GetRequiredService<IResourceLocationManager>();

        var locations = resourceLocationMgr.GetLocations() ?? Enumerable.Empty<ResourceLocation>();
        var displayLocations = locations.Select(l => new ResourceLocationDisplayModel(l)).ToArray();
        Locations = new ObservableCollection<ResourceLocationDisplayModel>(displayLocations);
        SelectedLocationItem = displayLocations.FirstOrDefault();
    }

    public void AddFiles(IEnumerable<string> filenames)
    {
        ResourceLocationDisplayModel lastAdded = null;
        foreach (var filename in filenames) {
            var location = new ResourceLocation {
                Name = System.IO.Path.GetFileNameWithoutExtension(filename),
                File = filename,
            };

            var displayLocation = new ResourceLocationDisplayModel(location);
            Locations.Add(displayLocation);
            lastAdded = displayLocation;
        }

        SelectedLocationItem = lastAdded;
    }

    public void RemoveSelected()
    {
        if (!HasSelectedLocation) return;

        Locations.Remove(SelectedLocationItem);
        SelectedLocationItem = null;
        HasChanges = true;
    }

    public async Task SaveAsync()
    {
        var locations = Locations.Select(x => x.DataSource);
        resourceLocationMgr.SetLocations(locations);

        await resourceLocationMgr.SaveAsync();
    }
}

internal class ResourceLocationsDesignerViewModel : ResourceLocationsViewModel
{
    public ResourceLocationsDesignerViewModel() : base(null) {}
}