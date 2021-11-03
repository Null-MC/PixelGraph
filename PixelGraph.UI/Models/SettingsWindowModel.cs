using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.Models
{
    public class SettingsWindowModel : ModelBase
    {
        private int? _app_Concurrency;
        private string _texture_imageEditorExe;
        private string _texture_imageEditorArgs;
        private bool _renderPreview_enabled;
        private bool _renderPreview_enableLinearSampling;
        private bool _renderPreview_enableSlopeNormals;
        private bool _renderPreview_enablePuddles;
        //private bool _renderPreview_parallaxEnabled;
        private decimal? _renderPreview_parallaxDepth;
        private int? _renderPreview_parallaxSamplesMin;
        private int? _renderPreview_parallaxSamplesMax;
        private string _theme_baseColor;
        private string _theme_accentColor;

        public event EventHandler DataChanged;

        public bool IsLoading {get; set;}

        public int? App_Concurrency {
            get => _app_Concurrency;
            set {
                if (_app_Concurrency == value) return;
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(App_Concurrency), "value must be greater than 0!");
                _app_Concurrency = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(App_EditConcurrency));
                OnDataChanged();
            }
        }

        public int? App_EditConcurrency {
            get => _app_Concurrency ?? Environment.ProcessorCount;
            set => App_Concurrency = value;
        }

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

        public bool RenderPreview_Enabled {
            get => _renderPreview_enabled;
            set {
                _renderPreview_enabled = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool RenderPreview_EnableLinearSampling {
            get => _renderPreview_enableLinearSampling;
            set {
                _renderPreview_enableLinearSampling = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool RenderPreview_EnableSlopeNormals {
            get => _renderPreview_enableSlopeNormals;
            set {
                _renderPreview_enableSlopeNormals = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool RenderPreview_EnablePuddles {
            get => _renderPreview_enablePuddles;
            set {
                _renderPreview_enablePuddles = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        //public bool RenderPreview_ParallaxEnabled {
        //    get => _renderPreview_parallaxEnabled;
        //    set {
        //        _renderPreview_parallaxEnabled = value;
        //        OnPropertyChanged();
        //        OnDataChanged();
        //    }
        //}

        public decimal? RenderPreview_ParallaxDepth {
            get => _renderPreview_parallaxDepth;
            set {
                _renderPreview_parallaxDepth = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? RenderPreview_ParallaxSamplesMin {
            get => _renderPreview_parallaxSamplesMin;
            set {
                _renderPreview_parallaxSamplesMin = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? RenderPreview_ParallaxSamplesMax {
            get => _renderPreview_parallaxSamplesMax;
            set {
                _renderPreview_parallaxSamplesMax = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string App_ThemeBaseColor {
            get => _theme_baseColor;
            set {
                _theme_baseColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string App_ThemeAccentColor {
            get => _theme_accentColor;
            set {
                _theme_accentColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        //public void LoadData(AppSettingsDataModel data)
        //{
        //    _isLoading = true;

        //    Texture_ImageEditorExe = data.TextureEditorExecutable;
        //    Texture_ImageEditorArgs = data.TextureEditorArguments;

        //    RenderPreview_Enabled = data.RenderPreview.Enabled ?? RenderPreviewSettings.Default_Enabled;
        //    RenderPreview_ParallaxEnabled = data.RenderPreview.Enabled ?? RenderPreviewSettings.Default_Enabled;
        //    RenderPreview_ParallaxDepth = data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth;
        //    RenderPreview_ParallaxSamplesMin = data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
        //    RenderPreview_ParallaxSamplesMax = data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;

        //    Theme_BaseColor = data.ThemeBaseColor;
        //    Theme_AccentColor = data.ThemeAccentColor;

        //    _isLoading = false;
        //}

        private void OnDataChanged()
        {
            if (IsLoading) return;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AppSettingsDesignVM : SettingsWindowModel
    {
        //
    }
}
