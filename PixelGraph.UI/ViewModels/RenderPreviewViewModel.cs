using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Material;
using PixelGraph.Rendering;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Models.Scene;
using SharpDX.DXGI;
using System;
using System.Threading;
using System.Threading.Tasks;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels
{
    public class RenderPreviewViewModel
    {
        private static readonly Lazy<Factory1> deviceFactory;

        private readonly IServiceProvider provider;
        private readonly IAppSettings appSettings;
        private readonly ITabPreviewManager tabPreviewMgr;

        public event EventHandler RenderModelChanged;
        public event EventHandler<ShaderCompileErrorEventArgs> ShaderCompileErrors;

        public ScenePropertiesModel SceneProperties {get; set;}
        public RenderPropertiesModel RenderProperties {get; set;}


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

        public void Initialize()
        {
            ResetViewport();

            RenderProperties.RenderModeChanged += OnRenderModeChanged;
            RenderProperties.RenderModelChanged += OnRenderModelChanged;
        }

        public void Prepare()
        {
            LoadAppSettings();
            UpdateShaders();

            RenderProperties.MissingMaterial = new MaterialProperties {
                Color = new MaterialColorProperties {
                    Value = "#f800f8",
                    Texture = "<missing>",
                }
            };
        }

        public void LoadAppSettings()
        {
            RenderProperties.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
            RenderProperties.ParallaxSamples = appSettings.Data.RenderPreview.ParallaxSamples ?? RenderPreviewSettings.Default_ParallaxSamples;
            RenderProperties.EnableBloom = appSettings.Data.RenderPreview.EnableBloom ?? RenderPreviewSettings.Default_EnableBloom;
            RenderProperties.WaterMode = appSettings.Data.RenderPreview.WaterMode ?? RenderPreviewSettings.Default_WaterMode;
            RenderProperties.SubSurfaceBlur = (float)(appSettings.Data.RenderPreview.SubSurfaceBlur ?? RenderPreviewSettings.Default_SubSurfaceBlur);

            RenderProperties.EnvironmentMapSize = appSettings.Data.RenderPreview.EnvironmentCubeSize ?? RenderPreviewSettings.Default_EnvironmentCubeSize;
            RenderProperties.IrradianceMapSize = appSettings.Data.RenderPreview.IrradianceCubeSize ?? RenderPreviewSettings.Default_IrradianceCubeSize;

            if (appSettings.Data.RenderPreview.SelectedMode != null)
                if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                    RenderProperties.RenderMode = renderMode;
        }

        public Task SaveRenderStateAsync(CancellationToken token = default)
        {
            appSettings.Data.RenderPreview.SelectedMode = RenderPreviewMode.GetString(RenderProperties.RenderMode);
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
            RenderProperties.EffectsManager?.Dispose();
            RenderProperties.EffectsManager = new PreviewEffectsManager(provider);
        }

        public string GetDeviceName()
        {
            if (RenderProperties.EffectsManager == null) return null;

            var adapter = deviceFactory.Value.GetAdapter(RenderProperties.EffectsManager.AdapterIndex);
            return adapter.Description.Description;
        }

        private void ResetViewport()
        {
            RenderProperties.Camera.Position = new Media3D.Point3D(10, 10, 10);
            RenderProperties.Camera.LookDirection = new Media3D.Vector3D(-10, -10, -10);

            SceneProperties.TimeOfDay = 4_000;
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
            tabPreviewMgr.InvalidateAllMaterialBuilders(true);
        }

        private void OnRenderModelChanged(object sender, EventArgs e)
        {
            OnRenderModelChanged();
        }

        protected virtual void OnRenderModelChanged()
        {
            RenderModelChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ShaderCompileErrorEventArgs : EventArgs
    {
        public ShaderCompileError[] Errors {get; set;}
    }
}
