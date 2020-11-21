using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using System;
using System.Threading.Tasks;
using System.Windows;

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

        private async Task SaveAsync()
        {
            try {
                var packWriter = provider.GetRequiredService<IResourcePackWriter>();
                await packWriter.WriteAsync("input.yml", VM.PackInput);
            }
            catch (Exception error) {
                ShowError($"Failed to save pack input 'input.yml'! {error.Message}");
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
            });
        }

        private async void OnDataChanged(object sender, EventArgs e)
        {
            await SaveAsync();
        }
    }
}
