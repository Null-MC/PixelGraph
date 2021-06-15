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
            Model.Texture_ImageEditorExe = appSettings.Data.TextureEditorExecutable;
            Model.Texture_ImageEditorArgs = appSettings.Data.TextureEditorArguments;

            Model.RenderPreview_Enabled = appSettings.Data.RenderPreview.Enabled ?? RenderPreviewSettings.Default_Enabled;
            Model.RenderPreview_EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;
            //Model.RenderPreview_ParallaxEnabled = appSettings.Data.RenderPreview.ParallaxEnabled ?? RenderPreviewSettings.Default_ParallaxEnabled;
            Model.RenderPreview_ParallaxDepth = appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth;
            Model.RenderPreview_ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.RenderPreview_ParallaxSamplesMax = appSettings.Data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;

            Model.Theme_BaseColor = appSettings.Data.ThemeBaseColor;
            Model.Theme_AccentColor = appSettings.Data.ThemeAccentColor;
            Model.IsLoading = false;
        }

        public void ResetImageEditor()
        {
            Model.Texture_ImageEditorExe = AppSettingsDataModel.DefaultImageEditorExe;
            Model.Texture_ImageEditorArgs = AppSettingsDataModel.DefaultImageEditorArgs;
        }

        public void ResetThemeColors()
        {
            Model.Theme_BaseColor = AppSettingsDataModel.DefaultThemeBaseColor;
            Model.Theme_AccentColor = AppSettingsDataModel.DefaultThemeAccentColor;
        }

        public void ResetRenderPreview()
        {
            Model.RenderPreview_Enabled = RenderPreviewSettings.Default_Enabled;
            Model.RenderPreview_EnableLinearSampling = RenderPreviewSettings.Default_EnableLinearSampling;
            //Model.RenderPreview_ParallaxEnabled = RenderPreviewSettings.Default_ParallaxEnabled;
            Model.RenderPreview_ParallaxDepth = RenderPreviewSettings.Default_ParallaxDepth;
            Model.RenderPreview_ParallaxSamplesMin = RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.RenderPreview_ParallaxSamplesMax = RenderPreviewSettings.Default_ParallaxSamplesMax;
        }

        public async Task<bool> SaveSettingsAsync(CancellationToken token)
        {
            try {
                appSettings.Data.TextureEditorExecutable = Model.Texture_ImageEditorExe;
                appSettings.Data.TextureEditorArguments = Model.Texture_ImageEditorArgs;

                appSettings.Data.RenderPreview.Enabled = Model.RenderPreview_Enabled == RenderPreviewSettings.Default_Enabled ? null : Model.RenderPreview_Enabled;
                appSettings.Data.RenderPreview.EnableLinearSampling = Model.RenderPreview_EnableLinearSampling == RenderPreviewSettings.Default_EnableLinearSampling ? null : Model.RenderPreview_EnableLinearSampling;
                //appSettings.Data.RenderPreview.ParallaxEnabled = Model.RenderPreview_ParallaxEnabled == RenderPreviewSettings.Default_ParallaxEnabled ? null : Model.RenderPreview_ParallaxEnabled;
                appSettings.Data.RenderPreview.ParallaxDepth = Model.RenderPreview_ParallaxDepth == RenderPreviewSettings.Default_ParallaxDepth ? null : Model.RenderPreview_ParallaxDepth;
                appSettings.Data.RenderPreview.ParallaxSamplesMin = Model.RenderPreview_ParallaxSamplesMin == RenderPreviewSettings.Default_ParallaxSamplesMin ? null : Model.RenderPreview_ParallaxSamplesMin;
                appSettings.Data.RenderPreview.ParallaxSamplesMax = Model.RenderPreview_ParallaxSamplesMax == RenderPreviewSettings.Default_ParallaxSamplesMax ? null : Model.RenderPreview_ParallaxSamplesMax;

                appSettings.Data.ThemeBaseColor = Model.Theme_BaseColor;
                appSettings.Data.ThemeAccentColor = Model.Theme_AccentColor;

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
