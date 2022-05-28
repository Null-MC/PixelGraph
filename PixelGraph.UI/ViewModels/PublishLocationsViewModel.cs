using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.IO.Publishing;
using PixelGraph.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    public class PublishLocationsViewModel : ModelBase
    {
        private readonly IPublishLocationManager publishLocationMgr;
        private ObservableCollection<PublishLocationDisplayModel> _locations;
        private PublishLocationDisplayModel _selectedLocationItem;

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

        public string EditPath {
            get => _selectedLocationItem?.Path;
            set {
                if (_selectedLocationItem == null) return;
                _selectedLocationItem.Path = value;
                OnPropertyChanged();

                HasChanges = true;
            }
        }

        public bool EditArchive {
            get => _selectedLocationItem?.Archive ?? false;
            set {
                if (_selectedLocationItem == null) return;
                _selectedLocationItem.Archive = value;
                OnPropertyChanged();

                HasChanges = true;
            }
        }

        public ObservableCollection<PublishLocationDisplayModel> Locations {
            get => _locations;
            private set {
                _locations = value;
                OnPropertyChanged();
            }
        }

        public PublishLocationDisplayModel SelectedLocationItem {
            get => _selectedLocationItem;
            set {
                _selectedLocationItem = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasSelectedLocation));
                OnPropertyChanged(nameof(EditName));
                OnPropertyChanged(nameof(EditPath));
                OnPropertyChanged(nameof(EditArchive));
            }
        }


        public PublishLocationsViewModel(IServiceProvider provider)
        {
            if (provider == null) return;

            publishLocationMgr = provider.GetRequiredService<IPublishLocationManager>();

            var locations = publishLocationMgr.GetLocations() ?? Enumerable.Empty<PublishLocation>();
            var displayLocations = locations.Select(l => new PublishLocationDisplayModel(l)).ToArray();
            Locations = new ObservableCollection<PublishLocationDisplayModel>(displayLocations);

            SelectedLocationItem = displayLocations.FirstOrDefault(l => string.Equals(l.DisplayName, publishLocationMgr.SelectedLocation, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddNew()
        {
            var newLocation = new PublishLocationDisplayModel();
            Locations.Add(newLocation);
            SelectedLocationItem = newLocation;
            HasChanges = true;
        }

        public void RemoveSelected()
        {
            if (!HasSelectedLocation) return;

            Locations.Remove(SelectedLocationItem);
            SelectedLocationItem = null;
            HasChanges = true;
        }

        public void DuplicateSelected()
        {
            if (!HasSelectedLocation) return;

            var newLocationData = (PublishLocation)SelectedLocationItem.DataSource.Clone();
            var newLocation = new PublishLocationDisplayModel(newLocationData);

            Locations.Add(newLocation);
            SelectedLocationItem = newLocation;
            HasChanges = true;
        }

        public async Task SaveAsync()
        {
            var locations = Locations.Select(x => x.DataSource);
            publishLocationMgr.SetLocations(locations);
            publishLocationMgr.SelectedLocation = SelectedLocationItem?.DisplayName;

            await publishLocationMgr.SaveAsync();
        }

        //public async Task SaveChangesAsync(CancellationToken token = default)
        //{
        //    var rows = Model.Locations.Select(x => x.DataSource);
        //    await locationMgr.SaveAsync(rows, token);
        //}
    }

    internal class PublishLocationsDesignerViewModel : PublishLocationsViewModel
    {
        public PublishLocationsDesignerViewModel() : base(null) {}
    }
}
