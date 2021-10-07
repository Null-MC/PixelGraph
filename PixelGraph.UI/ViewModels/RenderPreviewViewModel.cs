using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Minecraft.Java.Entities;
using PixelGraph.Common.IO;
using PixelGraph.Common.Models;
using PixelGraph.UI.Helix;
using PixelGraph.UI.Helix.Models;
using PixelGraph.UI.Helix.Shaders;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    public class RenderPreviewViewModel : IDisposable
    {
        private const float CubeSize = 4f;

        private static readonly Lazy<Factory1> deviceFactory;

        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly ITabPreviewManager tabPreviewMgr;
        private readonly IModelBuilder modelBuilder;
        private Stream brdfLutStream;

        //public event EventHandler SceneChanged;
        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public RenderPreviewModel Model {get; set;}
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
            modelBuilder = provider.GetRequiredService<IModelBuilder>();
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
                Width = 24,
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

            Model.BrdfLutMap = brdfLutStream;

            UpdateModel();
        }

        public void LoadAppSettings()
        {
            Model.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
            Model.ParallaxSamplesMin = appSettings.Data.RenderPreview.ParallaxSamplesMin ?? RenderPreviewSettings.Default_ParallaxSamplesMin;
            Model.ParallaxSamplesMax = appSettings.Data.RenderPreview.ParallaxSamplesMax ?? RenderPreviewSettings.Default_ParallaxSamplesMax;
            Model.EnableLinearSampling = appSettings.Data.RenderPreview.EnableLinearSampling ?? RenderPreviewSettings.Default_EnableLinearSampling;
            Model.EnableSlopeNormals = appSettings.Data.RenderPreview.EnableSlopeNormals ?? RenderPreviewSettings.Default_EnableSlopeNormals;
            Model.EnablePuddles = appSettings.Data.RenderPreview.EnablePuddles ?? RenderPreviewSettings.Default_EnablePuddles;

            if (appSettings.Data.RenderPreview.SelectedMode != null)
                if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                    Model.RenderMode = renderMode;
        }

        public void UpdateSun()
        {
            const float sun_overlap = 0.06f;
            const float sun_power = 0.9f;
            const float sun_azimuth = 30f;
            const float sun_roll = 25f;

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

        public void UpdateModel()
        {
            var map = new Dictionary<string, Func<BlockMeshGeometry3D>>(StringComparer.InvariantCultureIgnoreCase) {
                [ModelType.File] = BuildModelFile,
                [ModelType.Cube] = () => modelBuilder.BuildCube(CubeSize),
                [ModelType.Cross] = () => modelBuilder.BuildCross(CubeSize),
                [ModelType.Plane] = () => modelBuilder.BuildCube(CubeSize, 4, 1, 4),
                [ModelType.Bell] = () => modelBuilder.BuildEntity(CubeSize, new BellBody().GetLatestVersion()),
            };

            if (Model.ModelType != null) {
                if (!map.TryGetValue(Model.ModelType, out var meshFunc))
                    throw new ApplicationException($"Unknown model type '{Model.ModelType}'!");

                Model.BlockMesh = meshFunc();
            }
            else {
                Model.BlockMesh = modelBuilder.BuildCube(CubeSize);
            }
        }

        private BlockMeshGeometry3D BuildModelFile()
        {
            if (Model.ModelFile == null) return null;

            var reader = provider.GetRequiredService<IInputReader>();
            var parser = provider.GetRequiredService<IBlockModelParser>();

            var filename = reader.GetFullPath(Model.ModelFile);
            var localPath = Path.GetDirectoryName(filename);
            var localFile = Path.GetFileName(filename);

            var model = parser.LoadRecursive(localPath, localFile);
            if (model == null) throw new ApplicationException("Failed to load model!");

            return modelBuilder.BuildModel(CubeSize, model);
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
    }

    public class ShaderCompileErrorEventArgs : EventArgs
    {
        public ShaderCompileError[] Errors {get; set;}
    }
}
