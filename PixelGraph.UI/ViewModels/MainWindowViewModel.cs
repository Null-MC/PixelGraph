using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xaml.Behaviors.Core;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Tabs;
using PixelGraph.UI.ViewData;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

#if NORENDER
using PixelGraph.UI.Models.MockScene;
#else
using PixelGraph.Rendering.Models;
using PixelGraph.UI.Models.Scene;
#endif

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowViewModel : ModelBase, ISearchParameters
    {
        private readonly object busyLock;

        private ILogger<MainWindowViewModel> logger;
        private IAppSettingsManager appSettingsMgr;
        private ITabPreviewManager tabPreviewMgr;
        private IProjectContextManager projectContextMgr;
        private IPublishLocationManager publishLocationMgr;
        private MaterialPropertiesCache materialCache;
        private TextureEditUtility editUtility;

        private IServiceProvider _provider;
        private volatile bool _isBusy, _isInitializing;
        private volatile bool _isImageEditorOpen;
        private List<LocationDisplayModel> _publishLocations;
        private PublishProfileDisplayRow _selectedProfile;
        private string _projectFilename;
        private string _searchText;
        private bool _showAllFiles;
        private ContentTreeNode _selectedNode;
        private LocationDisplayModel _selectedLocation;
        private ContentTreeNode _treeRoot;
        private ITabModel _tabListSelection;
        private ITabModel _previewTab;
        private bool _isPreviewTabSelected;
        private bool _isRenderPreviewOn;
        private bool _isRenderPreviewEnabled;
        private EditModes _editMode;
        private string _selectedTag;

        public event EventHandler<UnhandledExceptionEventArgs> TreeError;
        public event EventHandler SelectedTabChanged;
        public event EventHandler SelectedTagChanged;
        public event EventHandler ViewModeChanged;
        public event EventHandler SelectedProfileChanged;

        public ObservableCollection<PublishProfileDisplayRow> ProfileList {get;}
        public TexturePreviewModel TextureModel {get;}
        public ObservableCollection<ITabModel> TabList {get;}
        public ICommand TabCloseButtonCommand {get;}

#if NORENDER
        public MockScenePropertiesModel SceneProperties {get;}
        public MockRenderPropertiesModel RenderProperties {get;}
#else
        public ScenePropertiesModel SceneProperties {get;}
        public RenderPropertiesModel RenderProperties {get;}
#endif


        public bool IsInitializing => _isInitializing;
        public bool IsProjectLoaded => _projectFilename != null;
        public bool HasProfileSelected => _selectedProfile != null;
        public bool HasTreeSelection => _selectedNode is ContentTreeFile;
        public bool HasTreeMaterialSelection => _selectedNode is ContentTreeMaterialDirectory;
        public bool HasTreeTextureSelection => _selectedNode is ContentTreeFile {Type: ContentNodeType.Texture};
        public bool HasSelectedTag => _selectedTag != null;
        public MaterialProperties SelectedTabMaterial => (SelectedTab as MaterialTabModel)?.MaterialRegistration?.Value;
        public ITabModel SelectedTab => _isPreviewTabSelected ? _previewTab : _tabListSelection;
        public bool HasSelectedMaterial => SelectedTab is MaterialTabModel;
        public bool HasSelectedTab => IsPreviewTabSelected || TabListSelection != null;
        public bool HasPreviewTab => PreviewTab != null;


        public PublishProfileDisplayRow SelectedProfile {
            get => _selectedProfile;
            set {
                if (value == _selectedProfile) return;
                _selectedProfile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasProfileSelected));

                OnSelectedProfileChanged();
            }
        }

        public ITabModel PreviewTab {
            get => _previewTab;
            set {
                if (_previewTab == value) return;
                _previewTab = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPreviewTab));

                if (_isPreviewTabSelected) {
                    OnPropertyChanged(nameof(SelectedTab));
                    OnPropertyChanged(nameof(SelectedTabMaterial));
                    OnPropertyChanged(nameof(HasSelectedMaterial));
                    OnSelectedTabChanged();
                }
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                if (value == _selectedTag) return;

                _selectedTag = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasSelectedTag));
                OnSelectedTagChanged();
            }
        }

        public bool IsRenderPreviewEnabled {
            get => _isRenderPreviewEnabled && RenderPreview.IsSupported;
            private set {
                if (_isRenderPreviewEnabled == value) return;
                _isRenderPreviewEnabled = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsRenderPreviewOn));
                OnViewModeChanged();
            }
        }

        public bool IsRenderPreviewOn {
            get => _isRenderPreviewOn && _isRenderPreviewEnabled && RenderPreview.IsSupported;
            set {
                if (_isRenderPreviewOn == value) return;
                _isRenderPreviewOn = value;
                OnPropertyChanged();

                OnViewModeChanged();
            }
        }

        public bool IsEditModeMaterial {
            get => _editMode == EditModes.Material;
            set {
                if (!value) return;
                _editMode = EditModes.Material;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsEditModeFilters));
                OnPropertyChanged(nameof(IsEditModeConnections));
                OnPropertyChanged(nameof(IsEditModeScene));
            }
        }

        public bool IsEditModeFilters {
            get => _editMode == EditModes.Filters;
            set {
                if (!value) return;
                _editMode = EditModes.Filters;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeMaterial));
                OnPropertyChanged(nameof(IsEditModeConnections));
                OnPropertyChanged(nameof(IsEditModeScene));
            }
        }

        public bool IsEditModeConnections {
            get => _editMode == EditModes.Connections;
            set {
                if (!value) return;
                _editMode = EditModes.Connections;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeMaterial));
                OnPropertyChanged(nameof(IsEditModeFilters));
                OnPropertyChanged(nameof(IsEditModeScene));
            }
        }

        public bool IsEditModeScene {
            get => _editMode == EditModes.Scene;
            set {
                if (!value) return;
                _editMode = EditModes.Scene;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditModeMaterial));
                OnPropertyChanged(nameof(IsEditModeFilters));
                OnPropertyChanged(nameof(IsEditModeConnections));
            }
        }

        public bool IsPreviewTabSelected {
            get => _isPreviewTabSelected;
            set {
                if (_isPreviewTabSelected == value) return;
                _isPreviewTabSelected = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedTab));

                if (_isPreviewTabSelected) {
                    OnPropertyChanged(nameof(SelectedTab));
                    OnPropertyChanged(nameof(SelectedTabMaterial));
                    OnPropertyChanged(nameof(HasSelectedMaterial));
                }
            }
        }
        
        public ITabModel TabListSelection {
            get => _tabListSelection;
            set {
                if (_tabListSelection == value) return;
                _tabListSelection = value;

                if (value != null && _isPreviewTabSelected)
                    _isPreviewTabSelected = false;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedMaterial));
                OnPropertyChanged(nameof(IsPreviewTabSelected));
                OnPropertyChanged(nameof(HasSelectedTab));

                if (!_isPreviewTabSelected) {
                    OnPropertyChanged(nameof(SelectedTab));
                    OnPropertyChanged(nameof(SelectedTabMaterial));
                    OnPropertyChanged(nameof(HasSelectedMaterial));
                }

                OnSelectedTabChanged();
            }
        }
        
        public string ProjectFilename {
            get => _projectFilename;
            set {
                _projectFilename = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(IsProjectLoaded));
            }
        }
        
        public List<LocationDisplayModel> PublishLocations {
            get => _publishLocations;
            private set {
                _publishLocations = value;
                OnPropertyChanged();
            }
        }

        public ContentTreeNode TreeRoot {
            get => _treeRoot;
            private set {
                _treeRoot = value;
                OnPropertyChanged();
            }
        }

        public string SearchText {
            get => _searchText;
            set {
                _searchText = value;
                OnPropertyChanged();

                TreeRoot.UpdateVisibility(this);
            }
        }

        public bool ShowAllFiles {
            get => _showAllFiles;
            set {
                _showAllFiles = value;
                OnPropertyChanged();

                TreeRoot.UpdateVisibility(this);
            }
        }

        public ContentTreeNode SelectedNode {
            get => _selectedNode;
            set {
                _selectedNode = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(HasTreeSelection));
                NotifyTreeSelectionChanged();
            }
        }

        public LocationDisplayModel SelectedLocation {
            get => _selectedLocation;
            set {
                _selectedLocation = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy {
            get => _isBusy || _isInitializing;
            set {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public bool IsImageEditorOpen {
            get => _isImageEditorOpen;
            private set {
                _isImageEditorOpen = value;
                OnPropertyChanged();
            }
        }


        public MainWindowViewModel()
        {
            _publishLocations = new List<LocationDisplayModel>();
            _treeRoot = new ContentTreeNode(null);
            busyLock = new object();

            TabList = new ObservableCollection<ITabModel>();
            ProfileList = new ObservableCollection<PublishProfileDisplayRow>();
            TabCloseButtonCommand = new ActionCommand(OnTabCloseButtonClicked);

            _isInitializing = true;
            _selectedTag = TextureTags.Color;

#if !NORENDER
            SceneProperties = new ScenePropertiesModel();
            RenderProperties = new RenderPropertiesModel();
#endif

            TextureModel = new TexturePreviewModel();
        }

        public void Initialize(IServiceProvider provider)
        {
            _provider = provider;

            logger = provider.GetRequiredService<ILogger<MainWindowViewModel>>();
            appSettingsMgr = provider.GetRequiredService<IAppSettingsManager>();
            tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            publishLocationMgr = provider.GetRequiredService<IPublishLocationManager>();
            materialCache = provider.GetRequiredService<MaterialPropertiesCache>();
            editUtility = provider.GetRequiredService<TextureEditUtility>();

            LoadAppSettings();
        }

        public void Initialize()
        {
            UpdatePublishLocations();

            if (appSettingsMgr.Data.SelectedPublishLocation != null) {
                var location = PublishLocations.FirstOrDefault(x => string.Equals(x.DisplayName, appSettingsMgr.Data.SelectedPublishLocation, StringComparison.InvariantCultureIgnoreCase));
                if (location != null) SelectedLocation = location;
            }
        }

        public void EndInit()
        {
            _isInitializing = false;
            OnPropertyChanged(nameof(IsInitializing));
            OnPropertyChanged(nameof(IsBusy));
        }

        public bool TryStartBusy()
        {
            lock (busyLock) {
                if (_isBusy) return false;
                _isBusy = true;
            }

            OnPropertyChanged(nameof(IsBusy));
            return true;
        }

        public void EndBusy()
        {
            lock (busyLock) {
                _isBusy = false;
            }

            OnPropertyChanged(nameof(IsBusy));
        }

        private void NotifyTreeSelectionChanged()
        {
            OnPropertyChanged(nameof(HasTreeMaterialSelection));
            OnPropertyChanged(nameof(HasTreeTextureSelection));
        }

        private void OnTabCloseButtonClicked(object parameter)
        {
            if (parameter is Guid tabId)
                CloseTab(tabId);
        }

        private void OnSelectedTabChanged()
        {
            var tab = SelectedTab;
            if (tab == null) return;

            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) throw new ApplicationException($"Tab context not found! id={SelectedTab.Id}");

#if !NORENDER
            if (tab is MaterialTabModel matTab) {
                var mat = matTab.MaterialRegistration.Value;
                RenderProperties.ApplyMaterial(mat);
            }

            RenderProperties.MeshParts = context.Mesh.ModelParts;
#endif

            TextureModel.Texture = context.GetLayerImageSource();

            SelectedTabChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedTagChanged()
        {
            SelectedTagChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnViewModeChanged()
        {
            ViewModeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedProfileChanged()
        {
            if (_isInitializing || isUpdatingProfiles) return;

            var context = projectContextMgr.GetContext();
            if (context != null) context.SelectedProfile = SelectedProfile?.Profile;

            SelectedProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            projectContextMgr.SetContext(null);

            ProfileList?.Clear();
            ProjectFilename = null;
            SelectedNode = null;
            TreeRoot = null;

            materialCache.Clear();
        }

        public void CloseAllTabs()
        {
            tabPreviewMgr.Clear();

            IsPreviewTabSelected = false;
            TabListSelection = null;
            PreviewTab = null;
            TabList.Clear();
        }

        public async Task LoadProjectAsync(string filename)
        {
            var serializer = new ProjectSerializer();
            var project = await serializer.LoadAsync(filename);

            var context = new ProjectContext {
                Project = project,
                ProjectFilename = filename,
                RootDirectory = Path.GetDirectoryName(filename),
            };

            projectContextMgr.SetContext(context);
        }

        public async Task LoadRootDirectoryAsync(Dispatcher dispatcher)
        {
            if (!TryStartBusy()) return;

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.Services.AddSingleton<ContentTreeReader>();
            
            await using var scope = serviceBuilder.Build();

            var loader = scope.GetRequiredService<IPublishReader>();
            var treeReader = scope.GetRequiredService<ContentTreeReader>();

            try {
                _isInitializing = true;
                loader.EnableAutoMaterial = projectContext.Project.Input?.AutoMaterial
                                            ?? PackInputEncoding.AutoMaterialDefault;

                await dispatcher.BeginInvoke(() => {
                    UpdatePublishProfiles();

                    TreeRoot = new ContentTreeDirectory(null) {
                        LocalPath = null,
                    };

                    try {
                        treeReader.Update(TreeRoot);
                    }
                    catch (Exception error) {
                        logger.LogError(error, "Failed to populate TreeView!");
                        OnTreeError(error);
                    }

                    TreeRoot.UpdateVisibility(this);

                    // TODO: add 'selected-profile' to project to restore last selection?
                    projectContext.SelectedProfile = projectContext.Project.Profiles.FirstOrDefault();

                    SelectedProfile = ProfileList.FirstOrDefault(p => p.Profile == projectContext.SelectedProfile);
                    EndBusy();
                });
            }
            catch {
                await dispatcher.BeginInvoke(EndBusy);
                throw;
            }
            finally {
                _isInitializing = false;
            }
        }

        public void ReloadContent()
        {
            if (!TryStartBusy()) return;

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.Services.AddSingleton<ContentTreeReader>();

            using var scope = serviceBuilder.Build();

            var loader = scope.GetRequiredService<IPublishReader>();
            var treeReader = scope.GetRequiredService<ContentTreeReader>();

            try {
                loader.EnableAutoMaterial = projectContext.Project.Input.AutoMaterial ?? PackInputEncoding.AutoMaterialDefault;

                treeReader.Update(TreeRoot);
                TreeRoot.UpdateVisibility(this);
            }
            finally {
                EndBusy();
            }
        }

        public void LoadAppSettings()
        {
            IsRenderPreviewEnabled = appSettingsMgr.Data.RenderPreview?.Enabled ?? RenderPreviewSettings.Default_Enabled;
        }

        public void ClearPreviewTab()
        {
            if (PreviewTab == null) return;

            if (PreviewTab is MaterialTabModel materialTab && materialTab.MaterialRegistration != null)
                materialCache.Release(materialTab.MaterialRegistration);

            tabPreviewMgr.Remove(PreviewTab.Id);
            PreviewTab = null;
            IsPreviewTabSelected = false;
        }

        public void SetPreviewTab(ITabModel newTab)
        {
            if (PreviewTab != null)
                ClearPreviewTab();

            var context = new TabPreviewContext(_provider) {
                Id = newTab.Id,
            };

            tabPreviewMgr.Add(context);
            IsPreviewTabSelected = true;
            PreviewTab = newTab;
            TabListSelection = null;
        }

        public void AddNewTab(ITabModel newTab)
        {
            var context = new TabPreviewContext(_provider) {
                Id = newTab.Id,
            };

            tabPreviewMgr.Add(context);
            TabList.Add(newTab);
            TabListSelection = newTab;
        }

        public bool HasAcceptedPatreonNotification()
        {
            return appSettingsMgr.Data.HasAcceptedPatreonNotification ?? false;
        }

        public bool HasAcceptedLicenseAgreement()
        {
            return appSettingsMgr.Data.AcceptedLicenseAgreementVersion == AppSettingsDataModel.CurrentLicenseVersion;
        }

        public bool HasAcceptedTermsOfService()
        {
            return appSettingsMgr.Data.AcceptedTermsOfServiceVersion == AppSettingsDataModel.CurrentTermsVersion;
        }

        public void UpdatePublishLocations()
        {
            PublishLocations = publishLocationMgr.GetLocations()
                .Select(l => new LocationDisplayModel(l)).ToList();

            // TODO: this should probably be lock-wrapped as well...
            SelectedLocation = PublishLocations.FirstOrDefault(l =>
                string.Equals(l.DisplayName, publishLocationMgr.SelectedLocation, StringComparison.InvariantCultureIgnoreCase));
        }

        private volatile bool isUpdatingProfiles;

        public void UpdatePublishProfiles()
        {
            var projectContext = projectContextMgr.GetContext();

            isUpdatingProfiles = true;

            try {
                ProfileList.Clear();

                foreach (var profile in projectContext.Project.Profiles)
                    ProfileList.Add(new PublishProfileDisplayRow(profile) {
                        DefaultName = projectContext.Project.Name,
                    });

                SelectedProfile = ProfileList.FirstOrDefault(p => p.Profile == projectContext.SelectedProfile);
            }
            finally {
                isUpdatingProfiles = false;
            }
        }

        public async Task UpdateTabPreviewAsync(Dispatcher dispatcher, CancellationToken token = default)
        {
            var tab = SelectedTab;
            if (tab == null) return;

            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) return;

            try {
                await dispatcher.BeginInvoke(() => tab.IsLoading = true);
                await Task.Run(() => UpdateTabPreviewAsync(dispatcher, context, token), token);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to update tab preview!");
            }
            finally {
                await dispatcher.BeginInvoke(() => tab.IsLoading = false);
            }
        }

        private async Task UpdateTabPreviewAsync(Dispatcher dispatcher, TabPreviewContext context, CancellationToken token)
        {
            try {
                if (SelectedTab is MaterialTabModel materialTab) {
                    var material = materialTab.MaterialRegistration.Value;
                    if (material == null) return;

#if !NORENDER
                    if (IsRenderPreviewOn) {
                        if (!context.IsMaterialBuilderValid) {
                            try {
                                var renderContext = BuildRenderContext();
                                await context.BuildModelMeshAsync(renderContext, token);
                            }
                            catch (Exception error) {
                                logger.LogError(error, "Failed to build model mesh!");
                                // TODO: show error modal!
                                return;
                            }
                        }

                        await dispatcher.BeginInvoke(() => {
                            if (SelectedTab == null || SelectedTab.Id != context.Id) return;

                            //RenderModel.MeshParts.Clear();

                            //var srcMeshParts = context.UpdateModel(Model);
                            //foreach (var (textureId, partData) in srcMeshParts) {
                            //    var part = new BlockMeshGeometryModel3D();
                            //    part.Geometry = partData.Geometry;

                            //    // TODO: load actual material textures
                            //    part.Material = material;

                            //    RenderModel.MeshParts.Add(part);
                            //}

                            //RenderModel.BlockMesh = context.UpdateModel(Model);
                            //RenderModel.ModelMaterial = context.UpdateMaterial(Model, SceneModel, RenderModel);
                            if (!context.IsMaterialValid) context.UpdateModelParts();

                            RenderProperties.MeshParts = context.Mesh.ModelParts;
                            //RenderModel.SetModel();
                            
                            TextureModel.Texture = null;
                        });
                    }
#endif
                    if (!IsRenderPreviewOn) {
                        if (!context.IsLayerValid) {
                            var image = await Task.Run(async () => {
                                using var previewBuilder = _provider.GetRequiredService<ILayerPreviewBuilder>();
                                var projectContext = projectContextMgr.GetContext();

                                previewBuilder.Project = projectContext.Project;
                                previewBuilder.Profile = projectContext.SelectedProfile;
                                previewBuilder.Material = material;
                                previewBuilder.TargetFrame = 0;

                                var tag = SelectedTag;
                                if (TextureTags.Is(tag, TextureTags.General))
                                    tag = TextureTags.Color;

                                return await previewBuilder.BuildAsync(tag, token);

                                //return await context.BuildLayerAsync(packContext, material, Model.SelectedTag, token);
                            }, token);

                            await dispatcher.BeginInvoke(() => context.SetImageSource(image));
                        }

                        //if (context.IsLayerValid) return;
                        
                        await dispatcher.BeginInvoke(() => {
                            //context.SetImageSource(image);

                            if (SelectedTab == null || SelectedTab.Id != context.Id) return;

#if !NORENDER
                            RenderProperties.MeshParts = null;
#endif

                            TextureModel.Texture = context.GetLayerImageSource();
                        });
                    }
                }

                if (SelectedTab is TextureTabModel textureTab) {
                    if (context.LayerImage != null) return;

                    var projectContext = projectContextMgr.GetContext();
                    context.SourceFile = PathEx.Join(projectContext.RootDirectory, textureTab.ImageFilename);

                    await dispatcher.BeginInvoke(() => {
                        if (SelectedTab == null || SelectedTab.Id != context.Id) return;

#if !NORENDER
                        RenderProperties.MeshParts = null;
#endif

                        TextureModel.Texture = context.GetLayerImageSource();
                    });
                }
            }
            catch (OperationCanceledException) {}
        }

        public async Task GenerateNormalAsync(MaterialProperties material, string filename, CancellationToken token = default)
        {
            var projectContext = projectContextMgr.GetContext();

            var inputFormat = TextureFormat.GetFactory(projectContext.Project.Input.Format);
            var inputEncoding = inputFormat?.Create() ?? new PackEncoding();
            inputEncoding.Merge(projectContext.Project.Input);
            inputEncoding.Merge(material);

            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

            await using var scope = serviceBuilder.Build();

            var context = scope.GetRequiredService<ITextureGraphContext>();
            var graph = scope.GetRequiredService<ITextureNormalGraph>();
            var reader = scope.GetRequiredService<IInputReader>();

            context.Project = (IProjectDescription)projectContext.Project.Clone();
            context.Material = material;

            var matMetaFileIn = NamingStructure.GetInputMetaName(material);
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            context.InputEncoding = inputEncoding.GetMapped().ToList();
            context.OutputEncoding = inputEncoding.GetMapped().ToList();

            using var normalImage = await graph.GenerateAsync(token);
            await normalImage.SaveAsync(filename, token);
        }

        public async Task GenerateOcclusionAsync(MaterialProperties material, string filename, CancellationToken token = default)
        {
            var projectContext = projectContextMgr.GetContext();

            var inputFormat = TextureFormat.GetFactory(projectContext.Project.Input.Format);
            var inputEncoding = inputFormat?.Create() ?? new PackEncoding();
            inputEncoding.Merge(projectContext.Project.Input);
            inputEncoding.Merge(material);

            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            
            await using var scope = serviceBuilder.Build();

            var context = scope.GetRequiredService<ITextureGraphContext>();
            var graph = scope.GetRequiredService<ITextureOcclusionGraph>();
            var reader = scope.GetRequiredService<IInputReader>();

            context.Project = (IProjectDescription)projectContext.Project.Clone();
            context.Profile = projectContext.SelectedProfile;
            context.Material = material;

            var matMetaFileIn = NamingStructure.GetInputMetaName(material);
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            context.InputEncoding = inputEncoding.GetMapped().ToList();
            context.OutputEncoding = inputEncoding.GetMapped().ToList();

            using var occlusionImage = await graph.GenerateAsync(token);

            if (occlusionImage == null)
                throw new ApplicationException("Unable to generate occlusion texture!");

            // WARN: This only allows separate images!
            // TODO: Support writing to the channel of an existing image?
            // This could cause issues if the existing image is not the correct size
            await occlusionImage.SaveAsync(filename, token);

            var inputChannel = context.InputEncoding.FirstOrDefault(c => EncodingChannel.Is(c.ID, EncodingChannel.Occlusion));
            
            if (inputChannel != null && !TextureTags.Is(inputChannel.Texture, TextureTags.Occlusion)) {
                //material.Occlusion ??= new MaterialOcclusionProperties();
                material.Occlusion.Texture = Path.GetFileName(filename);

                material.Occlusion.Input ??= new ResourcePackOcclusionChannelProperties();
                material.Occlusion.Input.Texture = TextureTags.Occlusion;
                material.Occlusion.Input.Invert = true;

                await SaveMaterialAsync(material);
            }
        }

        public async Task SaveMaterialAsync(MaterialProperties material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();
            
            serviceBuilder.Initialize();
            serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

            await using var scope = serviceBuilder.Build();

            try {
                var matWriter = scope.GetRequiredService<IMaterialWriter>();
                await matWriter.WriteAsync(material);
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to save material '{material.LocalFilename}'!", error);
            }
        }

        public async Task<MaterialProperties> ImportTextureAsync(string filename, CancellationToken token = default)
        {
            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();
            
            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

            await using var scope = serviceBuilder.Build();

            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var matWriter = scope.GetRequiredService<IMaterialWriter>();

            var itemName = Path.GetFileNameWithoutExtension(filename);
            var localPath = Path.GetDirectoryName(filename);
            var matFile = PathEx.Join(localPath, itemName, "mat.yml");

            var material = new MaterialProperties {
                LocalFilename = matFile,
                LocalPath = localPath,
                //...
            };

            await matWriter.WriteAsync(material, token);

            var ext = Path.GetExtension(filename);
            var destFile = PathEx.Join(localPath, itemName, $"albedo{ext}");
            await using (var sourceStream = reader.Open(filename)) {
                await writer.OpenWriteAsync(destFile, async destStream => {
                    await sourceStream.CopyToAsync(destStream, token);
                }, token);
            }

            writer.Delete(filename);
            return material;
        }

        public async Task LoadTabContentAsync(ITabModel tabModel, CancellationToken token = default)
        {
            if (tabModel is MaterialTabModel materialTab) {
                materialTab.MaterialRegistration = await materialCache.RegisterAsync(materialTab.MaterialFilename, token);
            }
        }

        private void CloseTab(Guid tabId)
        {
            ITabModel tab = null;
            if (PreviewTab?.Id == tabId) {
                tab = PreviewTab;
                PreviewTab = null;
            }
            else {
                for (var i = TabList.Count - 1; i >= 0; i--) {
                    if (TabList[i].Id != tabId) continue;

                    tab = TabList[i];
                    TabList.RemoveAt(i);
                    break;
                }
            }

            if (tab is MaterialTabModel materialTab && materialTab.MaterialRegistration != null)
                materialCache.Release(materialTab.MaterialRegistration);

            tabPreviewMgr.Remove(tabId);
        }

        public void InvalidateTab(Guid tabId)
        {
            var context = tabPreviewMgr.Get(tabId);
            if (context == null) return;

            context.InvalidateLayer(false);

#if !NORENDER
            context.InvalidateMaterialBuilder(false);
#endif
        }

        public void InvalidateTabLayer()
        {
            if (SelectedTab == null) return;
            tabPreviewMgr.Get(SelectedTab.Id)?.InvalidateLayer(false);
        }

        public void InvalidateTabChannels(Guid tabId, string[] channels)
        {
            var context = tabPreviewMgr.Get(tabId);
            if (context == null) return;

            context.InvalidateLayer(false);

#if !NORENDER
            context.InvalidateMaterialBuilder(channels, false);
#endif
        }

        public void InvalidateAllTabs()
        {
            tabPreviewMgr.InvalidateAll(true);
        }

        public void InvalidateAllTabLayers()
        {
            tabPreviewMgr.InvalidateAllLayers(true);
        }

#if !NORENDER
        public void UpdateMaterials()
        {
            var renderContext = BuildRenderContext();

            foreach (var tab in tabPreviewMgr.All) {
                tab.UpdateMaterials(renderContext);
            }
        }

        private RenderContext BuildRenderContext()
        {
            var projectContext = projectContextMgr.GetContext();

            return new RenderContext {
                RenderMode = RenderProperties.RenderMode,
                Project = projectContext?.Project,
                PackProfile = projectContext?.SelectedProfile,
                DefaultMaterial = SelectedTabMaterial,
                MissingMaterial = RenderProperties.MissingMaterial,
                DielectricBrdfLutMap = RenderProperties.DielectricBrdfLutMap,
                IrradianceCubeMap = RenderProperties.IrradianceCube,
                EnvironmentEnabled = SceneProperties.EnableAtmosphere || SceneProperties.EquirectangularMap != null,
                EnableLinearSampling = SceneProperties.PomType?.EnableLinearSampling ?? false,
                EnableTiling = RenderProperties.EnableTiling,

                EnvironmentCubeMap = SceneProperties.EnableAtmosphere
                    ? RenderProperties.DynamicSkyCubeSource
                    : RenderProperties.ErpCubeSource,
            };
        }
#endif

        #region External Image Editing

        public async Task BeginExternalEditAsync(CancellationToken token = default)
        {
            if (SelectedTab is not MaterialTabModel materialTab) return;
            
            var selectedMaterial = materialTab.MaterialRegistration.Value;
            if (selectedMaterial == null) return;

            if (!HasSelectedTag) return;

            try {
                IsImageEditorOpen = true;

                var success = await editUtility.EditLayerAsync(selectedMaterial, SelectedTag, token);
                if (!success) return;
            }
            catch (Exception error) {
                //logger.LogError(error, "Failed to launch external image editor!");
                //ShowError($"Failed to launch external image editor! {error.UnfoldMessageString()}");
                throw new ApplicationException("Failed to launch external image editor!", error);
            }
            finally {
                IsImageEditorOpen = false;
            }
        }

        public void CancelExternalImageEdit()
        {
            editUtility.Cancel();
            IsImageEditorOpen = false;
        }

        #endregion

        private void OnTreeError(Exception error)
        {
            var e = new UnhandledExceptionEventArgs(error, false);
            TreeError?.Invoke(this, e);
        }
    }

    internal class MainWindowDesignerViewModel : MainWindowViewModel
    {
        public MainWindowDesignerViewModel()
        {
            AddTreeItems();

            ProjectFilename = "x:\\dev\\test-rp\\project.yml";
            //IsBusy = true;
            SearchText = "as";
            ShowAllFiles = true;
            //IsPreviewLoading = true;

            PreviewTab = new MaterialTabModel {
                DisplayName = "Bricks",
                IsPreview = true,
            };
        }

        private void AddTreeItems()
        {
            IsPreviewTabSelected = true;
            PreviewTab = new MaterialTabModel {
                DisplayName = "Test Material",
            };

            TreeRoot.Nodes.Add(new ContentTreeDirectory(null) {
                Name = "assets",
                Nodes = {
                    new ContentTreeDirectory(null) {
                        Name = "minecraft",
                        Nodes = {
                            new ContentTreeFile(null) {
                                Name = "Dirt",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                            new ContentTreeFile(null) {
                                Name = "Grass",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                            new ContentTreeFile(null) {
                                Name = "Glass",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                            new ContentTreeFile(null) {
                                Name = "Stone",
                                Type = ContentNodeType.Texture,
                                Icon = PackIconFontAwesomeKind.ImageSolid,
                            },
                        },
                    },
                },
            });
        }
    }
}
