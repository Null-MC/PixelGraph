using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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
            loader.Expand = false;

            TextureTreeNode GetNode(string path) {
                var parts = path.Split('/', '\\');
                var parent = vm.Texture.TreeRoot;

                foreach (var part in parts) {
                    var node = parent.Nodes.FirstOrDefault(x => string.Equals(x.Name, part, StringComparison.InvariantCultureIgnoreCase));

                    if (node == null) {
                        node = new TextureTreeDirectory {
                            Name = part,
                        };

                        parent.Nodes.Add(node);
                    }

                    parent = node;
                }

                return parent;
            }

            await foreach (var item in loader.LoadAsync(token)) {
                if (item is PbrProperties texture) {
                    var parentNode = GetNode(texture.Path);

                    var textureNode = new TextureTreeTexture {
                        Name = texture.DisplayName,
                        Texture = texture,
                    };

                    parentNode.Nodes.Add(textureNode);
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

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var textureNode = e.NewValue as TextureTreeTexture;

            var texture = textureNode?.Texture;
            if (texture == null) return;

            var reader = provider.GetRequiredService<IInputReader>();

            vm.Texture.Textures.Clear();
            foreach (var tag in TextureTags.All) {
                foreach (var file in reader.EnumerateTextures(texture, tag)) {
                    var fullFile = reader.GetFullPath(file);

                    var thumbnailImage = new BitmapImage();
                    thumbnailImage.BeginInit();
                    thumbnailImage.UriSource = new Uri(fullFile);
                    thumbnailImage.DecodePixelHeight = 64;
                    thumbnailImage.EndInit();

                    var fullImage = new BitmapImage();
                    fullImage.BeginInit();
                    fullImage.UriSource = new Uri(fullFile);
                    fullImage.EndInit();

                    vm.Texture.Textures.Add(new TextureSource {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Filename = file,
                        Thumbnail = thumbnailImage,
                        Image = fullImage,
                        Tag = tag,
                    });
                }
            }

            vm.Texture.Selected = vm.Texture.Textures
                .FirstOrDefault(x => TextureTags.Is(x.Tag, TextureTags.Albedo))
                ?? vm.Texture.Textures.FirstOrDefault();
        }
    }
}
