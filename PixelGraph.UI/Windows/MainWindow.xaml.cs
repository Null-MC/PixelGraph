using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Controls;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Tabs;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using PixelGraph.Common.TextureFormats;

#if !NORENDER
using System.Windows.Media.Imaging;
#endif

namespace PixelGraph.UI.Windows
{
    public partial class MainWindow
    {
        private readonly ILogger<MainWindow> logger;
        private readonly IServiceProvider provider;
        private readonly IThemeHelper themeHelper;
        private readonly MainWindowViewModel viewModel;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            themeHelper = provider.GetRequiredService<IThemeHelper>();
            logger = provider.GetRequiredService<ILogger<MainWindow>>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new MainWindowViewModel(provider) {
                TextureModel = texturePreview.model,
                Dispatcher = Dispatcher,
                Model = Model,
            };

            FilterEditor.Initialize(provider);

#if !NORENDER
            Model.SceneProperties.DynamicSkyChanged += OnScenePropertiesDynamicSkyChanged;
            Model.SceneProperties.EnvironmentChanged += OnScenePropertiesEnvironmentChanged;

            viewModel.SceneProperties = renderPreview.SceneProperties;
            viewModel.RenderProperties = renderPreview.RenderProperties;
            PreviewKeyUp += OnWindowPreviewKeyUp;

            renderPreview.RefreshClick += OnPreviewRefreshClick;
            //renderPreview.ShaderCompileErrors += OnShaderCompileErrors;
#endif

            Model.SelectedLocation = ManualLocation;

            viewModel.TreeError += OnTreeViewError;

            Model.Profile.SelectionChanged += OnSelectedProfileChanged;
        }

        private async Task RefreshPreview(CancellationToken token = default)
        {
            if (Model.SelectedTab == null) return;

            viewModel.InvalidateTab(Model.SelectedTab.Id);
            await viewModel.UpdateTabPreviewAsync(token);
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
                Filter = "All Supported|*.zip;*.jar;*.mcpack|Zip Archive|*.zip|Java Archive|*.jar|McPack Archive|*.mcpack|All Files|*.*",
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

        private ITabModel GetTabModel(ContentTreeNode node)
        {
            var tabList = Model.TabList.AsEnumerable();
            if (Model.PreviewTab != null) tabList = tabList.Prepend(Model.PreviewTab);

            foreach (var tab in tabList) {
                switch (node) {
                    case ContentTreeMaterialDirectory materialNode when tab is MaterialTabModel materialTab: {
                        if (materialNode.MaterialFilename == materialTab.MaterialFilename)
                            return materialTab;

                        break;
                    }
                    case ContentTreeFile fileNode when tab is MaterialTabModel materialTab: {
                        if (fileNode.Filename == materialTab.MaterialFilename)
                            return materialTab;
                        break;
                    }
                    case ContentTreeFile fileNode when tab is TextureTabModel textureTab: {
                        if (fileNode.Filename == textureTab.ImageFilename)
                            return textureTab;

                        break;
                    }
                }
            }

            return null;
        }

        private static ITabModel BuildTabModel(ContentTreeNode node)
        {
            return node switch {
                ContentTreeMaterialDirectory materialNode => new MaterialTabModel {
                    DisplayName = materialNode.Name,
                    MaterialFilename = materialNode.MaterialFilename,
                },
                ContentTreeFile {Type: ContentNodeType.Material} materialFileNode => new MaterialTabModel {
                    DisplayName = materialFileNode.Name,
                    MaterialFilename = materialFileNode.Filename,
                },
                ContentTreeFile {Type: ContentNodeType.Texture} textureFileNode => new TextureTabModel {
                    DisplayName = textureFileNode.Name,
                    ImageFilename = textureFileNode.Filename,
                },
                _ => null,
            };
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
            var taskList = new List<Task>();

            taskList.Add(LoadWindowAsync());

#if !NORENDER
            taskList.Add(renderPreview.InitializeAsync(provider));
#endif

            await Task.WhenAll(taskList);

            await Dispatcher.BeginInvoke(() => {
                Model.EndInit();

#if !NORENDER
                renderPreview.Model.IsLoaded = true;
#endif
            });
        }

        private async Task LoadWindowAsync()
        {
            var publishLocationsTask = Task.Run(() => viewModel.LoadPublishLocationsAsync());
            var recentProjectsTask = Task.Run(() => viewModel.LoadRecentProjectsAsync());

            try {
                await publishLocationsTask;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load publishing locations!");
                ShowError($"Failed to load publishing locations! {error.UnfoldMessageString()}");
            }

            try {
                await recentProjectsTask;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load recent project list!");
                ShowError($"Failed to load recent project list! {error.UnfoldMessageString()}");
            }

            await Dispatcher.BeginInvoke(() => {
                try {
                    viewModel.Initialize();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to initialize main window!");
                    ShowError($"Failed to initialize main window! {error.UnfoldMessageString()}");
                }
            });
        }

        private async void OnRecentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecentList.SelectedItem is not string item) return;

            if (!Directory.Exists(item)) {
                //Model.RootDirectory = null;
                viewModel.Clear();
                MessageBox.Show(this, "The selected resource pack directory could not be found!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                await viewModel.RemoveRecentItemAsync(item);
                return;
            }

            try {
                await Task.Run(() => viewModel.SetRootDirectoryAsync(item));
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to load resource pack directory '{item}'!");

                Dispatcher.Invoke(() => {
                    viewModel.Clear();
                    MessageBox.Show(this, $"Failed to load resource pack directory! {error.UnfoldMessageString()}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
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

            viewModel.CloseAllTabs();
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

        private void OnCloseProjectClick(object sender, RoutedEventArgs e)
        {
            viewModel.CloseAllTabs();
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
                    //RootDirectory = Model.RootDirectory,
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
                    //RootDirectory = Model.RootDirectory,
                    Profiles = Model.Profile.List,
                    SelectedProfileItem = Model.Profile.Selected,
                },
            };

            window.ShowDialog();

            Model.Profile.Selected = window.Model.SelectedProfileItem;
        }

        private void OnTreeViewError(object sender, UnhandledExceptionEventArgs e)
        {
            var error = (Exception)e.ExceptionObject;
            ShowError($"Failed to update content tree! {error.Message}");
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

#if !NORENDER
                renderPreview.ViewModel.LoadAppSettings();
#endif
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

                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                serviceBuilder.Initialize();
                serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, Model.RootDirectory);

                await using var scope = serviceBuilder.Build();

                var materialWriter = scope.GetRequiredService<IMaterialWriter>();
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

        private async void OnChannelEditImageButtonClick(object sender, RoutedEventArgs e)
        {
            var tab = Model.SelectedTab;
            if (tab == null) return;

            try {
                await viewModel.BeginExternalEditAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to launch external image editor!");
                ShowError($"Failed to launch external image editor! {error.UnfoldMessageString()}");
                return;
            }

            viewModel.InvalidateTab(tab.Id);

            if (Model.SelectedTab == tab)
                await viewModel.UpdateTabPreviewAsync();
        }

        private void OnHelpDocumentationClick(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo {
                FileName = @"https://github.com/null511/PixelGraph/wiki",
                UseShellExecute = true,
            };

            using var process = Process.Start(info);
            process?.WaitForInputIdle(3_000);
        }

        private void OnHelpViewLogsClick(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo {
                FileName = "explorer",
                Arguments = $"\"{LocalLogFile.LogPath}\"",
                UseShellExecute = true,
            };

            using var process = Process.Start(info);
            process?.WaitForInputIdle(3_000);
        }

        private void OnPublishMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (!Model.Profile.HasLoaded) return;

            using var window = new PublishOutputWindow(provider) {
                Owner = this,
                Model = {
                    RootDirectory = Model.RootDirectory,
                    Input = Model.PackInput,
                    Profile = Model.Profile.Loaded,
                    Clean = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),
                },
            };

            if (Model.SelectedLocation != null && !Model.SelectedLocation.IsManualSelect) {
                var name = Model.Profile.Loaded.Name ?? GetDefaultPackProfileName(Model.Profile.Selected.LocalFile);
                if (name == null) throw new ApplicationException("Unable to determine profile name!");

                window.Model.Destination = Path.Combine(Model.SelectedLocation.Path, name);
                window.Model.Archive = Model.SelectedLocation.Archive;
            }
            else {
                var isBedrock = GameEdition.Is(Model.Profile.Loaded?.Edition, GameEdition.Bedrock);

                window.Model.Destination = GetArchiveFilename(isBedrock);
                window.Model.Archive = true;

                if (window.Model.Destination == null) return;
            }

            window.ShowDialog();
        }

        private static string GetDefaultPackProfileName(string localFile)
        {
            var name = Path.GetFileName(localFile);

            if (name.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
                name = name[..^9];

            return Path.GetFileNameWithoutExtension(name);
        }

        private async Task UpdateMaterialProperties(MaterialPropertyChangedEventArgs e)
        {
            var tab = Model.SelectedTab as MaterialTabModel;
            var material = tab?.MaterialRegistration.Value;
            if (material == null) return;

            await viewModel.SaveMaterialAsync(material);

            if (Model.SelectedTab != tab) return;

            if (Model.SelectedTab is MaterialTabModel matTab) {
#if !NORENDER
                if (string.Equals(e.ClassName, nameof(MaterialProperties), StringComparison.InvariantCultureIgnoreCase)) {
                    var updatePreview = string.Equals(e.PropertyName, nameof(MaterialProperties.BlendMode), StringComparison.InvariantCultureIgnoreCase) 
                                     || string.Equals(e.PropertyName, nameof(MaterialProperties.TintColor), StringComparison.InvariantCultureIgnoreCase);

                    if (updatePreview) {
                        renderPreview.RenderProperties.ApplyMaterial(matTab.MaterialRegistration?.Value);
                    }
                }
#endif
            }

            var channels = e.ClassName switch {
                nameof(MaterialColorProperties) => new[] {
                    EncodingChannel.ColorRed,
                    EncodingChannel.ColorGreen,
                    EncodingChannel.ColorBlue,
                },
                nameof(MaterialOpacityProperties) => new[] {EncodingChannel.Opacity},
                nameof(MaterialHeightProperties) => new[] {EncodingChannel.Height},
                nameof(MaterialNormalProperties) => new[] {
                    EncodingChannel.NormalX,
                    EncodingChannel.NormalY,
                    EncodingChannel.NormalZ,
                },
                nameof(MaterialOcclusionProperties) => new[] {EncodingChannel.Occlusion},
                nameof(MaterialSpecularProperties) => new[] {EncodingChannel.Specular},
                nameof(MaterialSmoothProperties) => new[] {EncodingChannel.Smooth},
                nameof(MaterialRoughProperties) => new[] {EncodingChannel.Rough},
                nameof(MaterialMetalProperties) => new[] {EncodingChannel.Metal},
                nameof(MaterialHcmProperties) => new[] {EncodingChannel.HCM},
                nameof(MaterialF0Properties) => new[] {EncodingChannel.F0},
                nameof(MaterialPorosityProperties) => new[] {EncodingChannel.Porosity},
                nameof(MaterialSssProperties) => new[] {EncodingChannel.SubSurfaceScattering},
                nameof(MaterialEmissiveProperties) => new[] {EncodingChannel.Emissive},
                _ => null,
            };

            if (channels == null) viewModel.InvalidateTab(tab.Id);
            else viewModel.InvalidateTabChannels(tab.Id, channels);

            await viewModel.UpdateTabPreviewAsync();
        }

        private async void OnMaterialPropertyChanged(object sender, MaterialPropertyChangedEventArgs e)
        {
            await UpdateMaterialProperties(e);
        }

        private async void OnMaterialFiltersChanged(object sender, EventArgs e)
        {
            await UpdateMaterialProperties(new MaterialPropertyChangedEventArgs(null, null));
        }

        private async void OnMaterialConnectionsChanged(object sender, EventArgs e)
        {
            await UpdateMaterialProperties(new MaterialPropertyChangedEventArgs(null, null));
        }

        private async void OnTextureTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Model.SelectedNode = e.NewValue as ContentTreeNode;

            var existingTab = GetTabModel(Model.SelectedNode);
            if (existingTab != null) {
                if (existingTab.IsPreview) {
                    Model.IsPreviewTabSelected = true;
                    Model.TabListSelection = null;
                }
                else {
                    Model.IsPreviewTabSelected = false;
                    Model.TabListSelection = existingTab;
                }
                return;
            }

            var newTab = BuildTabModel(Model.SelectedNode);
            if (newTab == null) {
                viewModel.ClearPreviewTab();
                return;
            }

            try {
                await viewModel.LoadTabContentAsync(newTab);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load tab content!");
                ShowError($"Failed to load tab content! {error.Message}");
                return;
            }

            newTab.IsPreview = true;
            await Dispatcher.BeginInvoke(() => viewModel.SetPreviewTab(newTab));
        }

        private async void OnContentTreeMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var existingTab = GetTabModel(Model.SelectedNode);
            if (existingTab != null) {
                if (!existingTab.IsPreview) {
                    Model.TabListSelection = existingTab;
                    return;
                }

                Model.PreviewTab = null;
                Model.IsPreviewTabSelected = false;
                existingTab.IsPreview = false;
                Model.TabList.Insert(0, existingTab);
                Model.TabListSelection = existingTab;
                return;
            }

            var newTab = BuildTabModel(Model.SelectedNode);
            if (newTab == null) return;

            await viewModel.LoadTabContentAsync(newTab);

            await Dispatcher.BeginInvoke(() => viewModel.AddNewTab(newTab));
        }

        private async void OnGenerateNormal(object sender, EventArgs e)
        {
            if (Model.SelectedTab is not MaterialTabModel materialTab) return;

            var material = materialTab.MaterialRegistration.Value;
            if (material == null) return;

            var outputName = TextureTags.Get(material, TextureTags.Normal);

            if (string.IsNullOrWhiteSpace(outputName)) {
                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                serviceBuilder.Initialize();
                serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, Model.RootDirectory);

                await using var scope = serviceBuilder.Build();

                var texWriter = scope.GetRequiredService<ITextureWriter>();
                outputName = texWriter.TryGet(TextureTags.Normal, material.Name, "png", material.UseGlobalMatching);
                if (outputName == null) {
                    // WARN: WHAT DO WE DO?!
                    throw new NotImplementedException();
                }
            }

            var path = PathEx.Join(Model.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "A normal texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            try {
                await viewModel.GenerateNormalAsync(material, fullName);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to generate normal texture!");
                ShowError($"Failed to generate normal texture! {error.UnfoldMessageString()}");
                return;
            }

            await Dispatcher.BeginInvoke(async () => {
                if (Model.SelectedTab != materialTab) return;
                if (!TextureTags.Is(Model.SelectedTag, TextureTags.Normal)) return;

                await RefreshPreview();
            });
        }

        private async void OnGenerateOcclusion(object sender, EventArgs e)
        {
            if (Model.SelectedTab is not MaterialTabModel materialTab) return;

            var material = materialTab.MaterialRegistration.Value;
            if (material == null) return;

            var outputName = TextureTags.Get(material, TextureTags.Occlusion);

            if (string.IsNullOrWhiteSpace(outputName)) {
                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                serviceBuilder.Initialize();
                serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, Model.RootDirectory);

                await using var scope = serviceBuilder.Build();

                var texWriter = scope.GetRequiredService<ITextureWriter>();
                outputName = texWriter.TryGet(TextureTags.Occlusion, material.Name, "png", material.UseGlobalMatching);

                if (outputName == null) {
                    // WARN: WHAT DO WE DO?!
                    throw new NotImplementedException();
                }
            }

            var path = PathEx.Join(Model.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "An occlusion texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            try {
                await viewModel.GenerateOcclusionAsync(material, fullName);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to generate occlusion texture!");
                ShowError($"Failed to generate occlusion texture! {error.UnfoldMessageString()}");
                return;
            }

            await Dispatcher.BeginInvoke(async () => {
                if (Model.SelectedTab != materialTab) return;
                if (!TextureTags.Is(Model.SelectedTag, TextureTags.Occlusion)) return;

                await RefreshPreview();
            });
        }

        private async void OnImportMaterialClick(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedNode is not ContentTreeFile fileNode) return;
            if (fileNode.Type != ContentNodeType.Texture) return;

            var material = await Task.Run(() => viewModel.ImportTextureAsync(fileNode.Filename));

            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, Model.RootDirectory);
            serviceBuilder.Services.AddSingleton<ContentTreeReader>();

            
            var parent = fileNode.Parent;

            if (parent == null) {
                await viewModel.LoadRootDirectoryAsync();
            }
            else {
                await Dispatcher.BeginInvoke(() => {
                    using var scope = serviceBuilder.Build();
                    var contentReader = scope.GetRequiredService<ContentTreeReader>();
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

            viewModel.InvalidateAllTabs();

            if (Model.SelectedTab != null)
                await viewModel.UpdateTabPreviewAsync();

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

        private void OnImageEditorCompleteClick(object sender, RoutedEventArgs e)
        {
            viewModel.CancelExternalImageEdit();
        }

#if !NORENDER
        private void OnWindowPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R) {
                var leftCtrl = e.KeyboardDevice.IsKeyDown(Key.LeftCtrl);
                var rightCtrl = e.KeyboardDevice.IsKeyDown(Key.RightCtrl);

                if (leftCtrl || rightCtrl) {
                    renderPreview.ViewModel.ReloadShaders();
                    renderPreview.ViewModel.UpdateShaders();
                    e.Handled = true;
                }
            }

            if (Model.IsViewModeRender) {
                if (e.Key == Key.PrintScreen) {
                    var screenshot = renderPreview.TakeScreenshot();
                    e.Handled = true;

                    Clipboard.SetImage(screenshot);
                    // TODO: show "copied to clipboard" toast
                }

                if (e.Key == Key.F2) {
                    var screenshot = renderPreview.TakeScreenshot();
                    e.Handled = true;

                    var saveFileDialog = new VistaSaveFileDialog {
                        Title = "Save ScreenShot",
                        Filter = "Bitmap File|*.bmp|PNG File|*.png",
                        FileName = $"{DateTime.Now:yyyyMMdd-HHmmss}.png",
                        OverwritePrompt = true,
                        AddExtension = true,
                    };

                    var result = saveFileDialog.ShowDialog();
                    if (result != true) return;

                    using var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(screenshot));
                    encoder.Save(fileStream);
                }
            }
        }

        private void OnScenePropertiesDynamicSkyChanged(object sender, EventArgs e)
        {
            renderPreview.Model.UpdateSunPosition();
        }

        private void OnScenePropertiesEnvironmentChanged(object sender, EventArgs e)
        {
            //await viewModel.UpdateTabPreviewAsync();
            viewModel.UpdateMaterials();
        }
#endif

        private async void OnPreviewRefreshClick(object sender, EventArgs e)
        {
            await RefreshPreview();
        }

        private void OnCloseDocumentTab(object sender, CloseTabEventArgs e)
        {
            viewModel.CloseTab(e.TabId);
        }

        private void OnCloseAllDocumentTabs(object sender, EventArgs e)
        {
            viewModel.CloseAllTabs();
        }

        private void OnConvertExistingClick(object sender, RoutedEventArgs e)
        {
            var window = new PackConvertWindow(provider) {
                Owner = this,
            };

            window.ShowDialog();
        }

        private async void OnMaterialPropertiesModelChanged(object sender, EventArgs e)
        {
            //renderPreview.UpdateModel(Model.SelectedTabMaterial);
            if (Model.SelectedTab == null) return;

            //await UpdateMaterialProperties(e);

            //viewModel.InvalidateTabModel();
            await viewModel.UpdateTabPreviewAsync();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
