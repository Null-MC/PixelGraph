using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using PixelGraph.UI.ViewModels;
using SixLabors.ImageSharp;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private const int ThumbnailSize = 64;

        private readonly IServiceProvider provider;
        private readonly MainWindowVM vm;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            if (!DesignerProperties.GetIsInDesignMode(this)) {
                vm = new MainWindowVM();

                vm.Profile.SelectionChanged += OnProfileChanged;
                vm.Profile.DataChanged += OnPackDataChanged;

                //vm.Texture.DataChanged += OnTextureDataChanged;

                DataContext = vm;
            }

            InitializeComponent();
        }

        private async void SelectRootDirectory(CancellationToken token)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) != true) return;

            vm.RootDirectory = dialog.SelectedPath;
            vm.Profile.Profiles.Clear();

            await Task.Factory.StartNew(() => LoadDirectoryAsync(dialog.SelectedPath, token), token);
        }

        private async Task LoadDirectoryAsync(string path, CancellationToken token)
        {
            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IFileLoader>();

            reader.SetRoot(path);
            loader.Expand = false;

            foreach (var file in Directory.EnumerateFiles(vm.RootDirectory, "*.pack.properties", SearchOption.TopDirectoryOnly)) {
                var profile = CreateProfileItem(file);

                Application.Current.Dispatcher.Invoke(() => {
                    vm.Profile.Profiles.Add(profile);
                });
            }

            await foreach (var item in loader.LoadAsync(token)) {
                if (!(item is PbrProperties texture)) continue;

                var textureNode = new TextureTreeTexture {
                    Name = texture.DisplayName,
                    TextureFilename = reader.GetFullPath(texture.FileName),
                    Texture = texture,
                };

                Application.Current.Dispatcher.Invoke(() => {
                    var parentNode = vm.Texture.GetTreeNode(texture.Path);
                    parentNode.Nodes.Add(textureNode);
                });
            }
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

            var profile = CreateProfileItem(dialog.FileName);
            Application.Current.Dispatcher.Invoke(() => {
                vm.Profile.Profiles.Add(profile);
                vm.Profile.Selected = profile;
            });
        }

        private void DeleteSelectedProfile()
        {
            var profile = vm.Profile.Selected;
            if (profile?.Filename == null) return;

            File.Delete(profile.Filename);
            vm.Profile.Profiles.Remove(profile);
        }

        private static ProfileItem CreateProfileItem(string filename)
        {
            var fileName = Path.GetFileName(filename);

            var item = new ProfileItem {
                Name = fileName[..^16],
                Filename = filename,
            };

            return item;
        }

        private async Task<PackProperties> LoadProfileAsync(string filename, CancellationToken token)
        {
            var packReader = provider.GetRequiredService<IPackReader>();
            await using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
            return await packReader.ReadAsync(stream, token: token);
        }

        private async Task SavePropertiesAsync(PropertiesFile pack, string filename, CancellationToken token)
        {
            var writer = provider.GetRequiredService<IPropertyWriter>();
            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            await writer.WriteAsync(stream, pack, token);
        }

        private void PopulateTextureViewer(TextureTreeTexture textureNode)
        {
            vm.Texture.Textures.Clear();

            var texture = textureNode?.Texture;
            if (texture == null) return;

            // TODO: wait for texture busy

            vm.Texture.LoadedFilename = textureNode.TextureFilename;
            vm.Texture.Loaded = texture;

            var reader = provider.GetRequiredService<IInputReader>();

            foreach (var tag in TextureTags.All) {
                foreach (var file in reader.EnumerateTextures(texture, tag)) {
                    var fullFile = reader.GetFullPath(file);

                    var thumbnailImage = new BitmapImage();
                    thumbnailImage.BeginInit();
                    thumbnailImage.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnailImage.DecodePixelHeight = ThumbnailSize;
                    thumbnailImage.UriSource = new Uri(fullFile);
                    thumbnailImage.EndInit();
                    thumbnailImage.Freeze();

                    var fullImage = new BitmapImage();
                    fullImage.BeginInit();
                    fullImage.CacheOption = BitmapCacheOption.OnLoad;
                    fullImage.UriSource = new Uri(fullFile);
                    fullImage.EndInit();
                    thumbnailImage.Freeze();

                    vm.Texture.Textures.Add(new TextureSource {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Thumbnail = thumbnailImage,
                        Image = fullImage,
                        Filename = file,
                        Tag = tag,
                    });
                }
            }

            vm.Texture.SelectFirstTexture();
        }

        private async Task GenerateNormalAsync(PackProperties pack, PbrProperties texture, CancellationToken token)
        {
            var naming = provider.GetRequiredService<INamingStructure>();
            var outputName = naming.GetOutputTextureName(pack, texture, TextureTags.Normal, texture.UseGlobalMatching);

            var path = PathEx.Join(vm.RootDirectory, texture.Path);
            if (!texture.UseGlobalMatching) path = PathEx.Join(path, texture.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "A normal texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            if (!vm.TryStartBusy()) return;
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();

            try {
                await Task.Factory.StartNew(async () => {
                    using var graph = graphBuilder.CreateGraph(pack, texture);
                    using var normalImage = await graph.GenerateNormalAsync(token);

                    await normalImage.SaveAsync(fullName, token);
                }, token);

                // TODO: update texture sources
                Application.Current.Dispatcher.Invoke(() => {
                    PopulateTextureViewer(vm.Texture.SelectedNode as TextureTreeTexture);
                });
            }
            catch (Exception error) {
                ShowError($"Failed to generate normal texture! {error.Message}");
            }
            finally {
                vm.EndBusy();
            }
        }

        private async Task GenerateOcclusionAsync(PackProperties pack, PbrProperties texture, CancellationToken token)
        {
            var naming = provider.GetRequiredService<INamingStructure>();
            var outputName = naming.GetOutputTextureName(pack, texture, TextureTags.Occlusion, texture.UseGlobalMatching);

            var path = PathEx.Join(vm.RootDirectory, texture.Path);
            if (!texture.UseGlobalMatching) path = PathEx.Join(path, texture.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "An occlusion texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            if (!vm.TryStartBusy()) return;
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();

            try {
                await Task.Factory.StartNew(async () => {
                    using var graph = graphBuilder.CreateGraph(pack, texture);
                    using var occlusionImage = await graph.GenerateOcclusionAsync(token);

                    await occlusionImage.SaveAsync(fullName, token);
                }, token);

                // TODO: update texture sources
                Application.Current.Dispatcher.Invoke(() => {
                    PopulateTextureViewer(vm.Texture.SelectedNode as TextureTreeTexture);
                });
            }
            catch (Exception error) {
                ShowError($"Failed to generate occlusion texture! {error.Message}");
            }
            finally {
                vm.EndBusy();
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
            });
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
            var profile = vm.Profile.Selected;
            if (profile == null) return;

            // TODO: Wait for existing save!

            vm.Profile.LoadedFilename = profile.Filename;
            vm.Profile.Loaded = await LoadProfileAsync(profile.Filename, CancellationToken.None);
        }

        private void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {
            SelectRootDirectory(CancellationToken.None);
        }

        private async void OnPackDataChanged(object sender, EventArgs e)
        {
            try {
                await SavePropertiesAsync(vm.Profile.Loaded, vm.Profile.LoadedFilename, CancellationToken.None);
            }
            catch (Exception error) {
                ShowError($"Failed to save pack profile '{vm.Profile.LoadedFilename}'! {error.Message}");
            }
        }

        private async void OnTextureDataChanged(object sender, EventArgs e)
        {
            try {
                await SavePropertiesAsync(vm.Texture.Loaded, vm.Texture.LoadedFilename, CancellationToken.None);
            }
            catch (Exception error) {
                ShowError($"Failed to save texture '{vm.Texture.LoadedFilename}'! {error.Message}");
            }
        }

        private void OnTextureTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.Texture.SelectedNode = e.NewValue as TextureTreeNode;
            PopulateTextureViewer(e.NewValue as TextureTreeTexture);
        }

        private void OnProfileListBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(ProfileListBox, e.GetPosition(ProfileListBox));
            if (r.VisualHit.GetType() != typeof(ListBoxItem)) ProfileListBox.UnselectAll();
        }

        private async void OnGenerateNormal(object sender, EventArgs e)
        {
            if (vm.Texture.Loaded == null) return;

            if (vm.Profile.Loaded == null) {
                MessageBox.Show("A pack profile must be selected first!", "Warning");
                return;
            }

            try {
                await GenerateNormalAsync(vm.Profile.Loaded, vm.Texture.Loaded, CancellationToken.None);
            }
            catch (Exception error) {
                ShowError($"Failed to generate normal texture! {error.Message}");
            }
        }

        private async void OnGenerateOcclusion(object sender, EventArgs e)
        {
            if (vm.Texture.Loaded == null) return;

            if (vm.Profile.Loaded == null) {
                MessageBox.Show("A pack profile must be selected first!", "Warning");
                return;
            }

            try {
                await GenerateOcclusionAsync(vm.Profile.Loaded, vm.Texture.Loaded, CancellationToken.None);
            }
            catch (Exception error) {
                ShowError($"Failed to generate occlusion texture! {error.Message}");
            }
        }

        #endregion
    }
}
