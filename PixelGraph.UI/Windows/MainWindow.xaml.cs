using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private readonly IServiceProvider provider;
        private readonly IThemeHelper themeHelper;
        private readonly ILogger<MainWindow> logger;
        private readonly MainViewModel viewModel;
        private readonly PreviewViewModel previewViewModel;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            themeHelper = provider.GetRequiredService<IThemeHelper>();
            logger = provider.GetRequiredService<ILogger<MainWindow>>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new MainViewModel(provider) {
                Dispatcher = Dispatcher,
                Model = Model,
            };

            viewModel.TreeError += OnTreeViewError;

            previewViewModel = new PreviewViewModel(provider) {
                Dispatcher = Dispatcher,
                Model = Model,
            };

            previewViewModel.ShaderCompileErrors += OnShaderCompileErrors;

            Model.Preview.EnvironmentCube = EnvironmentCubeMapSource;
            Model.Preview.IrradianceCube = IrradianceCubeMapSource;
            Model.SelectedLocation = ManualLocation;
            Model.Profile.SelectionChanged += OnSelectedProfileChanged;
        }

        private void OnTreeViewError(object sender, UnhandledExceptionEventArgs e)
        {
            var error = (Exception)e.ExceptionObject;
            ShowError($"Failed to update content tree! {error.Message}");
        }

        private async Task SelectRootDirectoryAsync(CancellationToken token)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) == true)
                await viewModel.SetRootDirectoryAsync(dialog.SelectedPath, token);
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

        private async Task ImportPackAsync(string source, bool isArchive)
        {
            using var window = new ImportPackWindow(provider) {
                Owner = this,
                Model = {
                    RootDirectory = Model.RootDirectory,
                    PackInput = Model.PackInput,
                    ImportSource = source,
                    IsArchive = isArchive,
                },
            };

            if (window.ShowDialog() != null)
                await viewModel.LoadRootDirectoryAsync();
        }

        private string GetArchiveFilename(bool isBedrock)
        {
            string defaultExt;
            var saveFileDialog = new VistaSaveFileDialog {
                Title = "Save published archive",
                AddExtension = true,
            };

            if (isBedrock) {
                defaultExt = ".mcpack";
                saveFileDialog.Filter = "MCPACK Archive|*.mcpack|All Files|*.*";
                saveFileDialog.FileName = $"{Model.Profile.Selected?.Name}.mcpack";
            }
            else {
                defaultExt = ".zip";
                saveFileDialog.Filter = "ZIP Archive|*.zip|All Files|*.*";
                saveFileDialog.FileName = $"{Model.Profile.Selected?.Name}.zip";
            }

            var result = saveFileDialog.ShowDialog();
            if (result != true) return null;

            var filename = saveFileDialog.FileName;
            if (saveFileDialog.FilterIndex == 1 && !filename.EndsWith(defaultExt, StringComparison.InvariantCultureIgnoreCase))
                filename += defaultExt;

            return filename;
        }

        public async Task PopulateTextureViewerAsync(CancellationToken token = default)
        {
            if (Model.SelectedNode is ContentTreeFile {Type: ContentNodeType.Texture} texFile) {
                var fullFile = PathEx.Join(Model.RootDirectory, texFile.Filename);

                await previewViewModel.SetFromFileAsync(fullFile);

                Model.Material.Loaded = null;
                return;
            }

            previewViewModel.Cancel();
            await previewViewModel.ClearAsync();

            bool isMat;
            string matFile;
            if (Model.SelectedNode is ContentTreeMaterialDirectory matFolder) {
                isMat = true;
                matFile = matFolder.MaterialFilename;
            }
            else {
                var fileNode = Model.SelectedNode as ContentTreeFile;
                isMat = fileNode?.Type == ContentNodeType.Material;
                matFile = fileNode?.Filename;
            }

            if (!isMat) {
                Model.Material.Loaded = null;
                return;
            }

            // TODO: wait for texture busy
            var reader = provider.GetRequiredService<IInputReader>();
            var matReader = provider.GetRequiredService<IMaterialReader>();

            reader.SetRoot(Model.RootDirectory);
            Model.Material.LoadedFilename = matFile;

            try {
                Model.Material.Loaded = await matReader.LoadAsync(matFile, token);
            }
            catch (Exception error) {
                Model.Material.Loaded = null;
                logger.LogError(error, "Failed to load material properties!");
                ShowError($"Failed to load material properties! {error.UnfoldMessageString()}");
            }

            var enableAutoMaterial = Model.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;

            if (Model.Material.Loaded == null && enableAutoMaterial) {
                var localPath = Path.GetDirectoryName(matFile);

                Model.Material.Loaded = new MaterialProperties {
                    LocalFilename = matFile,
                    LocalPath = Path.GetDirectoryName(localPath),
                    Name = Path.GetFileName(localPath),
                };
            }

            await previewViewModel.UpdateAsync(true, token);
        }

        private void ShowError(string message)
        {
            Dispatcher.Invoke(() => {
                MessageBox.Show(this, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        #region Events

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try {
                await viewModel.LoadPublishLocationsAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load publishing locations!");
                ShowError($"Failed to load publishing locations! {error.UnfoldMessageString()}");
            }

            try {
                previewViewModel.Initialize();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to initialize render preview!");
                ShowError($"Failed to initialize 3D viewport! {error.UnfoldMessageString()}");
            }

            try {
                await viewModel.InitializeAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to initialize main window!");
                ShowError($"Errors occurred during startup! {error.UnfoldMessageString()}");
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            previewViewModel.Dispose();
        }

        private async void OnRecentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecentList.SelectedItem is not string item) return;

            try {
                await viewModel.SetRootDirectoryAsync(item);
            }
            catch (DirectoryNotFoundException) {
                Dispatcher.Invoke(() => {
                    Model.RootDirectory = null;
                    MessageBox.Show(this, "The selected resource pack directory could not be found!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                });

                await viewModel.RemoveRecentItemAsync(item);
            }
        }

        private async void OnNewProjectClick(object sender, RoutedEventArgs e)
        {
            var window = new NewProjectWindow(provider) {
                Owner = this,
            };

            if (window.ShowDialog() != true) return;

            await previewViewModel.ClearAsync();
            viewModel.Clear();

            await viewModel.SetRootDirectoryAsync(window.Model.Location, CancellationToken.None);

            if (window.Model.EnablePackImport) {
                if (window.Model.ImportFromDirectory) {
                    await ShowImportFolderAsync();
                }
                else if (window.Model.ImportFromArchive) {
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

        private async void OnCloseProjectClick(object sender, RoutedEventArgs e)
        {
            await previewViewModel.ClearAsync();
            viewModel.Clear();
        }

        private async void OnImportZipClick(object sender, RoutedEventArgs e)
        {
            await ShowImportArchiveAsync();
        }

        private void OnInputEncodingClick(object sender, RoutedEventArgs e)
        {
            var window = new PackInputWindow(provider) {
                Owner = this,
                Model = {
                    RootDirectory = Model.RootDirectory,
                    PackInput = (ResourcePackInputProperties)Model.PackInput.Clone(),
                },
            };

            if (window.ShowDialog() != true) return;
            Model.PackInput = window.Model.PackInput;
        }

        private void OnProfilesClick(object sender, RoutedEventArgs e)
        {
            var window = new PackProfilesWindow(provider) {
                Owner = this,
                Model = {
                    RootDirectory = Model.RootDirectory,
                    Profiles = Model.Profile.List,
                    SelectedProfileItem = Model.Profile.Selected,
                },
            };

            window.ShowDialog();

            Model.Profile.Selected = window.Model.SelectedProfileItem;
        }

        private void OnLocationsClick(object sender, RoutedEventArgs e)
        {
            var window = new PublishLocationsWindow(provider) {
                Owner = this,
                Model = {
                    Locations = new ObservableCollection<LocationModel>(Model.PublishLocations),
                },
            };

            if (Model.SelectedLocation != null && !Model.SelectedLocation.IsManualSelect)
                window.Model.SelectedLocationItem = Model.SelectedLocation;

            var result = window.ShowDialog();
            if (result != true) return;

            Model.PublishLocations = window.Model.Locations.ToList();
            Model.SelectedLocation = window.Model.SelectedLocationItem;
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            var window = new SettingsWindow(provider) {
                Owner = this,
            };

            if (window.ShowDialog() == true) {
                themeHelper.ApplyCurrent(this);
                previewViewModel.LoadAppSettings();
            }
        }

        private async void OnNewMaterialMenuClick(object sender, RoutedEventArgs e)
        {
            var window = new NewMaterialWindow(provider) {
                Owner = this,
            };

            if (window.ShowDialog() != true) return;

            try {
                if (string.IsNullOrWhiteSpace(window.Model.Location))
                    throw new ApplicationException("New material Location cannot be empty!");

                var name = Path.GetFileName(window.Model.Location);
                var localPath = Path.GetDirectoryName(window.Model.Location);
                var localFile = PathEx.Join(localPath, name, "mat.yml");

                var material = new MaterialProperties {
                    Name = name,
                    LocalPath = localPath,
                    LocalFilename = localFile,
                    UseGlobalMatching = false,
                };

                var writer = provider.GetRequiredService<IOutputWriter>();
                var materialWriter = provider.GetRequiredService<IMaterialWriter>();

                writer.SetRoot(Model.RootDirectory);
                await materialWriter.WriteAsync(material);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to create new material definition!");
                ShowError("Failed to create new material definition!");
                return;
            }

            viewModel.ReloadContent();

            // TODO: select the new TreeView node
        }

        private void OnMaterialConnectionsMenuClick(object sender, RoutedEventArgs e)
        {
            ShowError("Not Yet Implemented");
        }

        private void OnMaterialFiltersMenuClick(object sender, RoutedEventArgs e)
        {
            ShowError("Not Yet Implemented");
        }

        private void OnPublishMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (!Model.Profile.HasSelection) return;

            using var window = new PublishOutputWindow(provider) {
                Owner = this,
                Model = {
                    RootDirectory = Model.RootDirectory,
                    Profile = Model.Profile.Selected,

                    // TODO: Attach this to something on the UI
                    Clean = false,
                },
            };

            if (Model.SelectedLocation != null && !Model.SelectedLocation.IsManualSelect) {
                var name = Model.Profile.Selected.Name ?? Path.GetFileNameWithoutExtension(Model.Profile.Selected.LocalFile);
                if (name == null) throw new ApplicationException("Unable to determine profile name!");

                window.Model.Destination = Path.Combine(Model.SelectedLocation.Path, name);
                window.Model.Archive = Model.SelectedLocation.Archive;
            }
            else {
                // TODO: Finish bedrock mcpack support
                var isBedrock = false;
                window.Model.Destination = GetArchiveFilename(isBedrock);
                window.Model.Archive = true;

                if (window.Model.Destination == null) return;
            }

            window.ShowDialog();
        }

        private async void OnMaterialChanged(object sender, EventArgs e)
        {
            await viewModel.SaveMaterialAsync();

            //await UpdatePreviewAsync(false);
            await previewViewModel.UpdateAsync(true);
        }

        private async void OnTextureTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Model.SelectedNode = e.NewValue as ContentTreeNode;

            await Task.Run(async () => {
                try {
                    await PopulateTextureViewerAsync();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to populate texture viewer!");
                    //ShowError("Failed to populate texture viewer!");
                }
            });
        }

        private async void OnGenerateNormal(object sender, EventArgs e)
        {
            if (!Model.Material.HasLoaded) return;

            var material = Model.Material.Loaded;
            var outputName = TextureTags.Get(material, TextureTags.Normal);

            if (string.IsNullOrWhiteSpace(outputName)) {
                outputName = NamingStructure.Get(TextureTags.Normal, material.Name, "png", material.UseGlobalMatching);
            }

            var path = PathEx.Join(Model.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "A normal texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            try {
                await viewModel.GenerateNormalAsync(fullName);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to generate normal texture!");
                ShowError($"Failed to generate normal texture! {error.UnfoldMessageString()}");
            }
        }

        private async void OnGenerateOcclusion(object sender, EventArgs e)
        {
            if (!Model.Material.HasLoaded) return;

            var material = Model.Material.Loaded;
            var outputName = TextureTags.Get(material, TextureTags.Occlusion);

            if (string.IsNullOrWhiteSpace(outputName)) {
                outputName = NamingStructure.Get(TextureTags.Occlusion, material.Name, "png", material.UseGlobalMatching);
            }

            var path = PathEx.Join(Model.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "An occlusion texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            try {
                await viewModel.GenerateOcclusionAsync(fullName);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to generate occlusion texture!");
                ShowError($"Failed to generate occlusion texture! {error.UnfoldMessageString()}");
            }
        }

        private async void OnImportMaterialClick(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedNode is not ContentTreeFile fileNode) return;
            if (fileNode.Type != ContentNodeType.Texture) return;

            var material = await Task.Run(() => viewModel.ImportTextureAsync(fileNode.Filename));

            var contentReader = provider.GetRequiredService<IContentTreeReader>();
            var parent = fileNode.Parent;

            if (parent == null) {
                await viewModel.LoadRootDirectoryAsync();
            }
            else {
                await Dispatcher.BeginInvoke(() => {
                    contentReader.Update(parent);

                    var selected = parent.Nodes.FirstOrDefault(n => {
                        if (n is not ContentTreeMaterialDirectory materialNode) return false;
                        return string.Equals(materialNode.MaterialFilename, material.LocalFilename);
                    });

                    if (selected != null) Model.SelectedNode = selected;
                });
            }
        }

        private void OnContentTreePreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            while (source != null && source is not TreeViewItem)
                source = VisualTreeHelper.GetParent(source);

            (source as TreeViewItem)?.Focus();
        }

        private void OnContentRefreshClick(object sender, RoutedEventArgs e)
        {
            viewModel.ReloadContent();
        }

        private async void OnSelectedProfileChanged(object sender, EventArgs e)
        {
            await viewModel.UpdateSelectedProfileAsync();

            await previewViewModel.UpdateAsync(false);

            // TODO: update recent 
        }

        //private void OnPreviewCancelClick(object sender, RoutedEventArgs e)
        //{
        //    previewViewModel.Cancel();
        //}

        private async void OnPublishLocationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model.IsInitializing) return;

            var settings = provider.GetRequiredService<IAppSettings>();
            var hasSelection = !(Model.SelectedLocation?.IsManualSelect ?? true);

            settings.Data.SelectedPublishLocation = hasSelection
                ? Model.SelectedLocation?.DisplayName : null;

            await settings.SaveAsync();
        }

        private void OnTreeOpenFolderClick(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedNode is ContentTreeFile fileNode) {
                OpenFolderSelectFile(fileNode.Filename);
            }
            else if (Model.SelectedNode is ContentTreeDirectory directoryNode) {
                OpenFolder(directoryNode.LocalPath);
            }
            else if (Model.SelectedNode is ContentTreeMaterialDirectory materialNode) {
                OpenFolder(materialNode.LocalPath);
            }
        }

        private void OpenFolder(string path)
        {
            var fullPath = PathEx.Join(Model.RootDirectory, path);

            if (!fullPath.EndsWith(Path.DirectorySeparatorChar))
                fullPath += Path.DirectorySeparatorChar;

            try {
                using var _ = Process.Start("explorer.exe", fullPath);
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to open external directory \"{fullPath}\"!");
                ShowError($"Failed to open directory! {error.UnfoldMessageString()}");
            }
        }

        private void OpenFolderSelectFile(string file)
        {
            var fullFile = PathEx.Join(Model.RootDirectory, file);

            try {
                using var _ = Process.Start("explorer.exe", $"/select,\"{fullFile}\"");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to open directory with file \"{fullFile}\"!");
                ShowError($"Failed to open directory! {error.UnfoldMessageString()}");
            }
        }

        private async void OnEditLayer(object sender, EventArgs e)
        {
            try {
                await viewModel.BeginExternalEditAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to launch external image editor!");
                ShowError($"Failed to launch external image editor! {error.UnfoldMessageString()}");
                return;
            }

            await previewViewModel.UpdateLayerAsync();
        }

        private void OnImageEditorCompleteClick(object sender, RoutedEventArgs e)
        {
            viewModel.CancelExternalImageEdit();
        }

        private void OnWindowPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R) {
                var leftCtrl = e.KeyboardDevice.IsKeyDown(Key.LeftCtrl);
                var rightCtrl = e.KeyboardDevice.IsKeyDown(Key.RightCtrl);

                if (leftCtrl || rightCtrl) {
                    previewViewModel.ReloadShaders();
                    e.Handled = true;
                }
            }
        }

        private async void OnRenderModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model.IsInitializing) return;
            await Task.Run(() => previewViewModel.SaveRenderStateAsync());
        }

        private async void OnShaderCompileErrors(object sender, ShaderCompileErrorEventArgs e)
        {
            var message = new StringBuilder("Failed to compile shaders!");

            foreach (var error in e.Errors) {
                message.AppendLine();
                message.Append(error.Message);
            }

            await Dispatcher.BeginInvoke(() => {
                MessageBox.Show(this, message.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion
    }
}
