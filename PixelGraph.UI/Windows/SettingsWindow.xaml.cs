using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class SettingsWindow
    {
        private readonly ILogger<SettingsWindow> logger;


        public SettingsWindow(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<SettingsWindow>>();

            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);

            Model.Initialize(provider);
        }

        private void OnVMDataChanged(object sender, EventArgs e)
        {
            ThemeManager.Current.ChangeTheme(this, $"{Model.App_ThemeBaseColor}.{Model.App_ThemeAccentColor}");
        }

        private void OnResetImageEditorClick(object sender, RoutedEventArgs e)
        {
            Model.ResetImageEditor();
        }

        private void OnResetAppThemeColorsClick(object sender, RoutedEventArgs e)
        {
            Model.ResetThemeColors();
        }

        private void OnResetPreviewRendererClick(object sender, RoutedEventArgs e)
        {
            Model.ResetPreviewRenderer();
        }

        private void OnResetPreviewParallaxClick(object sender, RoutedEventArgs e)
        {
            Model.ResetPreviewParallax();
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            try {
                await Model.SaveSettingsAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to save app settings!");
                await this.ShowMessageAsync("Error!", $"Failed to save settings! {error.UnfoldMessageString()}");
                return;
            }

            await Dispatcher.BeginInvoke(() => DialogResult = true);
        }

        private void OnTextureExecutableBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog {
                Title = "Select Executable",
                Filter = "Executable|*.exe|All Files|*.*",
                FileName = Model.Texture_ImageEditorExe,
                CheckFileExists = true,
                Multiselect = false,
            };

            if (dialog.ShowDialog(this) != true) return;

            Model.Texture_ImageEditorExe = dialog.FileName;
        }
    }
}
