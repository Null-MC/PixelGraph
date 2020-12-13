using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PixelGraph.UI.Windows
{
    public partial class PackInputWindow2
    {
        private readonly IServiceProvider provider;


        public PackInputWindow2(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        private async Task SavePackInputAsync()
        {
            var writer = provider.GetRequiredService<IOutputWriter>();
            writer.SetRoot(VM.RootDirectory);

            var packWriter = provider.GetRequiredService<IResourcePackWriter>();
            await packWriter.WriteAsync("input.yml", VM.PackInput);
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
            });
        }

        private void OnTextureResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is Button button)) return;
            var cell = button.FindParent<DataGridCell>();
            var mapping = (InputChannelMapping) cell?.DataContext;
            mapping?.Clear();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            try {
                await SavePackInputAsync();

                Application.Current.Dispatcher.Invoke(() => DialogResult = true);
            }
            catch (Exception error) {
                ShowError($"Failed to save pack input! {error.Message}");
            }
        }
    }
}
