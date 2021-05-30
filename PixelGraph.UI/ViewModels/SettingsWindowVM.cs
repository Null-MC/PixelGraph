using System;

namespace PixelGraph.UI.ViewModels
{
    public class SettingsWindowVM : ViewModelBase
    {
        private string _texture_imageEditor = "dark";
        private string _theme_baseColor = "dark";
        private string _theme_accentColor = "emerald";

        public event EventHandler DataChanged;

        public string Texture_ImageEditor {
            get => _texture_imageEditor;
            set {
                _texture_imageEditor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string Theme_BaseColor {
            get => _theme_baseColor;
            set {
                _theme_baseColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string Theme_AccentColor {
            get => _theme_accentColor;
            set {
                _theme_accentColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AppSettingsDesignVM : SettingsWindowVM
    {
        //
    }
}
