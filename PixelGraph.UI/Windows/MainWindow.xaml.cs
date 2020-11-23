using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.ViewModels;
using SixLabors.ImageSharp;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
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
        private const int ThumbnailSize = 64;

        private readonly IServiceProvider provider;
        private readonly MainWindowVM vm;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            if (!DesignerProperties.GetIsInDesignMode(this)) {
                vm = new MainWindowVM();
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
            if (!vm.TryStartBusy()) return;

            try {
                vm.RootDirectory = dialog.SelectedPath;
                vm.Profiles.Clear();

                var reader = provider.GetRequiredService<IInputReader>();
                reader.SetRoot(vm.RootDirectory);

                var writer = provider.GetRequiredService<IOutputWriter>();
                writer.SetRoot(vm.RootDirectory);

                await Task.Factory.StartNew(() =>
                    LoadDirectoryAsync(token), token);
            }
            finally {
                vm.EndBusy();
            }
        }

        private async Task LoadDirectoryAsync(CancellationToken token)
        {
            var packReader = provider.GetRequiredService<IResourcePackReader>();
            var loader = provider.GetRequiredService<IFileLoader>();

            loader.Expand = false;

            var packInput = await packReader.ReadInputAsync("input.yml")
                ?? new ResourcePackInputProperties {
                    Format = TextureEncoding.Format_Raw,
                };

            Application.Current.Dispatcher.Invoke(() => {
                vm.PackInput = packInput;
            });

            foreach (var file in Directory.EnumerateFiles(vm.RootDirectory, "*.pack.yml", SearchOption.TopDirectoryOnly)) {
                var localFile = Path.GetFileName(file);

                var profileItem = new ProfileItem {
                    Name = localFile[..^9],
                    LocalFile = localFile,
                };

                Application.Current.Dispatcher.Invoke(() => {
                    vm.Profiles.Add(profileItem);
                });
            }

            await foreach (var item in loader.LoadAsync(token)) {
                if (!(item is MaterialProperties material)) continue;

                var textureNode = new TextureTreeTexture {
                    Name = material.DisplayName,
                    MaterialFilename = material.LocalFilename,
                    Material = material,
                };

                Application.Current.Dispatcher.Invoke(() => {
                    var parentNode = vm.GetTreeNode(material.LocalPath);
                    parentNode.Nodes.Add(textureNode);
                });
            }
        }

        private void PopulateTextureViewer(TextureTreeTexture textureNode)
        {
            vm.Textures.Clear();

            var texture = textureNode?.Material;
            if (texture == null) return;

            // TODO: wait for texture busy

            vm.LoadedMaterialFilename = textureNode.MaterialFilename;
            vm.LoadedMaterial = texture;

            var reader = provider.GetRequiredService<IInputReader>();

            foreach (var tag in TextureTags.All) {
                foreach (var file in reader.EnumerateTextures(texture, tag)) {
                    if (!reader.FileExists(file)) continue;
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

                    vm.Textures.Add(new TextureSource {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Thumbnail = thumbnailImage,
                        Image = fullImage,
                        Tag = tag,
                    });
                }
            }

            vm.SelectFirstTexture();
        }

        private async Task GenerateNormalAsync(CancellationToken token)
        {
            var material = vm.LoadedMaterial;
            var outputName = TextureTags.Get(material, TextureTags.Normal);

            if (string.IsNullOrWhiteSpace(outputName)) {
                var naming = provider.GetRequiredService<INamingStructure>();
                outputName = naming.Get(TextureTags.Normal, material.Name, "png", material.UseGlobalMatching);
            }

            var path = PathEx.Join(vm.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "A normal texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            if (!vm.TryStartBusy()) return;

            try {
                var context = new MaterialContext {
                    Input = vm.PackInput,
                    //Profile = vm.LoadedProfile,
                    Material = material,
                };

                await Task.Factory.StartNew(async () => {
                    var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
                    using var graph = graphBuilder.BuildInputGraph(context);

                    using var normalImage = await graph.GenerateNormalAsync(token);
                    await normalImage.SaveAsync(fullName, token);
                }, token);

                // TODO: update texture sources
                Application.Current.Dispatcher.Invoke(() => {
                    PopulateTextureViewer(vm.SelectedNode as TextureTreeTexture);
                });
            }
            catch (Exception error) {
                ShowError($"Failed to generate normal texture! {error.Message}");
            }
            finally {
                vm.EndBusy();
            }
        }

        private async Task GenerateOcclusionAsync(CancellationToken token)
        {
            var material = vm.LoadedMaterial;
            var outputName = TextureTags.Get(material, TextureTags.Occlusion);

            if (string.IsNullOrWhiteSpace(outputName)) {
                var naming = provider.GetRequiredService<INamingStructure>();
                outputName = naming.Get(TextureTags.Occlusion, material.Name, "png", material.UseGlobalMatching);
            }

            var path = PathEx.Join(vm.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "An occlusion texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            if (!vm.TryStartBusy()) return;

            try {
                var context = new MaterialContext {
                    Input = vm.PackInput,
                    //Profile = vm.LoadedProfile,
                    Material = material,
                };

                await Task.Factory.StartNew(async () => {
                    var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
                    using var graph = graphBuilder.BuildInputGraph(context);

                    using var occlusionImage = await graph.GenerateOcclusionAsync(token);
                    await occlusionImage.SaveAsync(fullName, token);
                }, token);

                // TODO: update texture sources
                Application.Current.Dispatcher.Invoke(() => {
                    PopulateTextureViewer(vm.SelectedNode as TextureTreeTexture);
                });
            }
            catch (Exception error) {
                ShowError($"Failed to generate occlusion texture! {error.Message}");
            }
            finally {
                vm.EndBusy();
            }
        }

        private async Task SaveMaterialAsync()
        {
            try {
                var writer = provider.GetRequiredService<IMaterialWriter>();
                await writer.WriteAsync(vm.LoadedMaterial, vm.LoadedMaterialFilename);
            }
            catch (Exception error) {
                ShowError($"Failed to save material '{vm.LoadedMaterialFilename}'! {error.Message}");
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!");
            });
        }

        #region Events

        private void OnOpenMenuItemClick(object sender, RoutedEventArgs e)
        {
            SelectRootDirectory(CancellationToken.None);
        }

        private void OnContentEncodingMenuItemClick(object sender, RoutedEventArgs e)
        {
            var window = new PackInputWindow(provider) {
                Owner = this,
                VM = {
                    PackInput = vm.PackInput,
                },
            };

            window.ShowDialog();
        }

        private void OnManageProfilesMenuItemClick(object sender, RoutedEventArgs e)
        {
            var window = new PackProfilesWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    Profiles = vm.Profiles,
                },
            };

            window.ShowDialog();
        }

        private void OnAppSettingsMenuItemClick(object sender, RoutedEventArgs e)
        {
            var window = new SettingsWindow {
                Owner = this,
            };

            window.ShowDialog();
        }

        private void OnPublishMenuItemClick(object sender, RoutedEventArgs e)
        {
            var window = new PublishWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    Profiles = vm.Profiles.ToList(),
                },
            };

            window.ShowDialog();
        }

        private void OnExitMenuItemClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnMaterialChanged(object sender, EventArgs e)
        {
            await SaveMaterialAsync();
        }

        private void OnTextureTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.SelectedNode = e.NewValue as TextureTreeNode;
            PopulateTextureViewer(e.NewValue as TextureTreeTexture);
        }

        private async void OnGenerateNormal(object sender, EventArgs e)
        {
            if (vm.LoadedMaterial == null) return;

            try {
                await GenerateNormalAsync(CancellationToken.None);
            }
            catch (Exception error) {
                ShowError($"Failed to generate normal texture! {error.Message}");
            }
        }

        private async void OnGenerateOcclusion(object sender, EventArgs e)
        {
            if (vm.LoadedMaterial == null) return;

            try {
                await GenerateOcclusionAsync(CancellationToken.None);
            }
            catch (Exception error) {
                ShowError($"Failed to generate occlusion texture! {error.Message}");
            }
        }

        #endregion
    }
}
