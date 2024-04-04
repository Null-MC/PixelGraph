using PixelGraph.UI.Internal.IO.Resources;
using PixelGraph.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.UI.ViewModels;

public class ResourceLocationsViewModel : INotifyPropertyChanged
{
    private readonly IResourceLocationManager resourceLocationMgr;
    private ResourceLocationDisplayModel? _selectedLocationItem;

    public bool HasChanges {get; set;}

    public bool HasSelectedLocation => _selectedLocationItem != null;

    public string? EditName {
        get => _selectedLocationItem?.DisplayName;
        set {
            if (_selectedLocationItem == null) return;
            _selectedLocationItem.DisplayName = value;
            OnPropertyChanged();

            HasChanges = true;
        }
    }

    public string? EditFile {
        get => _selectedLocationItem?.FileName;
        set {
            if (_selectedLocationItem == null) return;
            _selectedLocationItem.FileName = value;
            OnPropertyChanged();

            HasChanges = true;
        }
    }

    public ObservableCollection<ResourceLocationDisplayModel> Locations {get;}

    public ResourceLocationDisplayModel? SelectedLocationItem {
        get => _selectedLocationItem;
        set {
            _selectedLocationItem = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(HasSelectedLocation));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(EditFile));
        }
    }


    public ResourceLocationsViewModel(IResourceLocationManager resourceLocationMgr)
    {
        this.resourceLocationMgr = resourceLocationMgr;

        var locations = resourceLocationMgr.GetLocations() ?? Enumerable.Empty<ResourceLocation>();
        var displayLocations = locations.Select(l => new ResourceLocationDisplayModel(l)).ToArray();
        Locations = new ObservableCollection<ResourceLocationDisplayModel>(displayLocations);
        SelectedLocationItem = displayLocations.FirstOrDefault();
    }

    public void AddFiles(IEnumerable<string> filenames)
    {
        ResourceLocationDisplayModel? lastAdded = null;
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
        if (_selectedLocationItem == null) return;

        Locations.Remove(_selectedLocationItem);
        SelectedLocationItem = null;
        HasChanges = true;
    }

    public async Task SaveAsync()
    {
        var locations = Locations.Select(x => x.DataSource);
        resourceLocationMgr.SetLocations(locations);

        await resourceLocationMgr.SaveAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

internal class ResourceLocationsDesignerViewModel() : ResourceLocationsViewModel(null);
