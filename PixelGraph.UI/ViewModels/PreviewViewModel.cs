using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.Models;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Tabs;
using SharpDX;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    internal class PreviewViewModel : IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly ITabPreviewManager tabPreviewMgr;
        private Stream brdfLutStream;

        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public MainWindowModel Model {get; set;}
        public Dispatcher Dispatcher {get; set;}


        public PreviewViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            appSettings = provider.GetRequiredService<IAppSettings>();
            tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();
        }

        public void Dispose()
        {
            brdfLutStream?.Dispose();
        }

        public async Task LoadContentAsync()
        {
            brdfLutStream = await ResourceLoader.BufferAsync("PixelGraph.UI.Resources.brdf_lut.dds");
        }

        public void Initialize()
        {
            LoadAppSettings();
            ReloadShaders();

            Model.Preview.Camera = new PerspectiveCamera {
                UpDirection = new Media3D.Vector3D(0, 1, 0),
            };

            Model.Preview.SunCamera = new OrthographicCamera {
                UpDirection = new Media3D.Vector3D(0f, 1f, 0f),
                NearPlaneDistance = 1f,
                FarPlaneDistance = 100f,
            };

            ResetViewport();

            Model.Preview.Model = BuildCube(4);
            //Model.Preview.Model = BuildBell();
            Model.Preview.BrdfLutMap = brdfLutStream;

            Model.SelectedTabChanged += OnSelectedTabChanged;

            Model.Preview.PropertyChanged += OnPreviewPropertyChanged;
            Model.Preview.EnableRenderChanged += OnEnableRenderChanged;
            Model.Preview.RenderModeChanged += OnRenderModeChanged;
            Model.Preview.RenderSceneChanged += OnRenderSceneChanged;
            Model.Preview.SelectedTagChanged += OnSelectedTagChanged;
        }

        private async void OnSelectedTabChanged(object sender, EventArgs e)
        {
            var tab = Model.SelectedTab;
            if (tab == null) return;

            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) throw new ApplicationException($"Tab context not found! id={Model.SelectedTab.Id}");

            Model.Preview.ModelMaterial = context.ModelMaterial;
            Model.Preview.LayerImage = context.GetLayerImageSource();
            //tab.IsLoading = true;

            await UpdateTabPreviewAsync(tab, CancellationToken.None);
        }

        public void LoadAppSettings()
        {
            Model.Preview.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
            Model.Preview.ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.Preview.ParallaxSamplesMax = appSettings.Data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;
            Model.Preview.EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;

            if (appSettings.Data.RenderPreview.SelectedMode != null)
                if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                    Model.Preview.RenderMode = renderMode;
        }

        public void UpdateSun()
        {
            //const float sun_distance = 10f;
            const float sun_overlap = 0.06f;
            const float sun_power = 0.9f;
            const float sun_azimuth = 30f;
            const float sun_roll = 15f;

            //var linearTimeOfDay = Model.Preview.TimeOfDayLinear;
            MinecraftTime.GetSunAngle(sun_azimuth, sun_roll, Model.Preview.TimeOfDayLinear, out var sunDirection);
            Model.Preview.SunDirection = sunDirection;

            var strength = MinecraftTime.GetSunStrength(Model.Preview.TimeOfDayLinear, sun_overlap, sun_power); // * (1f - Wetness * 0.6f);
            //Model.Preview.SunLightColor = new Color4(strength, strength, strength, strength);
            Model.Preview.SunStrength = strength;

            Model.Preview.SunCamera.Position = (sunDirection * 20f).ToPoint3D();
            Model.Preview.SunCamera.LookDirection = -sunDirection.ToVector3D();
        }

        public Task SaveRenderStateAsync(CancellationToken token = default)
        {
            appSettings.Data.RenderPreview.SelectedMode = RenderPreviewMode.GetString(Model.Preview.RenderMode);
            return appSettings.SaveAsync(token);
        }

        public void ReloadShaders()
        {
            var shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            if (!shaderMgr.LoadAll(out var compileErrors))
                OnShaderCompileErrors(compileErrors);

            Model.Preview.EffectsManager?.Dispose();
            Model.Preview.EffectsManager = new CustomEffectsManager(provider);

            // WARN: FOR TESTING ONLY!
            //Model.Preview.Model = BuildBell();
        }

        public void ResetViewport()
        {
            Model.Preview.Camera.Position = new Media3D.Point3D(10, 10, 10);
            Model.Preview.Camera.LookDirection = new Media3D.Vector3D(-10, -10, -10);

            Model.Preview.TimeOfDay = 8_000;
        }

        public void Invalidate(Guid tabId)
        {
            var context = tabPreviewMgr.Get(tabId);
            if (context == null) return;

            context.Invalidate(false);
        }

        public void InvalidateAll()
        {
            tabPreviewMgr.InvalidateAll();
        }

        private void OnPreviewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PreviewContextModel.TimeOfDay)) UpdateSun();
        }

        public async Task UpdateTabPreviewAsync(ITabModel tab, CancellationToken token = default)
        {
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

        private async Task UpdateTabPreviewAsync(TabPreviewContext context, CancellationToken token)
        {
            try {
                if (Model.SelectedTab is MaterialTabModel materialTab) {
                    if (Model.Preview.EnableRender) {
                        if (materialTab.Material == null || context.IsMaterialValid) return;

                        if (!context.IsMaterialBuilderValid)
                            await Task.Run(() => context.BuildMaterialAsync(Model, token), token);

                        await Dispatcher.BeginInvoke(() => {
                            if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

                            context.UpdateMaterial(Model);
                            Model.Preview.ModelMaterial = context.ModelMaterial;
                            Model.Preview.LayerImage = null;
                        });
                    }
                    else {
                        if (TextureTags.Is(context.CurrentLayerTag, Model.Preview.SelectedTag)) {
                            if (context.IsLayerValid) return;
                        }
                        
                        await Task.Run(() => context.BuildLayerAsync(Model, token), token);

                        await Dispatcher.BeginInvoke(() => {
                            if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

                            Model.Preview.ModelMaterial = null;
                            Model.Preview.LayerImage = context.GetLayerImageSource();
                        });
                    }
                }
                else if (Model.SelectedTab is TextureTabModel textureTab) {
                    if (context.LayerImage != null) return;

                    context.SourceFile = PathEx.Join(Model.RootDirectory, textureTab.ImageFilename);

                    await Dispatcher.BeginInvoke(() => {
                        if (Model.SelectedTab == null || Model.SelectedTab.Id != context.Id) return;

                        Model.Preview.ModelMaterial = null;
                        Model.Preview.LayerImage = context.GetLayerImageSource();
                    });
                }
            }
            catch (OperationCanceledException) {}
        }

        private void OnShaderCompileErrors(ShaderCompileError[] errors)
        {
            var e = new ShaderCompileErrorEventArgs {
                Errors = errors,
            };

            ShaderCompileErrors?.Invoke(this, e);
        }

        private async void OnEnableRenderChanged(object sender, EventArgs e)
        {
            var tab = Model.SelectedTab;
            if (tab == null) return;

            var context = tabPreviewMgr.Get(tab.Id);
            if (context == null) return;

            context.Invalidate(false);
            //if (Model.Preview.EnableRender) {
            //    context.IsMaterialValid = false;
            //}
            //else {
            //    context.IsLayerValid = false;
            //}

            await UpdateTabPreviewAsync(tab);
        }

        private async void OnRenderModeChanged(object sender, EventArgs e)
        {
            if (!Model.Preview.EnableRender) return;

            tabPreviewMgr.InvalidateAllMaterialBuilders(true);

            var tab = Model.SelectedTab;
            if (tab != null) await UpdateTabPreviewAsync(tab);
        }

        private async void OnRenderSceneChanged(object sender, EventArgs e)
        {
            if (!Model.Preview.EnableRender) return;

            tabPreviewMgr.InvalidateAllMaterials(true);

            var tab = Model.SelectedTab;
            if (tab != null) await UpdateTabPreviewAsync(tab);
        }

        private async void OnSelectedTagChanged(object sender, EventArgs e)
        {
            if (Model.Preview.EnableRender) return;

            var tab = Model.SelectedTab;
            if (tab == null) return;
            
            var context = tabPreviewMgr.Get(tab.Id);
            context.IsLayerValid = false;
            context.IsLayerSourceValid = false;
                
            await UpdateTabPreviewAsync(tab, CancellationToken.None);
        }

        private static MeshGeometry3D BuildCube(int size)
        {
            var builder = new MeshBuilder(true, true, true);

            builder.AddCubeFace(Vector3.Zero, new Vector3(1, 0, 0), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(-1, 0, 0), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, 0, 1), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, 0, -1), Vector3.UnitY, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, 1, 0), Vector3.UnitX, size, size, size);
            builder.AddCubeFace(Vector3.Zero, new Vector3(0, -1, 0), Vector3.UnitX, size, size, size);

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToMeshGeometry3D();
        }

        //private static MeshGeometry3D BuildBell()
        //{
        //    var builder = new MeshBuilder(true, true, true);

        //    var centerTop = new Vector3(0f, -2.5f, 0f);
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 1, 0,  0), Vector3.UnitY, 6, 6, 7, new Vector2(0f, 3/16f), new Vector2(3/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3(-1, 0,  0), Vector3.UnitY, 6, 6, 7, new Vector2(6/16f, 3/16f), new Vector2(9/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 0, 0,  1), Vector3.UnitY, 6, 6, 7, new Vector2(9/16f, 3/16f), new Vector2(12/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 0, 0, -1), Vector3.UnitY, 6, 6, 7, new Vector2(3/16f, 3/16f), new Vector2(6/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 0, 1,  0), Vector3.UnitX, 7, 6, 6, new Vector2(6/16f, 0f), new Vector2(9/16f, 3/16f));

        //    var centerBottom = new Vector3(0f, -7f, 0f);
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 1,  0,  0), Vector3.UnitY, 8, 8, 2, new Vector2(0f, 10.5f/16f), new Vector2(4/16f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3(-1,  0,  0), Vector3.UnitY, 8, 8, 2, new Vector2(8/16f, 10.5f/16f), new Vector2(12/16f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0,  0,  1), Vector3.UnitY, 8, 8, 2, new Vector2(12/16f, 10.5f/16f), new Vector2(1f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0,  0, -1), Vector3.UnitY, 8, 8, 2, new Vector2(4/16f, 10.5f/16f), new Vector2(8/16f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0,  1,  0), Vector3.UnitX, 2, 8, 8, new Vector2(8/16f, 6.5f/16f), new Vector2(12/16f, 10.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0, -1,  0), Vector3.UnitX, 2, 8, 8, new Vector2(4/16f, 6.5f/16f), new Vector2(8/16f, 10.5f/16f));

        //    //builder.Scale();
        //    builder.ComputeTangents(MeshFaces.Default);
        //    return builder.ToMeshGeometry3D();
        //}
    }

    internal class ShaderCompileErrorEventArgs : EventArgs
    {
        public ShaderCompileError[] Errors {get; set;}
    }
}
