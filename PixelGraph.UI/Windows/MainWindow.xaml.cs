using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.UI.ViewModels;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private const string FileFilters = "Property Files|*.properties|All Files|*.*";

        private readonly IServiceProvider provider;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            DataContext = provider.GetRequiredService<MainWindowVM>();

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

            // TODO: create file
            VM.PackFilename = dialog.FileName;
            VM.Initialize(new PackProperties());

            // TODO: further processing
        }

        private async void OpenPackButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: close if open?

            var dialog = new OpenFileDialog {
                Title = "Open existing pack.properties file",
                FileName = VM.PackFilename,
                Filter = FileFilters,
                Multiselect = false,
            };

            var result = dialog.ShowDialog(this);
            if (result != true) return;

            var packReader = provider.GetRequiredService<IPackReader>();
            var pack = await packReader.ReadAsync(dialog.FileName);

            VM.PackFilename = dialog.FileName;
            VM.Initialize(pack);

            // TODO: further processing
        }

        private void PublishPackButton_OnClick(object sender, RoutedEventArgs e)
        {
            var publish = new PublishWindow(provider);
            //...

            publish.ShowDialog();
        }
    }
}
