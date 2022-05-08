using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.TextureFormats;
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
        private readonly IProjectContextManager projectContextMgr;
        private readonly IPublishLocationManager publishLocationMgr;


        public MainWindow(IServiceProvider provider)
        {
            this.provider = provider;

            themeHelper = provider.GetRequiredService<IThemeHelper>();
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            publishLocationMgr = provider.GetRequiredService<IPublishLocationManager>();
            logger = provider.GetRequiredService<ILogger<MainWindow>>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            Model.Initialize(provider);

            RecentProjects.Initialize(provider);
            MatPropertiesPanel.Initialize(provider);
            FilterEditor.Initialize(provider);

#if !NORENDER
            scenePropertiesPanel.Initialize(provider);

            Model.SceneProperties.DynamicSkyChanged += OnScenePropertiesDynamicSkyChanged;
            Model.SceneProperties.EnvironmentChanged += OnScenePropertiesEnvironmentChanged;

            PreviewKeyUp += OnWindowPreviewKeyUp;

            renderPreview.RefreshClick += OnPreviewRefreshClick;
            //renderPreview.ShaderCompileErrors += OnShaderCompileErrors;
#endif

            Model.SelectedLocation = ManualLocation;

            Model.TreeError += OnTreeViewError;
        }

        private async Task RefreshPreview(CancellationToken token = default)
        {
            if (Model.SelectedTab == null) return;

            Model.InvalidateTab(Model.SelectedTab.Id);
            await Model.UpdateTabPreviewAsync(Dispatcher, token);
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
            try {
                using var window = new ImportPackWindow(provider) {
                    Owner = this,
                    Model = {
                        //RootDirectory = Model.RootDirectory,
                        //PackInput = Model.PackInput,
                        ImportSource = source,
                        IsArchive = isArchive,
                    },
                };

                if (window.ShowDialog() != true) return;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in ImportPackWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return;
            }

            await Model.LoadRootDirectoryAsync(Dispatcher);
        }

        private string GetArchiveFilename(bool isBedrock)
        {
            string defaultExt;
            var saveFileDialog = new VistaSaveFileDialog {
                Title = "Save published archive",
                AddExtension = true,
            };

            var projectContext = projectContextMgr.GetContext();

            if (isBedrock) {
                defaultExt = ".mcpack";
                saveFileDialog.Filter = "MCPACK Archive|*.mcpack|All Files|*.*";

                if (projectContext.SelectedProfile != null)
                    saveFileDialog.FileName = $"{projectContext.SelectedProfile.Name}.mcpack";
            }
            else {
                defaultExt = ".zip";
                saveFileDialog.Filter = "ZIP Archive|*.zip|All Files|*.*";

                if (projectContext.SelectedProfile != null)
                    saveFileDialog.FileName = $"{projectContext.SelectedProfile?.Name}.zip";
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

        private async Task<bool> ShowLicenseAgreementAsync()
        {
            try {
                var window = new EndUserLicenseAgreementWindow(provider) {Owner = this};
                return window.ShowDialog() ?? false;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in EndUserLicenseAgreementWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return false;
            }
        }

        private async Task<bool> ShowTermsOfServiceAsync()
        {
            try {
                var window = new TermsOfServiceWindow(provider) {Owner = this};
                return window.ShowDialog() ?? false;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in TermsOfServiceWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return false;
            }
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
            if (!Model.HasAcceptedLicenseAgreement() && !await ShowLicenseAgreementAsync()) {
                await Dispatcher.BeginInvoke(Close);
                return;
            }

            if (!Model.HasAcceptedTermsOfService() && !await ShowTermsOfServiceAsync()) {
                await Dispatcher.BeginInvoke(Close);
                return;
            }

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
            var publishLocationsTask = Task.Run(() => publishLocationMgr.LoadAsync());

            try {
                await publishLocationsTask;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load publishing locations!");
                ShowError($"Failed to load publishing locations! {error.UnfoldMessageString()}");
            }

            await Dispatcher.BeginInvoke(() => {
                try {
                    Model.UpdatePublishLocations();
                    Model.Initialize();
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to initialize main window!");
                    ShowError($"Failed to initialize main window! {error.UnfoldMessageString()}");
                }
            });
        }

        private async Task LoadCurrentProject()
        {
            var projectContext = projectContextMgr.GetContext();

            await Dispatcher.BeginInvoke(() => {
                Model.ProjectFilename = projectContext.ProjectFilename;
            });

            await RecentProjects.AppendAsync(projectContext.ProjectFilename);

            try {
                await Task.Run(() => Model.LoadRootDirectoryAsync(Dispatcher));
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to load content for project '{projectContext.ProjectFilename}'!");

                Dispatcher.Invoke(() => {
                    Model.Clear();
                    MessageBox.Show(this, $"Failed to load project content! {error.UnfoldMessageString()}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async void OnNewProjectClick(object sender, RoutedEventArgs e)
        {
            var window = new NewProjectWindow(provider) {Owner = this};

            try {
                if (window.ShowDialog() != true) return;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in NewProjectWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return;
            }

            Model.CloseAllTabs();
            Model.Clear();

            await window.BuildProjectAsync();
            await LoadCurrentProject();

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
            var dialog = new VistaOpenFileDialog {
                FileName = "project.yml",
                Filter = "Project Yaml|*.yml;*.yaml|All Files|*.*",
            };

            if (dialog.ShowDialog(this) != true) return;

            try {
                await Model.LoadProjectAsync(dialog.FileName);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load project data!");
                await this.ShowMessageAsync("Error!", "Failed to load project!");
                return;
            }

            await LoadCurrentProject();
        }

        private async void OnOpenLegacyClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Multiselect = false,
            };

            if (dialog.ShowDialog(this) != true) return;

            var warnResult = await this.ShowMessageAsync("Warning!", "Are you sure you want to upgrade the selected project directory? This operation cannot be undone.", MessageDialogStyle.AffirmativeAndNegative);
            if (warnResult != MessageDialogResult.Affirmative) return;

            Model.CloseAllTabs();
            Model.Clear();

            logger.LogDebug($"Upgrading legacy project from directory '{dialog.SelectedPath}'.");
            var deleteFileList = new List<string>();

            try {
                var project = new ProjectData();

                var inputFile = Path.Combine(dialog.SelectedPath, "input.yml");
                if (!File.Exists(inputFile)) throw new ApplicationException("No 'input.yml' file was found!");
                deleteFileList.Add(inputFile);

                await using (var stream = File.OpenRead(inputFile)) {
                    project.Input = ResourcePackReader.ParseInput(stream);
                }

                foreach (var packFile in Directory.EnumerateFiles(dialog.SelectedPath, "*.pack.yml")) {
                    await using var stream = File.OpenRead(packFile);
                    var profile = ResourcePackReader.ParseProfile(stream);

                    if (profile.Name == null) {
                        profile.Name = Path.GetFileNameWithoutExtension(packFile);
                        if (profile.Name.EndsWith(".pack")) profile.Name = profile.Name[..^5];
                    }

                    project.Profiles.Add(profile);
                    deleteFileList.Add(packFile);
                }

                projectContextMgr.SetContext(new ProjectContext {
                    ProjectFilename = Path.Combine(dialog.SelectedPath, "project.yml"),
                    RootDirectory = dialog.SelectedPath,
                    Project = project,
                });

                await projectContextMgr.SaveAsync();
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to upgrade project data from directory '{dialog.SelectedPath}'!");
                await this.ShowMessageAsync("Error!", "Failed to upgrade project!");
                return;
            }

            var deleteFailureCount = 0;
            foreach (var file in deleteFileList) {
                try {
                    File.Delete(file);
                }
                catch (Exception error) {
                    logger.LogWarning(error, $"Failed to delete file '{file}' after project upgrade!");
                    deleteFailureCount++;
                }
            }

            if (deleteFailureCount > 0) {
                await this.ShowMessageAsync("Warning!", "The upgrade was successful, but some of the old files could not be removed!");
            }
            else {
                await this.ShowMessageAsync("Success!", "The upgrade was successful.");
            }

            await LoadCurrentProject();
        }

        private async void OnImportFolderClick(object sender, RoutedEventArgs e)
        {
            await ShowImportFolderAsync();
        }

        private void OnCloseProjectClick(object sender, RoutedEventArgs e)
        {
            Model.CloseAllTabs();
            Model.Clear();
        }

        private async void OnImportZipClick(object sender, RoutedEventArgs e)
        {
            await ShowImportArchiveAsync();
        }

        private async void OnEditProjectConfigClick(object sender, RoutedEventArgs e)
        {
            try {
                var window = new ProjectConfigWindow(provider) {Owner = this};
                window.ShowDialog();
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in ProjectConfigWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
            }
        }

        private async void OnEditPublishProfilesClick(object sender, RoutedEventArgs e)
        {
            try {
                var window = new PackProfilesWindow(provider) {Owner = this};
                if (window.ShowDialog() != true) return;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in PackProfilesWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return;
            }

            Model.UpdatePublishProfiles();

            Model.InvalidateAllTabs();
            await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private void OnTreeViewError(object sender, UnhandledExceptionEventArgs e)
        {
            var error = (Exception)e.ExceptionObject;
            ShowError($"Failed to update content tree! {error.Message}");
        }

        private async void OnLocationsClick(object sender, RoutedEventArgs e)
        {
            try {
                var window = new PublishLocationsWindow(provider) {Owner = this};
                if (window.ShowDialog() != true) return;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in PublishLocationsWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return;
            }

            Model.UpdatePublishLocations();
        }

        private async void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            try {
                var window = new SettingsWindow(provider) {Owner = this};
                if (window.ShowDialog() != true) return;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in SettingsWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
                return;
            }

            themeHelper.ApplyCurrent(this);

#if !NORENDER
            renderPreview.ViewModel.LoadAppSettings();
#endif
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

                var projectContext = projectContextMgr.GetContext();
                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                serviceBuilder.Initialize();
                serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

                await using var scope = serviceBuilder.Build();

                var matWriter = scope.GetRequiredService<IMaterialWriter>();
                await matWriter.WriteAsync(material);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to create new material definition!");
                ShowError("Failed to create new material definition!");
                return;
            }

            Model.ReloadContent();

            // TODO: select the new TreeView node
        }

        private async void OnChannelEditImageButtonClick(object sender, RoutedEventArgs e)
        {
            var tab = Model.SelectedTab;
            if (tab == null) return;

            try {
                await Model.BeginExternalEditAsync();
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to launch external image editor!");
                ShowError($"Failed to launch external image editor! {error.UnfoldMessageString()}");
                return;
            }

            Model.InvalidateTab(tab.Id);

            if (Model.SelectedTab == tab)
                await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private async void OnHelpDocumentationClick(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo {
                FileName = @"https://github.com/null511/PixelGraph-Release/wiki",
                UseShellExecute = true,
            };

            try {
                using var process = Process.Start(info);
                process?.WaitForInputIdle(3_000);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to launch wiki link url!");
                await this.ShowMessageAsync("Error!", $"Failed to launch wiki! {error.UnfoldMessageString()}");
            }
        }

        private async void OnHelpAboutClick(object sender, RoutedEventArgs e)
        {
            try {
                var window = new AboutWindow(provider) {Owner = this};
                window.ShowDialog();
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in SettingsWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
            }
        }

        private async void OnHelpViewLogsClick(object sender, RoutedEventArgs e)
        {
            var info = new ProcessStartInfo {
                FileName = "explorer",
                Arguments = $"\"{LocalLogFile.LogPath}\"",
                UseShellExecute = true,
            };

            try {
                using var process = Process.Start(info);
                process?.WaitForInputIdle(3_000);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to open logging directory!");
                await this.ShowMessageAsync("Error!", $"Failed to open log folder! {error.UnfoldMessageString()}");
            }
        }

        private async void OnPublishMenuItemClick(object sender, RoutedEventArgs e)
        {
            var projectContext = projectContextMgr.GetContext();
            if (projectContext.SelectedProfile == null) return;

            try {
                using var window = new PublishOutputWindow(provider) {
                    Owner = this,
                    Model = {
                        Clean = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),
                    },
                };

                if (Model.SelectedLocation != null && !Model.SelectedLocation.IsManualSelect) {
                    var name = projectContext.SelectedProfile.Name;
                    if (name == null) throw new ApplicationException("Unable to determine profile name!");

                    window.Model.Destination = Path.Combine(Model.SelectedLocation.Path, name);
                    window.Model.Archive = Model.SelectedLocation.Archive;
                }
                else {
                    var isBedrock = GameEdition.Is(projectContext.SelectedProfile.Edition, GameEdition.Bedrock);

                    window.Model.Destination = GetArchiveFilename(isBedrock);
                    window.Model.Archive = true;

                    if (window.Model.Destination == null) return;
                }

                window.ShowDialog();
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred in PublishOutputWindow!");
                await this.ShowMessageAsync("Error!", $"An unknown error has occurred! {error.UnfoldMessageString()}");
            }
        }

        private async Task UpdateMaterialProperties(MaterialPropertyChangedEventArgs e)
        {
            var tab = Model.SelectedTab as MaterialTabModel;
            var material = tab?.MaterialRegistration.Value;
            if (material == null) return;

            await Model.SaveMaterialAsync(material);

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

            if (channels == null) Model.InvalidateTab(tab.Id);
            else Model.InvalidateTabChannels(tab.Id, channels);

            await Model.UpdateTabPreviewAsync(Dispatcher);
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
                Model.ClearPreviewTab();
                return;
            }

            try {
                await Model.LoadTabContentAsync(newTab);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to load tab content!");
                ShowError($"Failed to load tab content! {error.UnfoldMessageString()}");
                return;
            }

            newTab.IsPreview = true;
            await Dispatcher.BeginInvoke(() => Model.SetPreviewTab(newTab));
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

            await Model.LoadTabContentAsync(newTab);

            await Dispatcher.BeginInvoke(() => Model.AddNewTab(newTab));
        }

        private async void OnGenerateNormal(object sender, EventArgs e)
        {
            if (Model.SelectedTab is not MaterialTabModel materialTab) return;

            var material = materialTab.MaterialRegistration.Value;
            if (material == null) return;

            var projectContext = projectContextMgr.GetContext();
            var outputName = TextureTags.Get(material, TextureTags.Normal);

            if (string.IsNullOrWhiteSpace(outputName)) {
                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                serviceBuilder.Initialize();
                serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

                await using var scope = serviceBuilder.Build();

                var texWriter = scope.GetRequiredService<ITextureWriter>();
                outputName = texWriter.TryGet(TextureTags.Normal, material.Name, "png", material.UseGlobalMatching);
                if (outputName == null) {
                    // WARN: WHAT DO WE DO?!
                    throw new NotImplementedException();
                }
            }

            var path = PathEx.Join(projectContext.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "A normal texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            try {
                await Model.GenerateNormalAsync(material, fullName);
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

            var projectContext = projectContextMgr.GetContext();
            var outputName = TextureTags.Get(material, TextureTags.Occlusion);

            if (string.IsNullOrWhiteSpace(outputName)) {
                var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                serviceBuilder.Initialize();
                serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

                await using var scope = serviceBuilder.Build();

                var texWriter = scope.GetRequiredService<ITextureWriter>();
                outputName = texWriter.TryGet(TextureTags.Occlusion, material.Name, "png", material.UseGlobalMatching);

                if (outputName == null) {
                    // WARN: WHAT DO WE DO?!
                    throw new NotImplementedException();
                }
            }

            var path = PathEx.Join(projectContext.RootDirectory, material.LocalPath);
            if (!material.UseGlobalMatching) path = PathEx.Join(path, material.Name);
            var fullName = PathEx.Join(path, outputName);

            if (File.Exists(fullName)) {
                var result = MessageBox.Show(this, "An occlusion texture already exists! Would you like to overwrite it?", "Warning", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;
            }

            try {
                await Model.GenerateOcclusionAsync(material, fullName);
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

            var material = await Task.Run(() => Model.ImportTextureAsync(fileNode.Filename));

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.Services.AddSingleton<ContentTreeReader>();

            var parent = fileNode.Parent;

            if (parent == null) {
                await Model.LoadRootDirectoryAsync(Dispatcher);
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
            Model.ReloadContent();
        }

        private async void OnPublishLocationSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model.IsInitializing) return;

            var settings = provider.GetRequiredService<IAppSettings>();
            var hasSelection = !(Model.SelectedLocation?.IsManualSelect ?? true);

            settings.Data.SelectedPublishLocation = hasSelection
                ? Model.SelectedLocation?.DisplayName : null;

            await settings.SaveAsync();
        }

        //private async void OnPublishProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (Model.IsInitializing) return;

        //    var projectContext = projectContextMgr.GetContext();

        //    if (projectContext != null)
        //        projectContext.SelectedProfile = Model.SelectedProfile;

        //    Model.InvalidateAllTabs();

        //    if (Model.SelectedTab != null)
        //        await Model.UpdateTabPreviewAsync(Dispatcher);
        //}

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
            var projectContext = projectContextMgr.GetContext();
            var fullPath = PathEx.Join(projectContext.RootDirectory, path);

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
            var projectContext = projectContextMgr.GetContext();
            var fullFile = PathEx.Join(projectContext.RootDirectory, file);

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
            Model.CancelExternalImageEdit();
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
            Model.UpdateMaterials();
        }
#endif

        private async void OnPreviewRefreshClick(object sender, EventArgs e)
        {
            await RefreshPreview();
        }

        private void OnCloseAllDocumentTabs(object sender, EventArgs e)
        {
            Model.CloseAllTabs();
        }

        //private void OnConvertExistingClick(object sender, RoutedEventArgs e)
        //{
        //    var window = new PackConvertWindow(provider) {
        //        Owner = this,
        //    };

        //    window.ShowDialog();
        //}

        private async void OnRecentProjectTileClicked(object _, TileClickedEventArgs e)
        {
            try {
                await Task.Run(() => Model.LoadProjectAsync(e.Filename));
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to load recent project '{e.Filename}'!");

                Dispatcher.Invoke(() => {
                    Model.Clear();
                    MessageBox.Show(this, $"Failed to load project! {error.UnfoldMessageString()}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                });

                await RecentProjects.Model.RemoveAsync(e.Filename);
                return;
            }

            await LoadCurrentProject();
        }

        private async void OnMaterialPropertiesModelChanged(object sender, EventArgs e)
        {
            //renderPreview.UpdateModel(Model.SelectedTabMaterial);
            if (Model.SelectedTab == null) return;

            //await UpdateMaterialProperties(e);

            //viewModel.InvalidateTabModel();
            await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private async void OnSelectedTabChanged(object sender, EventArgs e)
        {
            if (Model.SelectedTab != null)
                await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private async void OnSelectedProfileChanged(object sender, EventArgs e)
        {
            Model.InvalidateAllTabs();

            if (Model.SelectedTab != null)
                await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private async void OnSelectedTagChanged(object sender, EventArgs e)
        {
            if (Model.SelectedTab == null) return;

            Model.InvalidateAllTabLayers();
            await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private async void OnViewModeChanged(object sender, EventArgs e)
        {
            if (Model.SelectedTab == null) return;

            //Model.InvalidateTabLayer();
            await Model.UpdateTabPreviewAsync(Dispatcher);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
