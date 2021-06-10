using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
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
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new SettingsViewModel(provider) {
                Model = Model,
            };

            viewModel.LoadData();

            //Model.DataChanged += OnVMDataChanged;
        }

        private void OnVMDataChanged(object sender, EventArgs e)
        {
            ThemeManager.Current.ChangeTheme(this, $"{Model.Theme_BaseColor}.{Model.Theme_AccentColor}");
        }

        private void OnResetImageEditorClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetImageEditor();
        }

        private void OnResetThemeColorsClick(object sender, RoutedEventArgs e)
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
    }
}
