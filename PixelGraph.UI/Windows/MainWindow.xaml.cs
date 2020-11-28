using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImageExtensions = PixelGraph.Common.IO.ImageExtensions;

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

            var recent = provider.GetRequiredService<IRecentPathManager>();
            vm.RecentDirectories = recent.List;

            InitializeComponent();
        }

        private async Task SelectRootDirectoryAsync(CancellationToken token)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) != true) return;
            await LoadRootDirectoryAsync(dialog.SelectedPath, token);
        }

        private async Task LoadRootDirectoryAsync(string path, CancellationToken token)
        {
            if (!vm.TryStartBusy()) return;

            var recent = provider.GetRequiredService<IRecentPathManager>();
            var reader = provider.GetRequiredService<IInputReader>();
            //var writer = provider.GetRequiredService<IOutputWriter>();

            try {
                reader.SetRoot(path);
                //writer.SetRoot(path);

                vm.RootDirectory = path;
                vm.TreeRoot.Nodes.Clear();
                vm.Profiles.Clear();

                LoadProfiles();

                await LoadPackInputAsync();

                LoadDirectory(vm.TreeRoot.Nodes, ".");
                vm.TreeRoot.UpdateVisibility(vm);
            }
            finally {
                vm.EndBusy();
            }

            await recent.InsertAsync(path, token);
        }

        private void LoadDirectory(ICollection<TextureTreeNode> collection, string path)
        {
            collection.Clear();

            var reader = provider.GetRequiredService<IInputReader>();
            reader.SetRoot(vm.RootDirectory);

            foreach (var childPath in reader.EnumerateDirectories(path, "*")) {
                var childNode = new TextureTreeDirectory {
                    Name = Path.GetFileName(childPath),
                    Path = childPath,
                };

                LoadDirectory(childNode.Nodes, childPath);
                collection.Add(childNode);
            }

            foreach (var file in reader.EnumerateFiles(path, "*.*")) {
                var fileName = Path.GetFileName(file);

                var childNode = new TextureTreeFile {
                    Name = fileName,
                    Filename = file,
                    Type = GetNodeType(fileName),
                    Icon = GetNodeIcon(fileName),
                };

                collection.Add(childNode);
            }
        }

        private static NodeType GetNodeType(string fileName)
        {
            if (string.Equals("input.yml", fileName, StringComparison.InvariantCultureIgnoreCase))
                return NodeType.PackInput;

            if (fileName.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
                return NodeType.PackProfile;

            if (string.Equals("pbr.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
                || fileName.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
                return NodeType.Material;

            return NodeType.Unknown;
        }

        private static PackIconKind GetNodeIcon(string fileName)
        {
            if (string.Equals("input.yml", fileName, StringComparison.InvariantCultureIgnoreCase))
                return PackIconKind.Palette;

            if (fileName.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
                return PackIconKind.Export;

            if (string.Equals("pbr.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
                || fileName.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
                return PackIconKind.FileChart;

            var ext = Path.GetExtension(fileName);
            if (ImageExtensions.Supports(ext))
                return PackIconKind.Image;

            return PackIconKind.File;
        }

        private async Task LoadPackInputAsync()
        {
            var packReader = provider.GetRequiredService<IResourcePackReader>();

            var packInput = await packReader.ReadInputAsync("input.yml")
                ?? new ResourcePackInputProperties {
                    Format = TextureEncoding.Format_Raw,
                };

            Application.Current.Dispatcher.Invoke(() => vm.PackInput = packInput);
        }

        private void LoadProfiles()
        {
            var reader = provider.GetRequiredService<IInputReader>();

            foreach (var file in reader.EnumerateFiles(".", "*.pack.yml")) {
                var localFile = Path.GetFileName(file);

                var profileItem = new ProfileItem {
                    Name = localFile[..^9],
                    LocalFile = localFile,
                };

                vm.Profiles.Add(profileItem);
            }
        }

        //private async Task LoadDirectoryXAsync(CancellationToken token)
        //{
        //    var packReader = provider.GetRequiredService<IResourcePackReader>();
        //    var loader = provider.GetRequiredService<IFileLoader>();

        //    loader.Expand = false;

        //    await foreach (var item in loader.LoadAsync(token)) {
        //        if (!(item is MaterialProperties material)) continue;

        //        var textureNode = new TextureTreeTexture {
        //            Name = material.DisplayName,
        //            MaterialFilename = material.LocalFilename,
        //            Material = material,
        //        };

        //        Application.Current.Dispatcher.Invoke(() => {
        //            var parentNode = vm.GetTreeNode(material.LocalPath);
        //            parentNode.Nodes.Add(textureNode);
        //        });
        //    }
        //}

        private async Task PopulateTextureViewerAsync(CancellationToken token)
        {
            vm.IsUpdatingSources = true;

            try {
                Application.Current.Dispatcher.Invoke(vm.Textures.Clear);

                var textureNode = vm.SelectedNode as TextureTreeFile;
                if (textureNode?.Type != NodeType.Material) return;

                // TODO: wait for texture busy
                var reader = provider.GetRequiredService<IInputReader>();
                var matReader = provider.GetRequiredService<IMaterialReader>();

                reader.SetRoot(vm.RootDirectory);
                vm.LoadedMaterialFilename = textureNode.Filename;
                vm.LoadedMaterial = await matReader.LoadAsync(textureNode.Filename, token);

                //vm.LoadedMaterialFilename = textureNode.MaterialFilename;
                //vm.LoadedMaterial = textureNode.Material;

                foreach (var tag in TextureTags.All) {
                    foreach (var file in reader.EnumerateTextures(vm.LoadedMaterial, tag)) {
                        if (!reader.FileExists(file)) continue;
                        var fullFile = reader.GetFullPath(file);

                        var thumbnailImage = new BitmapImage();
                        thumbnailImage.BeginInit();
                        thumbnailImage.CacheOption = BitmapCacheOption.OnLoad;
                        thumbnailImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        thumbnailImage.DecodePixelHeight = ThumbnailSize;
                        thumbnailImage.UriSource = new Uri(fullFile);
                        thumbnailImage.EndInit();
                        thumbnailImage.Freeze();

                        var fullImage = new BitmapImage();
                        fullImage.BeginInit();
                        fullImage.CacheOption = BitmapCacheOption.OnLoad;
                        fullImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        fullImage.UriSource = new Uri(fullFile);
                        fullImage.EndInit();
                        fullImage.Freeze();

                        var source = new TextureSource {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Thumbnail = thumbnailImage,
                            Image = fullImage,
                            Tag = tag,
                        };

                        Application.Current.Dispatcher.Invoke(() => vm.Textures.Add(source));
                    }
                }
            }
            finally {
                vm.IsUpdatingSources = false;
                vm.SelectFirstTexture();
            }
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

                await PopulateTextureViewerAsync(token);
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
                await PopulateTextureViewerAsync(token);
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

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var recent = provider.GetRequiredService<IRecentPathManager>();
            await recent.InitializeAsync();
        }

        private async void OnRecentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(RecentList.SelectedItem is string item)) return;
            await LoadRootDirectoryAsync(item, CancellationToken.None);
        }

        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {
            await SelectRootDirectoryAsync(CancellationToken.None);
        }

        private void OnImportFolderClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Select the root folder of a resource pack.",
            };

            if (dialog.ShowDialog(this) != true) return;

            var window = new ImportPackWindow(provider) {
                Owner = this,
                VM = {
                    SourcePath = dialog.SelectedPath,
                },
            };

            window.ShowDialog();
        }

        private void OnImportZipClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog {
                Title = "Import Zip Archive",
                Filter = "Zip Archive|*.zip|All Files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != true) return;

            var window = new ImportPackWindow(provider) {
                Owner = this,
                VM = {
                    SourceFile = dialog.FileName,
                },
            };

            window.ShowDialog();
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

        private void OnProfilesClick(object sender, RoutedEventArgs e)
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

        private void OnSettingsClick(object sender, RoutedEventArgs e)
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

        private async void OnMaterialChanged(object sender, EventArgs e)
        {
            await SaveMaterialAsync();
        }

        private async void OnTextureTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.SelectedNode = e.NewValue as TextureTreeNode;
            await PopulateTextureViewerAsync(CancellationToken.None);
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

        private void OnDocumentationButtonClick(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo {
                FileName = @"https://github.com/null511/PixelGraph/wiki",
                UseShellExecute = true,
            };

            Process.Start(info);
        }

        //private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
        //{
        //    if (!(e.OriginalSource is TreeViewItem item)) return;
        //    if (item.DataContext is TextureTreeDirectory node)
        //        LoadDirectory(node.Nodes, node.Path);
        //}

        #endregion
    }
}
