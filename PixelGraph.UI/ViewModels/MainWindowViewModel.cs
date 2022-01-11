using Microsoft.Extensions.DependencyInjection;
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
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Tabs;
using PixelGraph.UI.ViewData;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly ITextureEditUtility editUtility;
        private readonly ITabPreviewManager tabPreviewMgr;
        private readonly IMaterialPropertiesCache materialCache;
        private LocationDataModel[] publishLocationList;

        public event EventHandler<UnhandledExceptionEventArgs> TreeError;

        public MainWindowModel Model {get; set;}
        public TexturePreviewModel TextureModel {get; set;}
        public Dispatcher Dispatcher {get; set;}

#if !NORENDER
        public ScenePropertiesModel SceneModel {get; set;}
        public RenderPreviewModel RenderModel {get; set;}
#endif


        public MainWindowViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<MainWindowViewModel>>();
            recentMgr = provider.GetRequiredService<IRecentPathManager>();
            editUtility = provider.GetRequiredService<ITextureEditUtility>();
            tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();
            materialCache = provider.GetRequiredService<IMaterialPropertiesCache>();
        }

        public void Initialize()
        {
            var settings = provider.GetRequiredService<IAppSettings>();

            if (publishLocationList != null) {
                Model.PublishLocations = publishLocationList
                    .Select(x => new LocationModel(x)).ToList();

                if (settings.Data.SelectedPublishLocation != null) {
                    var location = Model.PublishLocations.FirstOrDefault(x => string.Equals(x.DisplayName, settings.Data.SelectedPublishLocation, StringComparison.InvariantCultureIgnoreCase));
                    if (location != null) Model.SelectedLocation = location;
                }
            }

            UpdateRecentProjectsList();

            Model.SelectedTabChanged += OnSelectedTabChanged;
            Model.SelectedTagChanged += OnSelectedTagChanged;
            Model.ViewModeChanged += OnViewModeChanged;
            Model.TabClosed += OnTabClosed;

#if !NORENDER
            RenderModel.RenderModeChanged += OnRenderModeChanged;
#endif
        }

        public void Clear()
        {
            Model.SelectedNode = null;
            Model.PackInput = null;
            Model.TreeRoot = null;
            Model.RootDirectory = null;

            Model.Profile.List.Clear();
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

        public async Task SetRootDirectoryAsync(string path, CancellationToken token = default)
        {
            Model.RootDirectory = path;
            materialCache.RootDirectory = path;

            await LoadRootDirectoryAsync();

            await Dispatcher.BeginInvoke(() => {
                recentMgr.Insert(path);
                UpdateRecentProjectsList();
            });

            await recentMgr.SaveAsync(token);
        }

        public async Task LoadRootDirectoryAsync()
        {
            if (!Model.TryStartBusy()) return;

            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IPublishReader>();
            var treeReader = provider.GetRequiredService<IContentTreeReader>();

            try {
                reader.SetRoot(Model.RootDirectory);

                try {
                    await LoadPackInputAsync();
                }
                catch (Exception error) {
                    throw new ApplicationException("Failed to load pack input definitions!", error);
                }

                loader.EnableAutoMaterial = Model.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;
                
                await Dispatcher.BeginInvoke(() => {
                    try {
                        Model.Profile.List.Clear();
                        UpdateProfileList();
                    }
                    catch (Exception error) {
                        throw new ApplicationException("Failed to load pack profile definitions!", error);
                    }

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
                    Model.Profile.Selected = Model.Profile.List.FirstOrDefault();
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

            var reader = provider.GetRequiredService<IInputReader>();
            var loader = provider.GetRequiredService<IPublishReader>();
            var treeReader = provider.GetRequiredService<IContentTreeReader>();

            try {
                reader.SetRoot(Model.RootDirectory);
                loader.EnableAutoMaterial = Model.PackInput?.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;

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

        public async Task UpdateSelectedProfileAsync()
        {
            if (!Model.Profile.HasSelection) {
                Model.Profile.Loaded = null;
                return;
            }

            var packReader = provider.GetRequiredService<IResourcePackReader>();
            var profile = await packReader.ReadProfileAsync(Model.Profile.Selected.LocalFile);

            await Dispatcher.BeginInvoke(() => Model.Profile.Loaded = profile);
        }

        public async Task LoadPublishLocationsAsync(CancellationToken token = default)
        {
            var locationMgr = provider.GetRequiredService<IPublishLocationManager>();
            publishLocationList = await locationMgr.LoadAsync(token);
        }

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
            catch (Exception) {
                // TODO: LOG
            }
            finally {
                await Dispatcher.BeginInvoke(() => tab.IsLoading = false);
            }
        }

        //public async Task ImportModelFiltersAsync(string filename, CancellationToken token = default)
        //{
        //    //
        //}

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

                        var renderContext = new RenderContext {
                            PackInput = Model.PackInput,
                            PackProfile = Model.Profile.Loaded,
                            DefaultMaterial = Model.SelectedTabMaterial,
                            MissingMaterial = RenderModel.MissingMaterial,
                            EnvironmentCubeMap = RenderModel.EnvironmentCube,
                            IrradianceCubeMap = RenderModel.IrradianceCube,
                            EnvironmentEnabled = SceneModel.SunEnabled,
                            BrdfLutMap = RenderModel.BrdfLutMap,
                        };

                        try {
                            await context.BuildModelMeshAsync(RenderModel.RenderMode, renderContext, token);
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
                            RenderModel.MeshParts = context.Mesh.ModelParts;
                            //RenderModel.SetModel();
                            
                            TextureModel.Texture = null;
                        });
                    }
#endif
                    if (!Model.IsViewModeRender) {
                        if (context.IsLayerValid) return;

                        await Task.Run(() => context.BuildLayerAsync(
                            Model.PackInput, Model.Profile.Loaded,
                            material, Model.SelectedTag, token), token);

                        await Dispatcher.BeginInvoke(() => {
                            if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

#if !NORENDER
                            //RenderModel.ModelMaterial = null;
                            //RenderModel.BlockMesh = null;
                            RenderModel.MeshParts.Clear();
#endif

                            TextureModel.Texture = context.GetLayerImageSource();
                        });
                    }
                }

                if (Model.SelectedTab is TextureTabModel textureTab) {
                    if (context.LayerImage != null) return;

                    context.SourceFile = PathEx.Join(Model.RootDirectory, textureTab.ImageFilename);

                    await Dispatcher.BeginInvoke(() => {
                        if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

#if !NORENDER
                        //RenderModel.ModelMaterial = null;
                        //RenderModel.BlockMesh = null;
                        RenderModel.MeshParts.Clear();
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

            try {
                var inputFormat = TextureFormat.GetFactory(Model.PackInput.Format);
                var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                inputEncoding.Merge(Model.PackInput);
                inputEncoding.Merge(material);

                await Task.Factory.StartNew(async () => {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var graph = scope.ServiceProvider.GetRequiredService<ITextureNormalGraph>();
                    var reader = scope.ServiceProvider.GetRequiredService<IInputReader>();

                    context.Input = Model.PackInput;
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

            try {
                var inputFormat = TextureFormat.GetFactory(Model.PackInput.Format);
                var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
                inputEncoding.Merge(Model.PackInput);
                inputEncoding.Merge(material);

                await Task.Factory.StartNew(async () => {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                    var graph = scope.ServiceProvider.GetRequiredService<ITextureOcclusionGraph>();
                    var reader = scope.ServiceProvider.GetRequiredService<IInputReader>();

                    context.Input = Model.PackInput;
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

            var writer = provider.GetRequiredService<IOutputWriter>();
            var matWriter = provider.GetRequiredService<IMaterialWriter>();

            try {
                writer.SetRoot(Model.RootDirectory);
                await matWriter.WriteAsync(material);
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to save material '{material.LocalFilename}'!", error);
            }
        }

        public async Task<MaterialProperties> ImportTextureAsync(string filename, CancellationToken token = default)
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileInput();
            scopeBuilder.AddFileOutput();
            //...

            await using var scope = scopeBuilder.Build();

            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var matWriter = scope.GetRequiredService<IMaterialWriter>();

            reader.SetRoot(Model.RootDirectory);
            writer.SetRoot(Model.RootDirectory);

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
                await writer.OpenAsync(destFile, async destStream => {
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

        public void InvalidateAllTabs()
        {
            tabPreviewMgr.InvalidateAll(true);
        }

        private void UpdateRecentProjectsList()
        {
            Model.RecentDirectories.Clear();

            foreach (var item in recentMgr.Items)
                Model.RecentDirectories.Add(item);
        }

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

        private async Task LoadPackInputAsync()
        {
            var packReader = provider.GetRequiredService<IResourcePackReader>();

            var packInput = await packReader.ReadInputAsync("input.yml")
                            ?? new ResourcePackInputProperties {
                                Format = TextureFormat.Format_Raw,
                            };

            Application.Current.Dispatcher.Invoke(() => Model.PackInput = packInput);
        }

        private void UpdateProfileList()
        {
            var reader = provider.GetRequiredService<IInputReader>();

            Model.Profile.List.Clear();

            foreach (var file in reader.EnumerateFiles(".", "*.pack.yml")) {
                var localFile = Path.GetFileName(file);

                var profileItem = new ProfileItem {
                    Name = localFile[..^9],
                    LocalFile = localFile,
                };

                Model.Profile.List.Add(profileItem);
            }
        }

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
                RenderModel.ApplyMaterial(mat);
                //RenderModel.MeshBlendMode = mat?.BlendMode;
                //RenderModel.MeshTintColor = mat?.TintColor;
            }

            RenderModel.MeshParts = context.Mesh.ModelParts;
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
