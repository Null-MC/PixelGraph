using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private readonly IServiceProvider provider;
        private readonly MainWindowVM vm;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            vm = provider.GetRequiredService<MainWindowVM>();
            vm.ProfileChanged += OnProfileChanged;
            vm.DataChanged += VM_OnDataChanged;

            DataContext = vm;
            InitializeComponent();
        }

        private async Task LoadDirectoryAsync(string path, CancellationToken token)
        {
            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IFileLoader>();

            reader.SetRoot(path);
            loader.Expand = false;

            foreach (var file in Directory.EnumerateFiles(vm.RootDirectory, "*.pack.properties", SearchOption.TopDirectoryOnly)) {
                var profile = CreateProfile(file);

                await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    vm.Profiles.Add(profile);
                }));
            }

            await foreach (var item in loader.LoadAsync(token)) {
                if (!(item is PbrProperties texture)) continue;

                var textureNode = new TextureTreeTexture {
                    Name = texture.DisplayName,
                    Texture = texture,
                };

                await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    var parentNode = vm.Texture.GetTreeNode(texture.Path);
                    parentNode.Nodes.Add(textureNode);
                }));
            }
        }

        private async void SelectRootDirectory(CancellationToken token)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) != true) return;

            vm.RootDirectory = dialog.SelectedPath;
            vm.Profiles.Clear();

            await Task.Factory.StartNew(() => LoadDirectoryAsync(dialog.SelectedPath, token), token);
        }

        private async Task CreateNewProfile(CancellationToken token)
        {
            var dialog = new SaveFileDialog {
                Title = "Create a new pack properties file",
                Filter = "Pack Properties|*.pack.properties|All Files|*.*",
                AddExtension = true,
                FileName = "default",
            };

            var result = dialog.ShowDialog(this);
            if (result != true) return;

            var pack = new PackProperties {
                InputFormat = EncodingProperties.Raw,
                OutputFormat = EncodingProperties.Lab13,
            };

            var writer = provider.GetRequiredService<IPropertyWriter>();

            await using (var stream = File.Create(dialog.FileName)) {
                await writer.WriteAsync(stream, pack, token);
            }

            var profile = CreateProfile(dialog.FileName);
            await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                vm.Profiles.Add(profile);
                vm.SelectedProfile = profile;
            }));
        }

        private void DeleteSelectedProfile()
        {
            var profile = vm.SelectedProfile;
            if (profile?.Filename == null) return;

            File.Delete(profile.Filename);
            vm.Profiles.Remove(profile);
        }

        private static ProfileItem CreateProfile(string filename)
        {
            var fileName = Path.GetFileName(filename);

            var item = new ProfileItem {
                Name = fileName[..^16],
                Filename = filename,
            };

            return item;
        }

        private Task<PackProperties> LoadProfileAsync(string filename, CancellationToken token)
        {
            var packReader = provider.GetRequiredService<IPackReader>();
            return packReader.ReadAsync(filename, token: token);
        }

        private async Task SaveProfileAsync(PackProperties pack, string filename, CancellationToken token)
        {
            var packWriter = provider.GetRequiredService<IPropertyWriter>();
            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            await packWriter.WriteAsync(stream, pack, token);
        }

        private void PopulateTextureTree(TextureTreeTexture textureNode)
        {
            var texture = textureNode?.Texture;
            if (texture == null) return;

            var reader = provider.GetRequiredService<IInputReader>();

            foreach (var tag in TextureTags.All) {
                foreach (var file in reader.EnumerateTextures(texture, tag)) {
                    var fullFile = reader.GetFullPath(file);

                    var thumbnailImage = new BitmapImage();
                    thumbnailImage.BeginInit();
                    thumbnailImage.UriSource = new Uri(fullFile);
                    thumbnailImage.DecodePixelHeight = 32;
                    thumbnailImage.EndInit();
                    thumbnailImage.Freeze();

                    var fullImage = new BitmapImage();
                    fullImage.BeginInit();
                    fullImage.UriSource = new Uri(fullFile);
                    fullImage.EndInit();
                    thumbnailImage.Freeze();

                    vm.Texture.Textures.Add(new TextureSource {
                        Name = Path.GetFileNameWithoutExtension(file),
                        //Filename = file,
                        Thumbnail = thumbnailImage,
                        Image = fullImage,
                        Tag = tag,
                    });
                }
            }

            var albedo = vm.Texture.Textures.FirstOrDefault(x => TextureTags.Is(x.Tag, TextureTags.Albedo));
            vm.Texture.Selected = albedo ?? vm.Texture.Textures.FirstOrDefault();
            
            if (albedo != null) {
                var albedoBrush = new ImageBrush(vm.Texture.Selected.Image) {
                    ViewportUnits = BrushMappingMode.Absolute,
                    ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                };
                albedoBrush.Freeze();

                var albedoMaterial = new DiffuseMaterial(albedoBrush);
                albedoMaterial.Freeze();

                vm.Texture.AlbedoMaterial = albedoMaterial;
            }
            else {
                vm.Texture.AlbedoMaterial = null;
            }
        }

        #region Events

        private async void OnNewProfileClick(object sender, RoutedEventArgs e)
        {
            await CreateNewProfile(CancellationToken.None);
        }

        private void OnDuplicateProfileClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
        {
            DeleteSelectedProfile();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(provider) {
                Owner = this,
            };
            //...

            settings.ShowDialog();
        }

        private void OnPublishClick(object sender, RoutedEventArgs e)
        {
            var settings = new PublishWindow(provider) {
                Owner = this,
            };
            //...

            settings.ShowDialog();
        }

        private async void OnProfileChanged(object sender, EventArgs e)
        {
            var profile = vm.SelectedProfile;
            if (profile == null) return;

            // TODO: Wait for existing save!

            vm.LoadedPackFilename = profile.Filename;
            vm.LoadedPack = await LoadProfileAsync(profile.Filename, CancellationToken.None);
        }

        private void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {
            SelectRootDirectory(CancellationToken.None);
        }

        private async void VM_OnDataChanged(object sender, EventArgs e)
        {
            await SaveProfileAsync(vm.LoadedPack, vm.LoadedPackFilename, CancellationToken.None);
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.Texture.Textures.Clear();

            var textureNode = e.NewValue as TextureTreeTexture;
            PopulateTextureTree(textureNode);
        }

        private void OnProfileListBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(ProfileListBox, e.GetPosition(ProfileListBox));
            if (r.VisualHit.GetType() != typeof(ListBoxItem)) ProfileListBox.UnselectAll();
        }

        #endregion
    }
}
