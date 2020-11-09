using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.IO;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private const string FileFilters = "Property Files|*.properties|All Files|*.*";

        private readonly IServiceProvider provider;
        private readonly MainWindowVM vm;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            vm = provider.GetRequiredService<MainWindowVM>();
            vm.Changed += VM_OnChanged;

            DataContext = vm;
            InitializeComponent();
        }

        private void NewPackButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: close if open?

            var dialog = new SaveFileDialog {
                Title = "Create a new pack.properties file",
                FileName = "pack.properties",
                Filter = FileFilters,
            };

            var result = dialog.ShowDialog(this);
            if (result != true) return;

            var pack = new PackProperties {
                InputFormat = EncodingProperties.Default,
                OutputFormat = EncodingProperties.Default,
            };

            // TODO: create file
            vm.PackFilename = dialog.FileName;
            vm.Initialize(pack);

            // TODO: further processing
        }

        private async void OpenPackButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: close if open?

            var dialog = new OpenFileDialog {
                Title = "Open existing pack.properties file",
                FileName = vm.PackFilename,
                Filter = FileFilters,
                Multiselect = false,
            };

            var result = dialog.ShowDialog(this);
            if (result != true) return;

            var packReader = provider.GetRequiredService<IPackReader>();
            var pack = await packReader.ReadAsync(dialog.FileName);

            vm.PackFilename = dialog.FileName;
            vm.Initialize(pack);

            // TODO: further processing
        }

        private void PublishPackButton_OnClick(object sender, RoutedEventArgs e)
        {
            var publish = new PublishWindow(provider);
            //...

            publish.ShowDialog();
        }

        private async void VM_OnChanged(object sender, EventArgs e)
        {
            var packWriter = provider.GetRequiredService<IPropertyWriter>();
            await using var stream = File.Open(vm.PackFilename, FileMode.Create, FileAccess.Write);
            await packWriter.WriteAsync(stream, vm.Pack);
        }
    }
}
