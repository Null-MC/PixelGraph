using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.IO;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using PixelGraph.Common.Textures;

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

        private async Task LoadPackContentAsync(string root, CancellationToken token)
        {
            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IFileLoader>();

            reader.SetRoot(root);

            await foreach (var item in loader.LoadAsync(token)) {
                if (item is PbrProperties texture) {
                    var textureVM = new TextureVM(texture) {
                        Name = texture.DisplayName,
                    };

                    var albedoFilename = reader.EnumerateTextures(texture, TextureTags.Albedo).FirstOrDefault();

                    if (albedoFilename != null) {
                        var filename = reader.GetFullPath(albedoFilename);
                        textureVM.AlbedoSource = new BitmapImage(new Uri(filename));
                    }

                    vm.TextureList.Add(textureVM);
                }
                else if (item is string localFile) {
                    //
                }
            }
        }

        private async void NewPackButton_OnClick(object sender, RoutedEventArgs e)
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
                InputFormat = EncodingProperties.Raw,
                OutputFormat = EncodingProperties.Lab13,
            };

            // TODO: create file
            vm.PackFilename = dialog.FileName;
            vm.Initialize(pack);

            // TODO: further processing
            var root = Path.GetDirectoryName(vm.PackFilename);
            await LoadPackContentAsync(root, CancellationToken.None);
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
            var root = Path.GetDirectoryName(vm.PackFilename);
            await LoadPackContentAsync(root, CancellationToken.None);
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(provider);
            //...

            settings.ShowDialog();
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
