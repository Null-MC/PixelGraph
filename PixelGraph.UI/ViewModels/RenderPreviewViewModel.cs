using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using SharpDX;
using SharpDX.DXGI;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using PixelGraph.UI.Helix;
using PixelGraph.UI.Helix.Shaders;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    public class RenderPreviewViewModel : IDisposable
    {
        private static readonly Lazy<Factory1> deviceFactory;

        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly ITabPreviewManager tabPreviewMgr;
        private Stream brdfLutStream;

        //public event EventHandler SceneChanged;
        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public RenderPreviewModel Model {get; set;}
        //public MainWindowModel MainModel {get; set;}
        public Dispatcher Dispatcher {get; set;}


        static RenderPreviewViewModel()
        {
            deviceFactory = new Lazy<Factory1>();
        }

        public RenderPreviewViewModel(IServiceProvider provider)
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

            ReloadShaders();
        }

        public void Initialize()
        {
            Model.Camera = new PerspectiveCamera {
                UpDirection = new Media3D.Vector3D(0, 1, 0),
            };

            Model.SunCamera = new OrthographicCamera {
                UpDirection = new Media3D.Vector3D(0f, 1f, 0f),
                NearPlaneDistance = 1f,
                FarPlaneDistance = 32f,
                Width = 8,
            };

            ResetViewport();

            Model.PropertyChanged += OnPreviewPropertyChanged;
            Model.RenderModeChanged += OnRenderModeChanged;
            Model.RenderSceneChanged += OnRenderSceneChanged;
        }

        public void Prepare()
        {
            LoadAppSettings();
            UpdateShaders();

            Model.CubeModel = BuildCube(4);
            Model.BrdfLutMap = brdfLutStream;
        }

        public void LoadAppSettings()
        {
            Model.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
            Model.ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.ParallaxSamplesMax = appSettings.Data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;
            Model.EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;
            Model.EnableSlopeNormals = appSettings.Data.RenderPreview.EnableSlopeNormals ?? RenderPreviewSettings.Default_EnableSlopeNormals;

            if (appSettings.Data.RenderPreview.SelectedMode != null)
                if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                    Model.RenderMode = renderMode;
        }

        public void UpdateSun()
        {
            const float sun_overlap = 0.06f;
            const float sun_power = 0.9f;
            const float sun_azimuth = 30f;
            const float sun_roll = 15f;

            MinecraftTime.GetSunAngle(sun_azimuth, sun_roll, Model.TimeOfDayLinear, out var sunDirection);
            Model.SunDirection = sunDirection;

            var strength = MinecraftTime.GetSunStrength(Model.TimeOfDayLinear, sun_overlap, sun_power);
            Model.SunStrength = strength;

            Model.SunCamera.Position = (sunDirection * 20f).ToPoint3D();
            Model.SunCamera.LookDirection = -sunDirection.ToVector3D();
        }

        public Task SaveRenderStateAsync(CancellationToken token = default)
        {
            appSettings.Data.RenderPreview.SelectedMode = RenderPreviewMode.GetString(Model.RenderMode);
            return appSettings.SaveAsync(token);
        }

        public void ReloadShaders()
        {
            var shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

            if (!shaderMgr.LoadAll(out var compileErrors))
                OnShaderCompileErrors(compileErrors);
        }

        public void UpdateShaders()
        {
            Model.EffectsManager?.Dispose();
            Model.EffectsManager = new PreviewEffectsManager(provider);
        }

        public void ResetViewport()
        {
            Model.Camera.Position = new Media3D.Point3D(10, 10, 10);
            Model.Camera.LookDirection = new Media3D.Vector3D(-10, -10, -10);

            Model.TimeOfDay = 8_000;
        }

        public string GetDeviceName()
        {
            if (Model.EffectsManager == null) return null;

            var adapter = deviceFactory.Value.GetAdapter(Model.EffectsManager.AdapterIndex);
            return adapter.Description.Description;
        }

        private void OnPreviewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.TimeOfDay)) UpdateSun();
        }

        private void OnShaderCompileErrors(ShaderCompileError[] errors)
        {
            var e = new ShaderCompileErrorEventArgs {
                Errors = errors,
            };

            ShaderCompileErrors?.Invoke(this, e);
        }

        private void OnRenderModeChanged(object sender, EventArgs e)
        {
            //if (!MainModel.IsViewModeRender) return;

            tabPreviewMgr.InvalidateAllMaterialBuilders(true);
            //OnSceneChanged();
        }

        private void OnRenderSceneChanged(object sender, EventArgs e)
        {
            //if (!MainModel.IsViewModeRender) return;

            tabPreviewMgr.InvalidateAllMaterials(true);
            //OnSceneChanged();
        }

        //private void OnSceneChanged()
        //{
        //    SceneChanged?.Invoke(this, EventArgs.Empty);
        //}

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

    public class ShaderCompileErrorEventArgs : EventArgs
    {
        public ShaderCompileError[] Errors {get; set;}
    }
}
