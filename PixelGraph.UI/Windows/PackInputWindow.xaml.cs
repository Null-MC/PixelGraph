using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Windows
{
    public partial class PackInputWindow
    {
        private readonly ILogger<PackInputWindow> logger;
        private readonly PackInputViewModel viewModel;


        public PackInputWindow(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<PackInputWindow>>();
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new PackInputViewModel(provider) {
                Model = Model,
            };
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        #region Events

        private void OnDataGridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            if (EncodingDataGrid.SelectedValue is not TextureChannelMapping channel) return;

            channel.Clear();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            try {
                await viewModel.SavePackInputAsync();
                Application.Current.Dispatcher.Invoke(() => DialogResult = true);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to save pack input!");
                ShowError($"Failed to save pack input! {error.UnfoldMessageString()}");
            }
        }

        #endregion
    }
}
