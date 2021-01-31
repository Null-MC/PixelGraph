using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private readonly IServiceProvider provider;
        //private readonly IAppSettings settings;
        private readonly ILogger logger;

        private readonly object previewLock;
        private ITexturePreviewBuilder previewBuilder;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            //settings = provider.GetRequiredService<IAppSettings>();
            logger = provider.GetRequiredService<ILogger<MainWindow>>();

            previewLock = new object();

            InitializeComponent();

            var recent = provider.GetRequiredService<IRecentPathManager>();
            vm.RecentDirectories = recent.List;

            vm.SelectedTagChanged += OnSelectedTagChanged;
        }

        private async Task SelectRootDirectoryAsync(CancellationToken token)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) == true)
                await SetRootDirectoryAsync(dialog.SelectedPath, token);
        }

        private async Task SetRootDirectoryAsync(string path, CancellationToken token)
        {
            vm.RootDirectory = path;
            await LoadRootDirectoryAsync();

            var recent = provider.GetRequiredService<IRecentPathManager>();
            await recent.InsertAsync(path, token);
        }

        private async Task LoadRootDirectoryAsync()
        {
            if (!vm.TryStartBusy()) return;

            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IFileLoader>();
            var treeReader = provider.GetRequiredService<IContentTreeReader>();

            try {
                reader.SetRoot(vm.RootDirectory);
                //settings.AutoMaterial;

                try {
                    await LoadPackInputAsync();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load pack input definitions!");
                    ShowError("Failed to load pack input definitions!");
                }

                loader.EnableAutoMaterial = vm.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;
                vm.Profiles.Clear();

                try {
                    LoadProfiles();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load pack profile definitions!");
                    ShowError("Failed to load pack profile definitions!");
                }

                vm.TreeRoot = treeReader.GetRootNode();
                vm.TreeRoot.UpdateVisibility(vm);
                vm.SelectedProfile = vm.Profiles.FirstOrDefault();
            }
            finally {
                vm.EndBusy();
            }
        }

        private void ReloadContent()
        {
            if (!vm.TryStartBusy()) return;

            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IFileLoader>();
            var treeReader = provider.GetRequiredService<IContentTreeReader>();

            try {
                reader.SetRoot(vm.RootDirectory);
                //loader.EnableAutoMaterial = settings.AutoMaterial;
                loader.EnableAutoMaterial = vm.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;

                vm.TreeRoot = treeReader.GetRootNode();
                vm.TreeRoot.UpdateVisibility(vm);
            }
            finally {
                vm.EndBusy();
            }
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

        private async Task PopulateTextureViewerAsync(CancellationToken token)
        {
            if (vm.SelectedNode is ContentTreeFile texFile && texFile.Type == ContentNodeType.Texture) {
                var fullFile = PathEx.Join(vm.RootDirectory, texFile.Filename);

                var texImage = new BitmapImage();
                texImage.BeginInit();
                texImage.CacheOption = BitmapCacheOption.OnLoad;
                texImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                texImage.UriSource = new Uri(fullFile);
                texImage.EndInit();
                texImage.Freeze();

                vm.LoadedTexture = texImage;
                vm.LoadedMaterial = null;
                return;
            }

            vm.LoadedTexture = null;

            bool isMat;
            string matFile;
            if (vm.SelectedNode is ContentTreeMaterialDirectory matFolder) {
                isMat = true;
                matFile = matFolder.MaterialFilename;
            }
            else {
                var fileNode = vm.SelectedNode as ContentTreeFile;
                isMat = fileNode?.Type == ContentNodeType.Material;
                matFile = fileNode?.Filename;
            }

            if (!isMat) {
                vm.LoadedMaterial = null;
                return;
            }

            // TODO: wait for texture busy
            var reader = provider.GetRequiredService<IInputReader>();
            var matReader = provider.GetRequiredService<IMaterialReader>();

            reader.SetRoot(vm.RootDirectory);
            vm.LoadedMaterialFilename = matFile;

            try {
                vm.LoadedMaterial = await matReader.LoadAsync(matFile, token);
            }
            catch (Exception error) {
                vm.LoadedMaterial = null;
                logger.LogError(error, "Failed to load material properties!");
                ShowError("Failed to load material properties!");
            }

            var enableAutoMaterial = vm.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;

            if (vm.LoadedMaterial == null && enableAutoMaterial) {
                var localPath = Path.GetDirectoryName(matFile);

                vm.LoadedMaterial = new MaterialProperties {
                    LocalFilename = matFile,
                    LocalPath = Path.GetDirectoryName(localPath),
                    Name = Path.GetFileName(localPath),
                };
            }

            await UpdatePreviewAsync(true);
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
                    var inputFormat = TextureEncoding.GetFactory(context.Input.Format);
                    var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                    inputEncoding.Merge(context.Input);
                    inputEncoding.Merge(context.Material);

                    using var graph = provider.GetRequiredService<ITextureGraph>();
                    graph.InputEncoding.AddRange(inputEncoding.GetMapped());
                    graph.OutputEncoding.AddRange(inputEncoding.GetMapped());
                    graph.Context = context;

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
                    var inputFormat = TextureEncoding.GetFactory(context.Input.Format);
                    var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                    inputEncoding.Merge(context.Input);
                    inputEncoding.Merge(context.Material);

                    using var graph = provider.GetRequiredService<ITextureGraph>();
                    graph.InputEncoding.AddRange(inputEncoding.GetMapped());
                    graph.OutputEncoding.AddRange(inputEncoding.GetMapped());
                    graph.Context = context;

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
            var writer = provider.GetRequiredService<IOutputWriter>();
            var matWriter = provider.GetRequiredService<IMaterialWriter>();

            try {
                writer.SetRoot(vm.RootDirectory);
                await matWriter.WriteAsync(vm.LoadedMaterial, vm.LoadedMaterialFilename);
            }
            catch (Exception error) {
                ShowError($"Failed to save material '{vm.LoadedMaterialFilename}'! {error.Message}");
            }
        }

        private async Task ImportPackAsync(string source, bool isArchive)
        {
            using var window = new ImportPackWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    PackInput = vm.PackInput,
                    ImportSource = source,
                    IsArchive = isArchive,
                },
            };

            if (window.ShowDialog() != null)
                await LoadRootDirectoryAsync();
        }

        private async Task<MaterialProperties> ImportTextureAsync(string filename, CancellationToken token)
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileInput();
            scopeBuilder.AddFileOutput();
            //...

            await using var scope = scopeBuilder.Build();

            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var matWriter = scope.GetRequiredService<IMaterialWriter>();

            reader.SetRoot(vm.RootDirectory);
            writer.SetRoot(vm.RootDirectory);

            var itemName = Path.GetFileNameWithoutExtension(filename);
            var localPath = Path.GetDirectoryName(filename);
            var matFile = PathEx.Join(localPath, itemName, "pbr.yml");

            var material = new MaterialProperties {
                LocalFilename = matFile,
                LocalPath = localPath,
                //...
            };

            await matWriter.WriteAsync(material, matFile);

            var ext = Path.GetExtension(filename);
            var destFile = PathEx.Join(localPath, itemName, $"albedo{ext}");
            await using (var sourceStream = reader.Open(filename)) {
                await using var destStream = writer.Open(destFile);
                await sourceStream.CopyToAsync(destStream, token);
            }

            writer.Delete(filename);
            return material;
        }

        private string GetArchiveFilename()
        {
            var saveFileDialog = new VistaSaveFileDialog {
                Title = "Save published archive",
                Filter = "ZIP Archive|*.zip|All Files|*.*",
                FileName = $"{vm.SelectedProfile?.Name}.zip",
                AddExtension = true,
            };

            return saveFileDialog.ShowDialog() == true
                ? saveFileDialog.FileName : null;
        }

        private static string GetDirectoryName()
        {
            var folderDialog = new VistaFolderBrowserDialog {
                Description = "Destination for published resource pack content.",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
            };

            return folderDialog.ShowDialog() == true
                ? folderDialog.SelectedPath : null;
        }

        private void OpenDocumentation()
        {
            var info = new ProcessStartInfo {
                FileName = @"https://github.com/null511/PixelGraph/wiki",
                UseShellExecute = true,
            };

            Process.Start(info);
        }

        private async Task UpdatePreviewAsync(bool clear)
        {
            ITexturePreviewBuilder p;

            lock (previewLock) {
                previewBuilder?.Cancel();

                p = previewBuilder = provider.GetRequiredService<ITexturePreviewBuilder>();
            }

            var hasSelectedTexture = vm.SelectedTag != null;

            await Application.Current.Dispatcher.BeginInvoke(() => {
                if (clear) vm.LoadedTexture = null;
                vm.IsPreviewLoading = hasSelectedTexture;
            });

            if (!hasSelectedTexture) return;

            p.Input = vm.PackInput;
            p.Material = vm.LoadedMaterial;

            ImageSource image = null;
            try {
                image = await Task.Run(() => p.BuildAsync(vm.SelectedTag), p.Token);
            }
            catch (TaskCanceledException) {}
            catch (Exception error) {
                logger.LogError(error, "Failed to create preview image!");
                // TODO: Set error image instead of message box
                ShowError($"Failed to create preview! {error.Message}");
            }
            finally {
                lock (previewLock) {
                    p.Dispose();
                    if (previewBuilder == p) previewBuilder = null;
                }

                await Application.Current.Dispatcher.BeginInvoke(() => {
                    vm.LoadedTexture = image;
                    vm.IsPreviewLoading = false;
                });
            }
        }

        private async Task ShowImportFolderAsync()
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Select the root folder of a resource pack.",
            };

            if (dialog.ShowDialog(this) != true) return;
            await ImportPackAsync(dialog.SelectedPath, false);
        }

        private async Task ShowImportArchiveAsync()
        {
            var dialog = new VistaOpenFileDialog {
                Title = "Import Zip Archive",
                Filter = "Zip Archive|*.zip|All Files|*.*",
                CheckFileExists = true,
            };

            if (dialog.ShowDialog(this) != true) return;
            await ImportPackAsync(dialog.FileName, true);
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

            try {
                await SetRootDirectoryAsync(item, CancellationToken.None);
            }
            catch (DirectoryNotFoundException) {
                Application.Current.Dispatcher.Invoke(() => {
                    vm.RootDirectory = null;
                    MessageBox.Show(this, "The selected resource pack directory could not be found!", "Error!");
                });

                var recent = provider.GetRequiredService<IRecentPathManager>();
                await recent.RemoveAsync(item);
            }
        }

        private async void OnNewProjectClick(object sender, RoutedEventArgs e)
        {
            var window = new NewProjectWindow(provider) {
                Owner = this,
            };

            if (window.ShowDialog() != true) return;

            vm.CloseProject();

            await SetRootDirectoryAsync(window.VM.Location, CancellationToken.None);

            if (window.VM.EnablePackImport) {
                if (window.VM.ImportFromDirectory) {
                    await ShowImportFolderAsync();
                }
                else if (window.VM.ImportFromArchive) {
                    await ShowImportArchiveAsync();
                }
            }
        }

        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {
            await SelectRootDirectoryAsync(CancellationToken.None);
        }

        private async void OnImportFolderClick(object sender, RoutedEventArgs e)
        {
            await ShowImportFolderAsync();
        }

        private void OnCloseProjectClick(object sender, RoutedEventArgs e)
        {
            vm.CloseProject();
        }

        private async void OnImportZipClick(object sender, RoutedEventArgs e)
        {
            await ShowImportArchiveAsync();
        }

        private void OnInputEncodingClick(object sender, RoutedEventArgs e)
        {
            var window = new PackInputWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    PackInput = (ResourcePackInputProperties)vm.PackInput.Clone(),
                },
            };

            if (window.ShowDialog() != true) return;
            vm.PackInput = window.VM.PackInput;
        }

        private void OnProfilesClick(object sender, RoutedEventArgs e)
        {
            var window = new PackProfilesWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    Profiles = vm.Profiles,
                    SelectedProfileItem = vm.SelectedProfile,
                },
            };

            window.ShowDialog();

            vm.SelectedProfile = window.VM.SelectedProfileItem;
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
            if (vm.SelectedProfile == null) return;

            var destination = vm.PublishArchive
                ? GetArchiveFilename() : GetDirectoryName();

            if (destination == null) return;

            using var window = new PublishWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    Destination = destination,
                    Profile = vm.SelectedProfile,
                    Archive = vm.PublishArchive,
                    Clean = vm.PublishClean,
                },
            };

            window.ShowDialog();
        }

        private async void OnMaterialChanged(object sender, EventArgs e)
        {
            await SaveMaterialAsync();

            await UpdatePreviewAsync(false);
        }

        private async void OnTextureTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.SelectedNode = e.NewValue as ContentTreeNode;

            try {
                await PopulateTextureViewerAsync(CancellationToken.None);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to populate texture viewer!");
                //ShowError("Failed to populate texture viewer!");
            }
        }

        private async void OnGenerateNormal(object sender, EventArgs e)
        {
            if (vm.LoadedMaterial == null) return;

            try {
                await GenerateNormalAsync(CancellationToken.None);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to generate normal texture!");
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
                logger.LogError(error, "Failed to generate occlusion texture!");
                ShowError($"Failed to generate occlusion texture! {error.Message}");
            }
        }

        private void OnDocumentationButtonClick(object sender, RoutedEventArgs e)
        {
            OpenDocumentation();
        }

        private async void OnImportMaterialClick(object sender, RoutedEventArgs e)
        {
            if (!(vm.SelectedNode is ContentTreeFile fileNode) || fileNode.Type != ContentNodeType.Texture) return;

            var material = await Task.Run(() => ImportTextureAsync(fileNode.Filename, CancellationToken.None));

            var parent = fileNode.Parent;
            var contentReader = provider.GetRequiredService<IContentTreeReader>();

            if (parent == null) {
                // refresh root
                await LoadRootDirectoryAsync();
            }
            else {
                //parent.Nodes.Clear();
                var newParent = contentReader.GetPathNode(parent, parent.LocalPath);
                var selected = newParent.Nodes.FirstOrDefault(n => {
                    if (!(n is ContentTreeMaterialDirectory materialNode)) return false;
                    return string.Equals(materialNode.MaterialFilename, material.LocalFilename);
                });

                await Application.Current.Dispatcher.BeginInvoke(() => {
                    parent.Nodes = newParent.Nodes;
                    vm.SelectedNode = selected;
                });
            }
        }

        private void OnContentTreePreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            (source as TreeViewItem)?.Focus();
        }

        private void OnContentRefreshClick(object sender, RoutedEventArgs e)
        {
            var reselectPath = vm.SelectedNode?.LocalPath;

            ReloadContent();

            if (reselectPath != null) {
                // TODO
            }
        }

        private async void OnSelectedTagChanged(object sender, EventArgs e)
        {
            await UpdatePreviewAsync(true);
        }

        private void OnPreviewCancelClick(object sender, RoutedEventArgs e)
        {
            lock (previewLock) {
                previewBuilder?.Cancel();
            }
        }

        #endregion
    }
}
