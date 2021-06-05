using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.Models
{
    public class SettingsWindowModel : ModelBase
    {
        private string _texture_imageEditorExe;
        private string _texture_imageEditorArgs;
        private string _theme_baseColor;
        private string _theme_accentColor;

        public event EventHandler DataChanged;

        public string Texture_ImageEditorExe {
            get => _texture_imageEditorExe;
            set {
                _texture_imageEditorExe = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string Texture_ImageEditorArgs {
            get => _texture_imageEditorArgs;
            set {
                _texture_imageEditorArgs = value;
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

    public class AppSettingsDesignVM : SettingsWindowModel
    {
        //
    }
}
