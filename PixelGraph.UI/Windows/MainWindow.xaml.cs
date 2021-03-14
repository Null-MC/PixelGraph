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
using PixelGraph.UI.ViewData;
using PixelGraph.UI.ViewModels;
using SixLabors.ImageSharp;
using System;
using System.Collections.ObjectModel;
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
        private readonly ILogger logger;

        private readonly object previewLock;
        private ITexturePreviewBuilder previewBuilder;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<MainWindow>>();

            previewLock = new object();

            InitializeComponent();

            var recent = provider.GetRequiredService<IRecentPathManager>();
            vm.RecentDirectories = recent.List;

            vm.SelectedTagChanged += OnSelectedTagChanged;
            vm.SelectedProfileChanged += OnSelectedProfileChanged;

            vm.SelectedLocation = ManualLocation;
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

                try {
                    await LoadPackInputAsync();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load pack input definitions!");
                    ShowError($"Failed to load pack input definitions! {error.UnfoldMessageString()}");
                }

                loader.EnableAutoMaterial = vm.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;
                vm.PublishProfiles.Clear();

                try {
                    LoadProfiles();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load pack profile definitions!");
                    ShowError($"Failed to load pack profile definitions! {error.UnfoldMessageString()}");
                }

                vm.TreeRoot = new ContentTreeDirectory(null) {
                    LocalPath = null,
                };
                treeReader.Update(vm.TreeRoot);

                vm.TreeRoot.UpdateVisibility(vm);
                vm.SelectedProfile = vm.PublishProfiles.FirstOrDefault();
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

                //vm.TreeRoot = treeReader.GetRootNode();
                vm.TreeRoot.UpdateVisibility(vm);
                treeReader.Update(vm.TreeRoot);
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

                vm.PublishProfiles.Add(profileItem);
            }
        }

        private async Task LoadPublishLocationsAsync(CancellationToken token = default)
        {
            var locationMgr = provider.GetRequiredService<IPublishLocationManager>();
            var locations = await locationMgr.LoadAsync(token);
            if (locations == null) return;

            var list = locations.Select(x => new LocationViewModel(x)).ToList();
            Application.Current.Dispatcher.Invoke(() => vm.PublishLocations = list);
        }

        private async Task PopulateTextureViewerAsync(CancellationToken token)
        {
            if (vm.SelectedNode is ContentTreeFile {Type: ContentNodeType.Texture} texFile) {
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
                vm.IsPreviewLoading = false;
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
                ShowError($"Failed to load material properties! {error.UnfoldMessageString()}");
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
                var inputFormat = TextureEncoding.GetFactory(vm.PackInput.Format);
                var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                inputEncoding.Merge(vm.PackInput);
                inputEncoding.Merge(material);

                await Task.Factory.StartNew(async () => {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var graph = scope.ServiceProvider.GetRequiredService<ITextureNormalGraph>();

                    context.Input = vm.PackInput;
                    //context.Profile = null;
                    context.Material = material;
                    context.InputEncoding = inputEncoding.GetMapped().ToList();
                    context.OutputEncoding = inputEncoding.GetMapped().ToList();

                    using var normalImage = await graph.GenerateAsync(token);
                    await normalImage.SaveAsync(fullName, token);
                }, token);

                await PopulateTextureViewerAsync(token);
            }
            catch (Exception error) {
                ShowError($"Failed to generate normal texture! {error.UnfoldMessageString()}");
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
                var inputFormat = TextureEncoding.GetFactory(vm.PackInput.Format);
                var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                inputEncoding.Merge(vm.PackInput);
                inputEncoding.Merge(material);

                await Task.Factory.StartNew(async () => {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var graph = scope.ServiceProvider.GetRequiredService<ITextureOcclusionGraph>();

                    context.Input = vm.PackInput;
                    //context.Profile = null;
                    context.Material = material;
                    context.InputEncoding = inputEncoding.GetMapped().ToList();
                    context.OutputEncoding = inputEncoding.GetMapped().ToList();

                    using var occlusionImage = await graph.GenerateAsync(token);
                    await occlusionImage.SaveAsync(fullName, token);
                }, token);

                // TODO: update texture sources
                await PopulateTextureViewerAsync(token);
            }
            catch (Exception error) {
                ShowError($"Failed to generate occlusion texture! {error.UnfoldMessageString()}");
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
                ShowError($"Failed to save material '{vm.LoadedMaterialFilename}'! {error.UnfoldMessageString()}");
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

            var result = saveFileDialog.ShowDialog();
            if (result != true) return null;

            var filename = saveFileDialog.FileName;
            if (saveFileDialog.FilterIndex == 1 && !filename.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                filename += ".zip";

            return filename;
        }

        //private static string GetDirectoryName()
        //{
        //    var folderDialog = new VistaFolderBrowserDialog {
        //        Description = "Destination for published resource pack content.",
        //        UseDescriptionForTitle = true,
        //        ShowNewFolderButton = true,
        //    };

        //    return folderDialog.ShowDialog() == true
        //        ? folderDialog.SelectedPath : null;
        //}

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

            if (!vm.HasLoadedMaterial || !hasSelectedTexture) return;

            p.Input = vm.PackInput;
            p.Material = vm.LoadedMaterial;

            if (vm.SelectedProfile != null) {
                var reader = provider.GetRequiredService<IResourcePackReader>();

                try {
                    p.Profile = await reader.ReadProfileAsync(vm.SelectedProfile.LocalFile);
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to load profile!");
                    // TODO: display warning
                }
            }

            ImageSource image = null;
            try {
                image = await Task.Run(() => p.BuildAsync(vm.SelectedTag), p.Token);
            }
            catch (OperationCanceledException) {}
            catch (HeightSourceEmptyException) {}
            catch (Exception error) {
                logger.LogError(error, "Failed to create preview image!");
                // TODO: Set error image instead of message box
                ShowError($"Failed to create preview! {error.UnfoldMessageString()}");
            }
            finally {
                if (!p.Token.IsCancellationRequested) {
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
            try {
                await LoadPublishLocationsAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load publishing locations!");
                ShowError($"Failed to load publishing locations! {error.UnfoldMessageString()}");
            }

            try {
                var recentMgr = provider.GetRequiredService<IRecentPathManager>();
                await recentMgr.InitializeAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load recent projects list!");
                ShowError($"Failed to load recent projects list! {error.UnfoldMessageString()}");
            }

            try {
                var settings = provider.GetRequiredService<IAppSettings>();
                await settings.LoadAsync();

                if (settings.Data.SelectedPublishLocation != null) {
                    var location = vm.PublishLocations.FirstOrDefault(x => string.Equals(x.DisplayName, settings.Data.SelectedPublishLocation, StringComparison.InvariantCultureIgnoreCase));
                    if (location != null) vm.SelectedLocation = location;
                }
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load application settings!");
                ShowError($"Failed to load application settings! {error.UnfoldMessageString()}");
            }
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
                    Profiles = vm.PublishProfiles,
                    SelectedProfileItem = vm.SelectedProfile,
                },
            };

            window.ShowDialog();

            vm.SelectedProfile = window.VM.SelectedProfileItem;
        }

        private void OnLocationsClick(object sender, RoutedEventArgs e)
        {
            var window = new PublishLocationsWindow(provider) {
                Owner = this,
                VM = {
                    Locations = new ObservableCollection<LocationViewModel>(vm.PublishLocations),
                },
            };

            if (vm.SelectedLocation != null && !vm.SelectedLocation.IsManualSelect)
                window.VM.SelectedLocationItem = vm.SelectedLocation;

            var result = window.ShowDialog();
            if (result != true) return;

            vm.PublishLocations = window.VM.Locations.ToList();
            vm.SelectedLocation = window.VM.SelectedLocationItem;
        }

        //private void OnSettingsClick(object sender, RoutedEventArgs e)
        //{
        //    var window = new SettingsWindow {
        //        Owner = this,
        //    };

        //    window.ShowDialog();
        //}

        private void OnPublishMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedProfile == null) return;

            using var window = new PublishWindow(provider) {
                Owner = this,
                VM = {
                    RootDirectory = vm.RootDirectory,
                    Profile = vm.SelectedProfile,

                    // TODO: Attach this to something on the UI
                    Clean = false,
                },
            };

            if (vm.SelectedLocation != null && !vm.SelectedLocation.IsManualSelect) {
                var name = vm.SelectedProfile.Name ?? Path.GetFileNameWithoutExtension(vm.SelectedProfile.LocalFile);
                if (name == null) throw new ApplicationException("Unable to determine profile name!");

                window.VM.Destination = Path.Combine(vm.SelectedLocation.Path, name);
                window.VM.Archive = vm.SelectedLocation.Archive;
                //window.VM.Clean = vm.PublishClean;
            }
            else {
                //window.VM.Clean = vm.PublishClean;
                //window.VM.Archive = vm.PublishArchive;
                //window.VM.Destination = vm.PublishArchive
                //    ? GetArchiveFilename() : GetDirectoryName();

                // TODO: Add option to publish folder/archive when manually selecting destination
                window.VM.Destination = GetArchiveFilename();
                window.VM.Archive = true;

                if (window.VM.Destination == null) return;
            }

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
                ShowError($"Failed to generate normal texture! {error.UnfoldMessageString()}");
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
                ShowError($"Failed to generate occlusion texture! {error.UnfoldMessageString()}");
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

            var contentReader = provider.GetRequiredService<IContentTreeReader>();
            var parent = fileNode.Parent;

            if (parent == null) {
                // refresh root
                await LoadRootDirectoryAsync();
            }
            else {
                //parent.Nodes.Clear();

                await Application.Current.Dispatcher.BeginInvoke(() => {
                    contentReader.Update(parent);

                    var selected = parent.Nodes.FirstOrDefault(n => {
                        if (!(n is ContentTreeMaterialDirectory materialNode)) return false;
                        return string.Equals(materialNode.MaterialFilename, material.LocalFilename);
                    });

                    if (selected != null) vm.SelectedNode = selected;
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
                
                //vm.SelectedNode = vm.TreeRoot.FindNode(n => string.Equals(n.LocalPath, reselectPath, StringComparison.InvariantCultureIgnoreCase));
                // TODO
            }
        }

        private async void OnSelectedTagChanged(object sender, EventArgs e)
        {
            await UpdatePreviewAsync(true);
        }

        private async void OnSelectedProfileChanged(object sender, EventArgs e)
        {
            await UpdatePreviewAsync(true);
        }

        private void OnPreviewCancelClick(object sender, RoutedEventArgs e)
        {
            lock (previewLock) {
                previewBuilder?.Cancel();
            }
        }

        private async void OnPublishLocationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var settings = provider.GetRequiredService<IAppSettings>();
            var hasSelection = !vm.SelectedLocation?.IsManualSelect ?? true;
            var newValue = hasSelection ? vm.SelectedLocation?.DisplayName : null;
            if (newValue == settings.Data.SelectedPublishLocation) return;

            settings.Data.SelectedPublishLocation = newValue;
            await settings.SaveAsync();
        }

        private void OnTreeOpenFolderClick(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedNode is ContentTreeFile fileNode) {
                OpenFolderSelectFile(fileNode.Filename);
            }
            else if (vm.SelectedNode is ContentTreeDirectory directoryNode) {
                OpenFolder(directoryNode.LocalPath);
            }
            else if (vm.SelectedNode is ContentTreeMaterialDirectory materialNode) {
                OpenFolder(materialNode.LocalPath);
            }
        }

        private void OpenFolder(string path)
        {
            var fullPath = PathEx.Join(vm.RootDirectory, path);

            if (!fullPath.EndsWith(Path.DirectorySeparatorChar))
                fullPath += Path.DirectorySeparatorChar;

            try {
                Process.Start("explorer.exe", fullPath);
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to open external directory \"{fullPath}\"!");
                ShowError($"Failed to open directory! {error.UnfoldMessageString()}");
            }
        }

        private void OpenFolderSelectFile(string file)
        {
            var fullFile = PathEx.Join(vm.RootDirectory, file);

            try {
                Process.Start("explorer.exe", $"/select,\"{fullFile}\"");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to open directory with file \"{fullFile}\"!");
                ShowError($"Failed to open directory! {error.UnfoldMessageString()}");
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
