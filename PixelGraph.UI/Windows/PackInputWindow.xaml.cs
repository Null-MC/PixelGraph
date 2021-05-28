using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.UI.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Windows
{
    public partial class PackInputWindow
    {
        private readonly IServiceProvider provider;


        public PackInputWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        private async Task<bool> SavePackInputAsync()
        {
            try {
                var writer = provider.GetRequiredService<IOutputWriter>();
                writer.SetRoot(VM.RootDirectory);

                var packWriter = provider.GetRequiredService<IResourcePackWriter>();
                await packWriter.WriteAsync("input.yml", VM.PackInput);

                return true;
            }
            catch (Exception error) {
                ShowError($"Failed to save pack input! {error.Message}");
                return false;
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
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
            if (await SavePackInputAsync())
                Application.Current.Dispatcher.Invoke(() => DialogResult = true);
        }

        #endregion
    }
}
