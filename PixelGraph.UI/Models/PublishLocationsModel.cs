using PixelGraph.UI.Internal;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.Models
{
    internal class PublishLocationsModel : ModelBase
    {
        protected ObservableCollection<LocationDisplayModel> _locations;
        private LocationDisplayModel _selectedLocationItem;

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

        public ObservableCollection<LocationDisplayModel> Locations {
            get => _locations;
            set {
                _locations = value;
                OnPropertyChanged();
            }
        }

        public LocationDisplayModel SelectedLocationItem {
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
    }
}
