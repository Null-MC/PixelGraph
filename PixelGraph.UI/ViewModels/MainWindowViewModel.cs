﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
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
using SixLabors.ImageSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

#if !NORENDER
using PixelGraph.Rendering.Models;
using PixelGraph.UI.Models.Scene;
#endif

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowViewModel
    {
        private readonly ILogger<MainWindowViewModel> logger;
        private readonly IServiceProvider provider;
        private readonly IRecentPathManager recentMgr;
        private readonly ITabPreviewManager tabPreviewMgr;
        private readonly IProjectContextManager projectContextMgr;
        private readonly IPublishLocationManager publishLocationMgr;
        private readonly MaterialPropertiesCache materialCache;
        private readonly TextureEditUtility editUtility;

        public event EventHandler<UnhandledExceptionEventArgs> TreeError;

        public MainWindowModel Model {get; set;}
        public TexturePreviewModel TextureModel {get; set;}
        public Dispatcher Dispatcher {get; set;}

#if !NORENDER
        public ScenePropertiesModel SceneProperties {get; set;}
        public RenderPropertiesModel RenderProperties {get; set;}
#endif


        public MainWindowViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<MainWindowViewModel>>();
            recentMgr = provider.GetRequiredService<IRecentPathManager>();
            tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            publishLocationMgr = provider.GetRequiredService<IPublishLocationManager>();
            materialCache = provider.GetRequiredService<MaterialPropertiesCache>();
            editUtility = provider.GetRequiredService<TextureEditUtility>();
        }

        public void Initialize()
        {
            UpdatePublishLocations();

            var settings = provider.GetRequiredService<IAppSettings>();

            if (settings.Data.SelectedPublishLocation != null) {
                var location = Model.PublishLocations.FirstOrDefault(x => string.Equals(x.DisplayName, settings.Data.SelectedPublishLocation, StringComparison.InvariantCultureIgnoreCase));
                if (location != null) Model.SelectedLocation = location;
            }

            UpdateRecentProjectsList();

            Model.SelectedTabChanged += OnSelectedTabChanged;
            Model.SelectedTagChanged += OnSelectedTagChanged;
            Model.ViewModeChanged += OnViewModeChanged;
            Model.TabClosed += OnTabClosed;

#if !NORENDER
            RenderProperties.RenderModeChanged += OnRenderModeChanged;
#endif
        }

        public void Clear()
        {
            projectContextMgr.SetContext(null);

            Model.ProfileList?.Clear();
            Model.ProjectFilename = null;
            Model.SelectedNode = null;
            Model.TreeRoot = null;

            materialCache.Clear();
        }

        public void CloseAllTabs()
        {
            tabPreviewMgr.Clear();

            Model.IsPreviewTabSelected = false;
            Model.TabListSelection = null;
            Model.PreviewTab = null;
            Model.TabList.Clear();
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

        public Task AppendRecentProject(string filename, CancellationToken token = default)
        {
            recentMgr.Insert(filename);
            return recentMgr.SaveAsync(token);
        }

        public async Task SetRootDirectoryAsync(CancellationToken token = default)
        {
            //await LoadRootDirectoryAsync();

            await Dispatcher.BeginInvoke(UpdateRecentProjectsList);
        }

        public async Task LoadRootDirectoryAsync()
        {
            if (!Model.TryStartBusy()) return;

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.Services.AddSingleton<ContentTreeReader>();
            
            await using var scope = serviceBuilder.Build();

            //var reader = scope.GetRequiredService<IInputReader>();
            var loader = scope.GetRequiredService<IPublishReader>();
            var treeReader = scope.GetRequiredService<ContentTreeReader>();

            try {
                //try {
                //    await LoadPackInputAsync();
                //}
                //catch (Exception error) {
                //    throw new ApplicationException("Failed to load pack input definitions!", error);
                //}

                loader.EnableAutoMaterial = projectContext.Project.Input?.AutoMaterial
                    ?? ResourcePackInputProperties.AutoMaterialDefault;
                
                await Dispatcher.BeginInvoke(() => {
                    UpdatePublishProfiles();

                    Model.TreeRoot = new ContentTreeDirectory(null) {
                        LocalPath = null,
                    };

                    try {
                        treeReader.Update(Model.TreeRoot);
                    }
                    catch (Exception error) {
                        logger.LogError(error, "Failed to populate TreeView!");
                        OnTreeError(error);
                    }

                    Model.TreeRoot.UpdateVisibility(Model);

                    // TODO: add 'selected-profile' to project to restore last selection?
                    Model.SelectedProfile = projectContext.SelectedProfile = projectContext.Project.Profiles.FirstOrDefault();
                    Model.EndBusy();
                });
            }
            catch {
                await Dispatcher.BeginInvoke(() => {
                    Model.EndBusy();
                });

                throw;
            }
        }

        public void ReloadContent()
        {
            if (!Model.TryStartBusy()) return;

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.Services.AddSingleton<ContentTreeReader>();

            using var scope = serviceBuilder.Build();

            var loader = scope.GetRequiredService<IPublishReader>();
            var treeReader = scope.GetRequiredService<ContentTreeReader>();

            try {
                loader.EnableAutoMaterial = projectContext.Project.Input.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;

                treeReader.Update(Model.TreeRoot);
                Model.TreeRoot.UpdateVisibility(Model);
            }
            finally {
                Model.EndBusy();
            }
        }

        public void ClearPreviewTab()
        {
            if (Model.PreviewTab == null) return;

            if (Model.PreviewTab is MaterialTabModel materialTab && materialTab.MaterialRegistration != null)
                materialCache.Release(materialTab.MaterialRegistration);

            tabPreviewMgr.Remove(Model.PreviewTab.Id);
            Model.PreviewTab = null;
            Model.IsPreviewTabSelected = false;
        }

        public void SetPreviewTab(ITabModel newTab)
        {
            if (Model.PreviewTab != null)
                ClearPreviewTab();

            var context = new TabPreviewContext(provider) {
                Id = newTab.Id,
            };

            tabPreviewMgr.Add(context);
            Model.IsPreviewTabSelected = true;
            Model.PreviewTab = newTab;
            Model.TabListSelection = null;
        }

        public void AddNewTab(ITabModel newTab)
        {
            var context = new TabPreviewContext(provider) {
                Id = newTab.Id,
            };

            tabPreviewMgr.Add(context);
            Model.TabList.Add(newTab);
            Model.TabListSelection = newTab;
        }

        public void UpdatePublishLocations()
        {
            Model.PublishLocations = publishLocationMgr.GetLocations()
                .Select(l => new LocationDisplayModel(l)).ToList();

            // TODO: this should probably be lock-wrapped as well...
            Model.SelectedLocation = Model.PublishLocations.FirstOrDefault(l =>
                string.Equals(l.DisplayName, publishLocationMgr.SelectedLocation, StringComparison.InvariantCultureIgnoreCase));
        }

        public void UpdatePublishProfiles()
        {
            var projectContext = projectContextMgr.GetContext();
            Model.ProfileList = new ObservableCollection<ResourcePackProfileProperties>(projectContext.Project.Profiles);
            Model.SelectedProfile = projectContext.SelectedProfile;
        }

        //public async Task UpdateSelectedProfileAsync()
        //{
        //    if (!Model.Profile.HasSelection) {
        //        Model.Profile.Loaded = null;
        //        return;
        //    }

        //    var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

        //    serviceBuilder.Initialize();
        //    serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContextMgr.RootDirectory);

        //    await using var scope = serviceBuilder.Build();

        //    var packReader = scope.GetRequiredService<IResourcePackReader>();
        //    var profile = await packReader.ReadProfileAsync(Model.Profile.Selected.LocalFile);

        //    await Dispatcher.BeginInvoke(() => Model.Profile.Loaded = profile);
        //}

        //public async Task LoadPublishLocationsAsync(CancellationToken token = default)
        //{
        //    var locationMgr = provider.GetRequiredService<IPublishLocationManager>();

        //    publishLocationList = await locationMgr.LoadAsync(token);
        //}

        public async Task LoadRecentProjectsAsync()
        {
            try {
                await recentMgr.LoadAsync();
            }
            catch (Exception error) {
                throw new ApplicationException("Failed to load recent projects list!", error);
            }
        }

        public async Task UpdateTabPreviewAsync(CancellationToken token = default)
        {
            var tab = Model.SelectedTab;
            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) return;

            try {
                tab.IsLoading = true;
                await UpdateTabPreviewAsync(context, token);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to update tab preview!");
            }
            finally {
                await Dispatcher.BeginInvoke(() => tab.IsLoading = false);
            }
        }

        private async Task UpdateTabPreviewAsync(TabPreviewContext context, CancellationToken token)
        {
            try {
                if (Model.SelectedTab is MaterialTabModel materialTab) {
                    var material = materialTab.MaterialRegistration.Value;
                    if (material == null || context.IsMaterialValid) return;

#if !NORENDER
                    if (Model.IsViewModeRender) {
                        //if (!context.IsMaterialBuilderValid)
                        //    await context.BuildModelMeshAsync(material, token);

                        try {
                            var renderContext = BuildRenderContext();
                            await context.BuildModelMeshAsync(renderContext, token);
                        }
                        catch (Exception error) {
                            logger.LogError(error, "Failed to build model mesh!");
                            // TODO: show error modal!
                        }

                        await Dispatcher.BeginInvoke(() => {
                            if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

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
                            context.UpdateModelParts();
                            RenderProperties.MeshParts = context.Mesh.ModelParts;
                            //RenderModel.SetModel();
                            
                            TextureModel.Texture = null;
                        });
                    }
#endif
                    if (!Model.IsViewModeRender) {
                        if (context.IsLayerValid) return;

                        //var packContext = new ResourcePackContext {
                        //    //RootPath = projectContext.RootDirectory,
                        //    Input = Model.PackInput,
                        //    Profile = Model.Profile.Loaded,
                        //};

                        var image = await Task.Run(async () => {
                            using var previewBuilder = provider.GetRequiredService<ILayerPreviewBuilder>();
                            var projectContext = projectContextMgr.GetContext();

                            //previewBuilder.RootDirectory = context.RootPath;
                            previewBuilder.Input = projectContext.Project.Input;
                            previewBuilder.Profile = projectContext.SelectedProfile;
                            previewBuilder.Material = material;
                            previewBuilder.TargetFrame = 0;

                            var tag = Model.SelectedTag;
                            if (TextureTags.Is(tag, TextureTags.General))
                                tag = TextureTags.Color;

                            return await previewBuilder.BuildAsync(tag, token);

                            //return await context.BuildLayerAsync(packContext, material, Model.SelectedTag, token);
                        }, token);

                        await Dispatcher.BeginInvoke(() => {
                            context.SetImageSource(image);

                            if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

#if !NORENDER
                            //RenderModel.ModelMaterial = null;
                            //RenderModel.BlockMesh = null;
                            RenderProperties.MeshParts.Clear();
#endif

                            TextureModel.Texture = context.GetLayerImageSource();
                        });
                    }
                }

                if (Model.SelectedTab is TextureTabModel textureTab) {
                    if (context.LayerImage != null) return;

                    var projectContext = projectContextMgr.GetContext();
                    context.SourceFile = PathEx.Join(projectContext.RootDirectory, textureTab.ImageFilename);

                    await Dispatcher.BeginInvoke(() => {
                        if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

#if !NORENDER
                        //RenderModel.ModelMaterial = null;
                        //RenderModel.BlockMesh = null;
                        RenderProperties.MeshParts.Clear();
#endif

                        TextureModel.Texture = context.GetLayerImageSource();
                    });
                }
            }
            catch (OperationCanceledException) {}
        }

        public async Task GenerateNormalAsync(MaterialProperties material, string filename, CancellationToken token = default)
        {
            if (!Model.TryStartBusy()) return;

            var projectContext = projectContextMgr.GetContext();

            try {
                var inputFormat = TextureFormat.GetFactory(projectContext.Project.Input.Format);
                var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                inputEncoding.Merge(projectContext.Project.Input);
                inputEncoding.Merge(material);

                await Task.Factory.StartNew(async () => {
                    var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                    serviceBuilder.Initialize();
                    serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

                    await using var scope = serviceBuilder.Build();

                    var context = scope.GetRequiredService<ITextureGraphContext>();
                    var graph = scope.GetRequiredService<ITextureNormalGraph>();
                    var reader = scope.GetRequiredService<IInputReader>();

                    context.Input = projectContext.Project.Input;
                    context.Material = material;

                    var matMetaFileIn = NamingStructure.GetInputMetaName(material);
                    context.IsAnimated = reader.FileExists(matMetaFileIn);

                    context.InputEncoding = inputEncoding.GetMapped().ToList();
                    context.OutputEncoding = inputEncoding.GetMapped().ToList();

                    using var normalImage = await graph.GenerateAsync(token);
                    await normalImage.SaveAsync(filename, token);
                }, token);
            }
            finally {
                Model.EndBusy();
            }
        }

        public async Task GenerateOcclusionAsync(MaterialProperties material, string filename, CancellationToken token = default)
        {
            if (!Model.TryStartBusy()) return;

            var projectContext = projectContextMgr.GetContext();

            try {
                var inputFormat = TextureFormat.GetFactory(projectContext.Project.Input.Format);
                var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                inputEncoding.Merge(projectContext.Project.Input);
                inputEncoding.Merge(material);

                await Task.Factory.StartNew(async () => {
                    var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

                    serviceBuilder.Initialize();
                    serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
                    
                    await using var scope = serviceBuilder.Build();

                    var context = scope.GetRequiredService<ITextureGraphContext>();
                    var graph = scope.GetRequiredService<ITextureOcclusionGraph>();
                    var reader = scope.GetRequiredService<IInputReader>();

                    context.Input = projectContext.Project.Input;
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
                }, token);
            }
            finally {
                Model.EndBusy();
            }
        }

        public async Task SaveMaterialAsync(MaterialProperties material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            var projectContext = projectContextMgr.GetContext();
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();
            
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
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();
            
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

        public async Task RemoveRecentItemAsync(string item, CancellationToken token = default)
        {
            recentMgr.Remove(item);
            UpdateRecentProjectsList();

            await recentMgr.SaveAsync(token);
        }

        public async Task LoadTabContentAsync(ITabModel tabModel, CancellationToken token = default)
        {
            if (tabModel is MaterialTabModel materialTab) {
                materialTab.MaterialRegistration = await materialCache.RegisterAsync(materialTab.MaterialFilename, token);
            }
        }

        public void CloseTab(Guid tabId)
        {
            ITabModel tab = null;
            if (Model.PreviewTab?.Id == tabId) {
                tab = Model.PreviewTab;
                Model.PreviewTab = null;
            }
            else {
                for (var i = Model.TabList.Count - 1; i >= 0; i--) {
                    if (Model.TabList[i].Id != tabId) continue;

                    tab = Model.TabList[i];
                    Model.TabList.RemoveAt(i);
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

        public void UpdateRecentProjectsList()
        {
            Model.RecentDirectories.Clear();

            foreach (var item in recentMgr.Items)
                Model.RecentDirectories.Add(item);
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
                PackInput = projectContext.Project.Input,
                PackProfile = projectContext.SelectedProfile,
                DefaultMaterial = Model.SelectedTabMaterial,
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
            if (Model.SelectedTab is not MaterialTabModel materialTab) return;
            
            var selectedMaterial = materialTab.MaterialRegistration.Value;
            if (selectedMaterial == null) return;

            if (!Model.HasSelectedTag) return;

            try {
                Model.IsImageEditorOpen = true;

                var success = await editUtility.EditLayerAsync(selectedMaterial, Model.SelectedTag, token);
                if (!success) return;
            }
            catch (Exception error) {
                //logger.LogError(error, "Failed to launch external image editor!");
                //ShowError($"Failed to launch external image editor! {error.UnfoldMessageString()}");
                throw new ApplicationException("Failed to launch external image editor!", error);
            }
            finally {
                Model.IsImageEditorOpen = false;
            }
        }

        public void CancelExternalImageEdit()
        {
            editUtility.Cancel();
            Model.IsImageEditorOpen = false;
        }

        #endregion

        //private async Task LoadPackInputAsync()
        //{
        //    var projectContext = projectContextMgr.GetContext();
        //    var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

        //    serviceBuilder.Initialize();
        //    serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

        //    await using var scope = serviceBuilder.Build();

        //    var packReader = scope.GetRequiredService<IResourcePackReader>();

        //    var packInput = await packReader.ReadInputAsync("input.yml")
        //                    ?? new ResourcePackInputProperties {
        //                        Format = TextureFormat.Format_Raw,
        //                    };

        //    Application.Current.Dispatcher.Invoke(() => Model.PackInput = packInput);
        //}

        private void OnTreeError(Exception error)
        {
            var e = new UnhandledExceptionEventArgs(error, false);
            TreeError?.Invoke(this, e);
        }

        private void OnTabClosed(object sender, TabClosedEventArgs e)
        {
            CloseTab(e.TabId);
        }

        private async void OnSelectedTabChanged(object sender, EventArgs e)
        {
            var tab = Model.SelectedTab;
            if (tab == null) return;

            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) throw new ApplicationException($"Tab context not found! id={Model.SelectedTab.Id}");

#if !NORENDER
            if (tab is MaterialTabModel matTab) {
                var mat = matTab.MaterialRegistration.Value;
                RenderProperties.ApplyMaterial(mat);
                //RenderModel.MeshBlendMode = mat?.BlendMode;
                //RenderModel.MeshTintColor = mat?.TintColor;
            }

            RenderProperties.MeshParts = context.Mesh.ModelParts;
            //RenderModel.ModelMaterial = context.ModelMaterial;
#endif

            TextureModel.Texture = context.GetLayerImageSource();
            //tab.IsLoading = true;

            await UpdateTabPreviewAsync();
        }

        private async void OnSelectedTagChanged(object sender, EventArgs e)
        {
            tabPreviewMgr.InvalidateAllLayers(true);

            if (Model.IsViewModeRender || Model.SelectedTab == null) return;

            await UpdateTabPreviewAsync();
        }

        private async void OnViewModeChanged(object sender, EventArgs e)
        {
            var tab = Model.SelectedTab;
            if (tab == null) return;

            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) return;

            context.InvalidateLayer(false);

#if !NORENDER
            context.InvalidateMaterial(false);
#endif

            await UpdateTabPreviewAsync();
        }

#if !NORENDER
        private async void OnRenderModeChanged(object sender, EventArgs e)
        {
            if (!Model.IsViewModeRender) return;

            tabPreviewMgr.InvalidateAllMaterials(true);

            if (Model.SelectedTab != null)
                await UpdateTabPreviewAsync();
        }
#endif
    }
}
