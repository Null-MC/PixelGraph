using PixelGraph.Common;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class PbrPropertiesVM : ViewModelBase
    {
        protected PbrProperties _texture;

        public event EventHandler DataChanged;

        public PbrProperties Texture {
            get => _texture;
            set {
                _texture = value;
                OnPropertyChanged();
            }
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class PbrPropertiesDesignVM : PbrPropertiesVM
    {
        public PbrPropertiesDesignVM()
        {
            _texture = new PbrProperties {
                Wrap = true,
                Properties = {
                    ["height.value"] = "128",
                },
            };
        }
    }
}
