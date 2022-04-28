using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.Threading;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class SettingsWindow
    {
        private readonly SettingsViewModel viewModel;


        public SettingsWindow(IServiceProvider provider)
        {
            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);

            viewModel = new SettingsViewModel(provider) {
                Model = Model,
            };

            viewModel.LoadData();
        }

        private void OnVMDataChanged(object sender, EventArgs e)
        {
            ThemeManager.Current.ChangeTheme(this, $"{Model.App_ThemeBaseColor}.{Model.App_ThemeAccentColor}");
        }

        private void OnResetImageEditorClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetImageEditor();
        }

        private void OnResetAppThemeColorsClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetThemeColors();
        }

        private void OnResetRenderPreviewClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetRenderPreview();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if (await viewModel.SaveSettingsAsync(CancellationToken.None))
                Application.Current.Dispatcher.Invoke(() => DialogResult = true);
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
