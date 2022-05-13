using HelixToolkit.SharpDX.Core;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class SettingsViewModel : ModelBase
    {
        private IAppSettings appSettings;
        private AppSettingsDataModel data;
        private bool isLoading;

        public event EventHandler DataChanged;

        public int DefaultConcurrency {get;}
        public string RenderPreview_SubSurfaceBlurText => (RenderPreview_SubSurfaceBlur ?? 0m).ToString("P0");

        public int? App_Concurrency {
            get => data?.Concurrency;
            set {
                if (data == null) return;
                if (value is < 1) throw new ArgumentOutOfRangeException(nameof(App_Concurrency), "value must be greater than 0!");
                data.Concurrency = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string App_ThemeBaseColor {
            get => data?.ThemeBaseColor;
            set {
                if (data == null) return;
                data.ThemeBaseColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string App_ThemeAccentColor {
            get => data?.ThemeAccentColor;
            set {
                if (data == null) return;
                data.ThemeAccentColor = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string Texture_ImageEditorExe {
            get => data?.TextureEditorExecutable;
            set {
                if (data == null) return;
                data.TextureEditorExecutable = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string Texture_ImageEditorArgs {
            get => data?.TextureEditorArguments;
            set {
                if (data == null) return;
                data.TextureEditorArguments = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? RenderPreview_Enabled {
            get => data?.RenderPreview.Enabled;
            set {
                if (data == null) return;
                data.RenderPreview.Enabled = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? RenderPreview_EnableBloom {
            get => data?.RenderPreview.EnableBloom;
            set {
                if (data == null) return;
                data.RenderPreview.EnableBloom = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public bool? RenderPreview_EnableSwapChain {
            get => data?.RenderPreview.EnableSwapChain;
            set {
                if (data == null) return;
                data.RenderPreview.EnableSwapChain = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? RenderPreview_WaterMode {
            get => data?.RenderPreview.WaterMode;
            set {
                if (data == null) return;
                data.RenderPreview.WaterMode = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public FXAALevel? RenderPreview_FXAA {
            get => data?.RenderPreview.FXAA;
            set {
                if (data == null) return;
                data.RenderPreview.FXAA = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? RenderPreview_EnvironmentCubeSize {
            get => data?.RenderPreview.EnvironmentCubeSize;
            set {
                if (data == null) return;
                data.RenderPreview.EnvironmentCubeSize = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? RenderPreview_IrradianceCubeSize {
            get => data?.RenderPreview.IrradianceCubeSize;
            set {
                if (data == null) return;
                data.RenderPreview.IrradianceCubeSize = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? RenderPreview_ParallaxDepth {
            get => data?.RenderPreview.ParallaxDepth;
            set {
                if (data == null) return;
                data.RenderPreview.ParallaxDepth = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public int? RenderPreview_ParallaxSamples {
            get => data?.RenderPreview.ParallaxSamples;
            set {
                if (data == null) return;
                data.RenderPreview.ParallaxSamples = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public decimal? RenderPreview_SubSurfaceBlur {
            get => data?.RenderPreview.SubSurfaceBlur;
            set {
                if (data == null) return;
                data.RenderPreview.SubSurfaceBlur = value;
                OnPropertyChanged();
                OnDataChanged();

                OnPropertyChanged(nameof(RenderPreview_SubSurfaceBlurText));
            }
        }


        public SettingsViewModel()
        {
            DefaultConcurrency = ConcurrencyHelper.GetDefaultValue();
            isLoading = true;
        }

        public void Initialize(IServiceProvider provider)
        {
            appSettings = provider.GetRequiredService<IAppSettings>();
            data = (AppSettingsDataModel)appSettings.Data.Clone();
            UpdateAllProperties();
            isLoading = false;
        }

        public void ResetImageEditor()
        {
            Texture_ImageEditorExe = null;
            Texture_ImageEditorArgs = null;
        }

        public void ResetThemeColors()
        {
            App_ThemeBaseColor = null;
            App_ThemeAccentColor = null;
        }

        public void ResetPreviewRenderer()
        {
            RenderPreview_Enabled = RenderPreviewSettings.Default_Enabled;
            RenderPreview_EnableBloom = RenderPreviewSettings.Default_EnableBloom;
            RenderPreview_EnableSwapChain = RenderPreviewSettings.Default_EnableSwapChain;
            RenderPreview_WaterMode = RenderPreviewSettings.Default_WaterMode;
            RenderPreview_FXAA = RenderPreviewSettings.Default_FXAA;

            RenderPreview_EnvironmentCubeSize = RenderPreviewSettings.Default_EnvironmentCubeSize;
            RenderPreview_IrradianceCubeSize = RenderPreviewSettings.Default_IrradianceCubeSize;
            RenderPreview_SubSurfaceBlur = RenderPreviewSettings.Default_SubSurfaceBlur;
        }

        public void ResetPreviewParallax()
        {
            RenderPreview_ParallaxDepth = null;
            RenderPreview_ParallaxSamples = null;
        }

        public async Task SaveSettingsAsync(CancellationToken token = default)
        {
            appSettings.Data = data;
            await appSettings.SaveAsync(token);
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(App_Concurrency));
            OnPropertyChanged(nameof(App_ThemeBaseColor));
            OnPropertyChanged(nameof(App_ThemeAccentColor));
            OnPropertyChanged(nameof(Texture_ImageEditorExe));
            OnPropertyChanged(nameof(Texture_ImageEditorArgs));
            OnPropertyChanged(nameof(RenderPreview_Enabled));
            OnPropertyChanged(nameof(RenderPreview_EnableBloom));
            OnPropertyChanged(nameof(RenderPreview_EnableSwapChain));
            OnPropertyChanged(nameof(RenderPreview_WaterMode));
            OnPropertyChanged(nameof(RenderPreview_FXAA));
            OnPropertyChanged(nameof(RenderPreview_EnvironmentCubeSize));
            OnPropertyChanged(nameof(RenderPreview_IrradianceCubeSize));
            OnPropertyChanged(nameof(RenderPreview_ParallaxDepth));
            OnPropertyChanged(nameof(RenderPreview_ParallaxSamples));
            OnPropertyChanged(nameof(RenderPreview_SubSurfaceBlur));
            OnPropertyChanged(nameof(RenderPreview_SubSurfaceBlurText));
        }

        private void OnDataChanged()
        {
            if (isLoading) return;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class SettingsDesignerViewModel : SettingsViewModel {}
}
