using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewData;
using System;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.Models
{
    public class ProfileContextModel : ModelBase
    {
        private ResourcePackProfileProperties _loaded;
        private ProfileItem _selected;

        public event EventHandler SelectionChanged;
        public event EventHandler LoadedChanged;

        public ObservableCollection<ProfileItem> List {get;}

        public bool HasSelection => _selected != null;
        public bool HasLoaded => _loaded != null;

        public ProfileItem Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));

                OnSelectionChanged();
            }
        }

        public ResourcePackProfileProperties Loaded {
            get => _loaded;
            set {
                _loaded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLoaded));
                OnLoadedChanged();
            }
        }


        public ProfileContextModel()
        {
            List = new ObservableCollection<ProfileItem>();
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadedChanged()
        {
            LoadedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
