using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
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

            Model.Texture_ImageEditorExe = appSettings.Data.TextureEditorExecutable;
            Model.Texture_ImageEditorArgs = appSettings.Data.TextureEditorArguments;
            Model.Theme_BaseColor = appSettings.Data.ThemeBaseColor;
            Model.Theme_AccentColor = appSettings.Data.ThemeAccentColor;
        }

        public void ResetImageEditor()
        {
            Model.Texture_ImageEditorExe = SettingsDataModel.DefaultImageEditorExe;
            Model.Texture_ImageEditorArgs = SettingsDataModel.DefaultImageEditorArgs;
        }

        public void ResetThemeColors()
        {
            Model.Theme_BaseColor = SettingsDataModel.DefaultThemeBaseColor;
            Model.Theme_AccentColor = SettingsDataModel.DefaultThemeAccentColor;
        }

        public async Task<bool> SaveSettingsAsync(CancellationToken token)
        {
            try {
                appSettings.Data.TextureEditorExecutable = Model.Texture_ImageEditorExe;
                appSettings.Data.TextureEditorArguments = Model.Texture_ImageEditorArgs;
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
