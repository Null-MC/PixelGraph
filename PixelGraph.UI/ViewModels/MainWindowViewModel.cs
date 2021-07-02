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
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
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

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowViewModel
    {
        private readonly ILogger<MainWindowViewModel> logger;
        private readonly IServiceProvider provider;
        private readonly IRecentPathManager recentMgr;
        private readonly ITextureEditUtility editUtility;
        private readonly ITabPreviewManager tabPreviewMgr;

        public event EventHandler<UnhandledExceptionEventArgs> TreeError;

        public MainWindowModel Model {get; set;}
        public Dispatcher Dispatcher {get; set;}


        public MainWindowViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<MainWindowViewModel>>();
            recentMgr = provider.GetRequiredService<IRecentPathManager>();
            editUtility = provider.GetRequiredService<ITextureEditUtility>();
            tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();
        }

        public async Task InitializeAsync()
        {
            var settings = provider.GetRequiredService<IAppSettings>();

            if (settings.Data.SelectedPublishLocation != null) {
                var location = Model.PublishLocations.FirstOrDefault(x => string.Equals(x.DisplayName, settings.Data.SelectedPublishLocation, StringComparison.InvariantCultureIgnoreCase));
                if (location != null) Model.SelectedLocation = location;
            }

            try {
                Model.RecentDirectories = recentMgr.List;

                await recentMgr.InitializeAsync();
            }
            catch (Exception error) {
                throw new ApplicationException("Failed to load recent projects list!", error);
            }

            Model.TabClosed += OnTabClosed;
        }

        public void Clear()
        {
            Model.SelectedNode = null;
            Model.PackInput = null;
            Model.TreeRoot = null;
            Model.RootDirectory = null;

            Model.Profile.List.Clear();
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

            await LoadRootDirectoryAsync();

            await recentMgr.InsertAsync(path, token);
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
                Model.Profile.List.Clear();

                try {
                    UpdateProfileList();
                }
                catch (Exception error) {
                    throw new ApplicationException("Failed to load pack profile definitions!", error);
                }

                Model.TreeRoot = new ContentTreeDirectory(null) {
                    LocalPath = null,
                };

                await Dispatcher.BeginInvoke(() => {
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

            tabPreviewMgr.Remove(Model.PreviewTab.Id);
            Model.PreviewTab = null;
            Model.IsPreviewTabSelected = false;
        }

        public void SetPreviewTab(ITabModel newTab)
        {
            if (Model.PreviewTab != null)
                tabPreviewMgr.Remove(Model.PreviewTab.Id);

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
            var locations = await locationMgr.LoadAsync(token);
            if (locations == null) return;

            var list = locations.Select(x => new LocationModel(x)).ToList();
            Application.Current.Dispatcher.Invoke(() => Model.PublishLocations = list);
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

                    context.Input = Model.PackInput;
                    context.Material = material;

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

                    context.Input = Model.PackInput;
                    context.Material = material;

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

                // TODO: update texture sources
                //await PopulateTextureViewerAsync(token);
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

            await matWriter.WriteAsync(material);

            var ext = Path.GetExtension(filename);
            var destFile = PathEx.Join(localPath, itemName, $"albedo{ext}");
            await using (var sourceStream = reader.Open(filename)) {
                await using var destStream = writer.Open(destFile);
                await sourceStream.CopyToAsync(destStream, token);
            }

            writer.Delete(filename);
            return material;
        }

        public Task RemoveRecentItemAsync(string item, CancellationToken token = default)
        {
            return recentMgr.RemoveAsync(item, token);
        }

        public async Task LoadTabContentAsync(ITabModel tabModel, CancellationToken token = default)
        {
            if (tabModel is MaterialTabModel materialTab) {
                var reader = provider.GetRequiredService<IInputReader>();
                var matReader = provider.GetRequiredService<IMaterialReader>();

                reader.SetRoot(Model.RootDirectory);
                materialTab.Material = await matReader.LoadAsync(materialTab.MaterialFilename, token);
            }
        }

        #region External Image Editing

        public async Task BeginExternalEditAsync(CancellationToken token = default)
        {
            if (Model.SelectedTab is not MaterialTabModel materialTab) return;
            
            var selectedMaterial = materialTab.Material;
            if (selectedMaterial == null) return;

            if (!Model.Preview.HasSelectedTag) return;

            try {
                Model.IsImageEditorOpen = true;

                var success = await editUtility.EditLayerAsync(selectedMaterial, Model.Preview.SelectedTag, token);
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
            for (var i = Model.TabList.Count - 1; i >= 0; i--) {
                if (Model.TabList[i].Id != e.TabId) continue;

                Model.TabList.RemoveAt(i);
                break;
            }

            tabPreviewMgr.Remove(e.TabId);
        }
    }
}
