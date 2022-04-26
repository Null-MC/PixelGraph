using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class SettingsViewModel
    {
        private readonly IAppSettings appSettings;

        public SettingsWindowModel Model {get; set;}


        public SettingsViewModel(IServiceProvider provider)
        {
            appSettings = provider.GetRequiredService<IAppSettings>();
        }

        public void LoadData()
        {
            Model.IsLoading = true;

            Model.App_Concurrency = appSettings.Data.Concurrency;
            Model.App_ThemeBaseColor = appSettings.Data.ThemeBaseColor;
            Model.App_ThemeAccentColor = appSettings.Data.ThemeAccentColor;

            Model.Texture_ImageEditorExe = appSettings.Data.TextureEditorExecutable;
            Model.Texture_ImageEditorArgs = appSettings.Data.TextureEditorArguments;

            Model.RenderPreview_Enabled = appSettings.Data.RenderPreview.Enabled ?? RenderPreviewSettings.Default_Enabled;
            //Model.RenderPreview_EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;
            //Model.RenderPreview_EnableSlopeNormals = appSettings.Data.RenderPreview.EnableSlopeNormals ?? RenderPreviewSettings.Default_EnableSlopeNormals;
            Model.RenderPreview_EnableBloom = appSettings.Data.RenderPreview.EnableBloom ?? RenderPreviewSettings.Default_EnableBloom;
            Model.RenderPreview_WaterMode = appSettings.Data.RenderPreview.WaterMode ?? RenderPreviewSettings.Default_WaterMode;
            //Model.RenderPreview_ParallaxEnabled = appSettings.Data.RenderPreview.ParallaxEnabled ?? RenderPreviewSettings.Default_ParallaxEnabled;
            Model.RenderPreview_ParallaxDepth = appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth;
            //Model.RenderPreview_ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.RenderPreview_ParallaxSamples = appSettings.Data.RenderPreview.ParallaxSamples ?? RenderPreviewSettings.Default_ParallaxSamples;
            
            Model.IsLoading = false;
        }

        public void ResetImageEditor()
        {
            Model.Texture_ImageEditorExe = AppSettingsDataModel.DefaultImageEditorExe;
            Model.Texture_ImageEditorArgs = AppSettingsDataModel.DefaultImageEditorArgs;
        }

        public void ResetThemeColors()
        {
            Model.App_ThemeBaseColor = AppSettingsDataModel.DefaultThemeBaseColor;
            Model.App_ThemeAccentColor = AppSettingsDataModel.DefaultThemeAccentColor;
        }

        public void ResetRenderPreview()
        {
            Model.RenderPreview_Enabled = RenderPreviewSettings.Default_Enabled;
            //Model.RenderPreview_EnableLinearSampling = RenderPreviewSettings.Default_EnableLinearSampling;
            //Model.RenderPreview_EnableSlopeNormals = RenderPreviewSettings.Default_EnableSlopeNormals;
            Model.RenderPreview_EnableBloom = RenderPreviewSettings.Default_EnableBloom;
            Model.RenderPreview_WaterMode = RenderPreviewSettings.Default_WaterMode;
            //Model.RenderPreview_ParallaxEnabled = RenderPreviewSettings.Default_ParallaxEnabled;
            Model.RenderPreview_ParallaxDepth = RenderPreviewSettings.Default_ParallaxDepth;
            //Model.RenderPreview_ParallaxSamplesMin = RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.RenderPreview_ParallaxSamples = RenderPreviewSettings.Default_ParallaxSamples;
        }

        public async Task<bool> SaveSettingsAsync(CancellationToken token)
        {
            try {
                appSettings.Data.Concurrency = Model.App_Concurrency;
                appSettings.Data.ThemeBaseColor = Model.App_ThemeBaseColor;
                appSettings.Data.ThemeAccentColor = Model.App_ThemeAccentColor;

                appSettings.Data.TextureEditorExecutable = Model.Texture_ImageEditorExe;
                appSettings.Data.TextureEditorArguments = Model.Texture_ImageEditorArgs;

                appSettings.Data.RenderPreview.Enabled = Model.RenderPreview_Enabled == RenderPreviewSettings.Default_Enabled ? null : Model.RenderPreview_Enabled;
                //appSettings.Data.RenderPreview.EnableLinearSampling = Model.RenderPreview_EnableLinearSampling == RenderPreviewSettings.Default_EnableLinearSampling ? null : Model.RenderPreview_EnableLinearSampling;
                //appSettings.Data.RenderPreview.EnableSlopeNormals = Model.RenderPreview_EnableSlopeNormals == RenderPreviewSettings.Default_EnableSlopeNormals ? null : Model.RenderPreview_EnableSlopeNormals;
                appSettings.Data.RenderPreview.EnableBloom = Model.RenderPreview_EnableBloom == RenderPreviewSettings.Default_EnableBloom ? null : Model.RenderPreview_EnableBloom;
                appSettings.Data.RenderPreview.WaterMode = Model.RenderPreview_WaterMode == RenderPreviewSettings.Default_WaterMode ? null : Model.RenderPreview_WaterMode;
                //appSettings.Data.RenderPreview.ParallaxEnabled = Model.RenderPreview_ParallaxEnabled == RenderPreviewSettings.Default_ParallaxEnabled ? null : Model.RenderPreview_ParallaxEnabled;
                appSettings.Data.RenderPreview.ParallaxDepth = Model.RenderPreview_ParallaxDepth == RenderPreviewSettings.Default_ParallaxDepth ? null : Model.RenderPreview_ParallaxDepth;
                //appSettings.Data.RenderPreview.ParallaxSamplesMin = Model.RenderPreview_ParallaxSamplesMin == RenderPreviewSettings.Default_ParallaxSamplesMin ? null : Model.RenderPreview_ParallaxSamplesMin;
                appSettings.Data.RenderPreview.ParallaxSamples = Model.RenderPreview_ParallaxSamples == RenderPreviewSettings.Default_ParallaxSamples ? null : Model.RenderPreview_ParallaxSamples;

                await appSettings.SaveAsync(token);
                return true;
            }
            catch {
                // TODO: log
                // TODO: show
                return false;
            }
        }
    }
}
