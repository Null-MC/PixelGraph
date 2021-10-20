using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Rendering;
using PixelGraph.Rendering.Shaders;
using PixelGraph.Rendering.Utilities;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Scene;
using SharpDX;
using SharpDX.DXGI;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.Material;
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

        public ScenePropertiesModel SceneModel {get; set;}
        public RenderPreviewModel RenderModel {get; set;}
        //public MainWindowModel MainModel {get; set;}


        static RenderPreviewViewModel()
        {
            deviceFactory = new Lazy<Factory1>();
        }

        public RenderPreviewViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            appSettings = provider.GetRequiredService<IAppSettings>();
            tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();
            //modelBuilder = provider.GetRequiredService<IModelBuilder>();
        }

        public void Dispose()
        {
            brdfLutStream?.Dispose();
        }

        public async Task LoadContentAsync()
        {
            brdfLutStream = await ResourceLoader.BufferAsync("PixelGraph.Rendering.Resources.brdf_lut.dds");

            ReloadShaders();
        }

        public void Initialize()
        {
            RenderModel.Camera = new PerspectiveCamera {
                UpDirection = new Media3D.Vector3D(0, 1, 0),
            };

            RenderModel.SunCamera = new OrthographicCamera {
                UpDirection = new Media3D.Vector3D(0f, 1f, 0f),
                //NearPlaneDistance = 1f,
                //FarPlaneDistance = 128f,
                //Width = 512,
            };

            ResetViewport();

            //RenderModel.PropertyChanged += OnPreviewPropertyChanged;
            RenderModel.RenderModeChanged += OnRenderModeChanged;
            RenderModel.RenderSceneChanged += OnRenderSceneChanged;
        }

        public void Prepare()
        {
            LoadAppSettings();
            UpdateShaders();

            RenderModel.BrdfLutMap = brdfLutStream;

            RenderModel.MissingMaterial = new MaterialProperties {
                Color = new MaterialColorProperties {
                    //ValueRed = 248m,
                    //ValueGreen = 0m,
                    //ValueBlue = 248m,
                    Texture = "<missing>",
                }
            };

            //UpdateModel();
        }

        public void LoadAppSettings()
        {
            RenderModel.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
            RenderModel.ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            RenderModel.ParallaxSamplesMax = appSettings.Data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;
            RenderModel.EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;
            RenderModel.EnableSlopeNormals = appSettings.Data.RenderPreview.EnableSlopeNormals ?? RenderPreviewSettings.Default_EnableSlopeNormals;
            RenderModel.EnablePuddles = appSettings.Data.RenderPreview.EnablePuddles ?? RenderPreviewSettings.Default_EnablePuddles;

            if (appSettings.Data.RenderPreview.SelectedMode != null)
                if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                    RenderModel.RenderMode = renderMode;
        }

        public void UpdateSun()
        {
            SceneModel.GetSunAngle(out var sunDirection, out var sunStrength);

            RenderModel.SunDirection = sunDirection;
            RenderModel.SunStrength = sunStrength;
            RenderModel.SunCamera.Position = (new Vector3(0f, 2f, 0f) + sunDirection * 32f).ToPoint3D();
            RenderModel.SunCamera.LookDirection = -sunDirection.ToVector3D();

            //RenderModel.SunCamera.ZoomExtents(viewport3D, new Media3D.Point3D(), 32f, 0D);
        }

        public Task SaveRenderStateAsync(CancellationToken token = default)
        {
            appSettings.Data.RenderPreview.SelectedMode = RenderPreviewMode.GetString(RenderModel.RenderMode);
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
            RenderModel.EffectsManager?.Dispose();
            RenderModel.EffectsManager = new PreviewEffectsManager(provider);
        }

        //public void UpdateModel(MaterialProperties material)
        //{
        //    var map = new Dictionary<string, Func<BlockMeshGeometry3D>>(StringComparer.InvariantCultureIgnoreCase) {
        //        [ModelType.Bell] = () => modelBuilder.BuildEntity(CubeSize, new BellBody().GetLatestVersion()),
        //        [ModelType.Boat] = () => modelBuilder.BuildEntity(CubeSize, new Boat().GetLatestVersion()),
        //        [ModelType.Cow] = () => modelBuilder.BuildEntity(CubeSize, new Cow().GetLatestVersion()),
        //        [ModelType.Cube] = () => modelBuilder.BuildCube(CubeSize),
        //        [ModelType.Plane] = () => modelBuilder.BuildCube(CubeSize, 4, 1, 4),
        //        [ModelType.Zombie] = () => modelBuilder.BuildEntity(CubeSize, new Zombie().GetLatestVersion()),
        //    };

        //    if (!string.IsNullOrWhiteSpace(material.ModelFile)) {
        //        try {
        //            RenderModel.BlockMesh = BuildModelFile(material);
        //            return;
        //        }
        //        catch (Exception) {
        //            // TODO: log error!
        //        }
        //    }

        //    if (material.ModelType != null) {
        //        if (map.TryGetValue(material.ModelType, out var meshFunc)) {
        //            RenderModel.BlockMesh = meshFunc();
        //            if (RenderModel.BlockMesh != null) return;
        //        }
        //        else {
        //            //throw new ApplicationException($"Unknown model type '{Model.ModelType}'!");
        //            // TODO: log error!
        //        }
        //    }

        //    RenderModel.BlockMesh = modelBuilder.BuildCube(CubeSize);
        //}

        public void ResetViewport()
        {
            RenderModel.Camera.Position = new Media3D.Point3D(10, 10, 10);
            RenderModel.Camera.LookDirection = new Media3D.Vector3D(-10, -10, -10);

            SceneModel.TimeOfDay = 4_000;
        }

        public string GetDeviceName()
        {
            if (RenderModel.EffectsManager == null) return null;

            var adapter = deviceFactory.Value.GetAdapter(RenderModel.EffectsManager.AdapterIndex);
            return adapter.Description.Description;
        }

        //private void OnPreviewPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(RenderModel.TimeOfDay)) UpdateSun();
        //}

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
    }

    public class ShaderCompileErrorEventArgs : EventArgs
    {
        public ShaderCompileError[] Errors {get; set;}
    }
}
