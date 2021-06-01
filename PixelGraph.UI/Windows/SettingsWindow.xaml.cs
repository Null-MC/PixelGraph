using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class SettingsWindow
    {
        private readonly IAppSettings appSettings;


        public SettingsWindow(IServiceProvider provider)
        {
            appSettings = provider.GetRequiredService<IAppSettings>();

            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);

            VM.Texture_ImageEditorExe = appSettings.Data.TextureEditorExecutable;
            VM.Texture_ImageEditorArgs = appSettings.Data.TextureEditorArguments;
            VM.Theme_BaseColor = appSettings.Data.ThemeBaseColor;
            VM.Theme_AccentColor = appSettings.Data.ThemeAccentColor;

            VM.DataChanged += OnVMDataChanged;
        }

        private async Task<bool> SaveSettingsAsync(CancellationToken token)
        {
            try {
                appSettings.Data.TextureEditorExecutable = VM.Texture_ImageEditorExe;
                appSettings.Data.TextureEditorArguments = VM.Texture_ImageEditorArgs;
                appSettings.Data.ThemeBaseColor = VM.Theme_BaseColor;
                appSettings.Data.ThemeAccentColor = VM.Theme_AccentColor;

                await appSettings.SaveAsync(token);
                return true;
            }
            catch {
                // TODO: log
                // TODO: show
                return false;
            }
        }

        private void OnVMDataChanged(object sender, EventArgs e)
        {
            ThemeManager.Current.ChangeTheme(this, $"{VM.Theme_BaseColor}.{VM.Theme_AccentColor}");
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if (await SaveSettingsAsync(CancellationToken.None))
                Application.Current.Dispatcher.Invoke(() => DialogResult = true);
        }

        private void OnResetImageEditorClick(object sender, RoutedEventArgs e)
        {
            VM.Texture_ImageEditorExe = SettingsDataModel.DefaultImageEditorExe;
            VM.Texture_ImageEditorArgs = SettingsDataModel.DefaultImageEditorArgs;
        }

        private void OnResetThemeColorsClick(object sender, RoutedEventArgs e)
        {
            VM.Theme_BaseColor = SettingsDataModel.DefaultThemeBaseColor;
            VM.Theme_AccentColor = SettingsDataModel.DefaultThemeAccentColor;
        }
    }
}
