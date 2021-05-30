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

            VM.Texture_ImageEditor = appSettings.Data.TextureEditCommand;
            VM.Theme_BaseColor = appSettings.Data.AppThemeBase;
            VM.Theme_AccentColor = appSettings.Data.AppThemeAccent;

            VM.DataChanged += OnVMDataChanged;
        }

        private async Task<bool> SaveSettingsAsync(CancellationToken token)
        {
            try {
                appSettings.Data.TextureEditCommand = VM.Texture_ImageEditor;
                appSettings.Data.AppThemeBase = VM.Theme_BaseColor;
                appSettings.Data.AppThemeAccent = VM.Theme_AccentColor;

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
    }
}
