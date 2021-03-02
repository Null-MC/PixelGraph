using System.Collections.ObjectModel;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishLocationsVM : ViewModelBase
    {
        protected ObservableCollection<LocationViewModel> _locations;
        private LocationViewModel _selectedLocationItem;
        //private bool _hasChanges;
        public bool HasChanges {get; set;}

        public bool HasSelectedLocation => _selectedLocationItem != null;

        //public bool HasChanges {
        //    get => _hasChanges;
        //    set {
        //        _hasChanges = value;
        //        OnPropertyChanged();
        //    }
        //}

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

        public ObservableCollection<LocationViewModel> Locations {
            get => _locations;
            set {
                _locations = value;
                OnPropertyChanged();
            }
        }

        public LocationViewModel SelectedLocationItem {
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
