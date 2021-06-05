using PixelGraph.Common.Material;
using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.Models
{
    public class MaterialContextModel : ModelBase
    {
        private string _loadedFilename;
        private MaterialProperties _loaded;

        public event EventHandler MaterialChanged;

        public bool HasLoaded => _loaded != null;

        public string LoadedFilename {
            get => _loadedFilename;
            set {
                _loadedFilename = value;
                OnPropertyChanged();
            }
        }

        public MaterialProperties Loaded {
            get => _loaded;
            set {
                if (value == _loaded) return;
                _loaded = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLoaded));
                OnMaterialChanged();
            }
        }


        private void OnMaterialChanged()
        {
            MaterialChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
