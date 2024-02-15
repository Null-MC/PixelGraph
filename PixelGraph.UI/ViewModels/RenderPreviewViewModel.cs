using HelixToolkit.SharpDX.Core;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Rendering;
using PixelGraph.Rendering.Shaders;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Tabs;
using PixelGraph.UI.Models.Scene;
using SharpDX.DXGI;
using Media3D = System.Windows.Media.Media3D;

namespace PixelGraph.UI.ViewModels;

public class RenderPreviewViewModel(IServiceProvider provider)
{
    private static readonly Lazy<Factory1> deviceFactory;

    private readonly IAppSettingsManager appSettings = provider.GetRequiredService<IAppSettingsManager>();
    private readonly ITabPreviewManager tabPreviewMgr = provider.GetRequiredService<ITabPreviewManager>();

    public event EventHandler? RenderModelChanged;
    public event EventHandler<ShaderCompileErrorEventArgs>? ShaderCompileErrors;

    public ScenePropertiesModel? SceneProperties {get; set;}
    public RenderPropertiesModel? RenderProperties {get; set;}


    static RenderPreviewViewModel()
    {
        deviceFactory = new Lazy<Factory1>();
    }

    public void Initialize()
    {
        ResetViewport();

        ArgumentNullException.ThrowIfNull(RenderProperties);

        RenderProperties.RenderModeChanged += OnRenderModeChanged;
        RenderProperties.RenderModelChanged += OnRenderModelChanged;
    }

    public void Prepare()
    {
        LoadAppSettings();
        UpdateShaders();

        //ArgumentNullException.ThrowIfNull(RenderProperties);

        //RenderProperties.MissingMaterial = new MaterialProperties {
        //    Color = new MaterialColorProperties {
        //        Value = "#f800f8",
        //        Texture = "<missing>",
        //    }
        //};
    }

    public void LoadAppSettings()
    {
        ArgumentNullException.ThrowIfNull(RenderProperties);

        RenderProperties.EnableBloom = appSettings.Data.RenderPreview.EnableBloom ?? RenderPreviewSettings.Default_EnableBloom;
        RenderProperties.EnableSwapChain = appSettings.Data.RenderPreview.EnableSwapChain ?? RenderPreviewSettings.Default_EnableSwapChain;
        RenderProperties.WaterMode = appSettings.Data.RenderPreview.WaterMode ?? RenderPreviewSettings.Default_WaterMode;
        RenderProperties.FXAA = (FXAALevel)(appSettings.Data.RenderPreview.FXAA ?? RenderPreviewSettings.Default_FXAA);

        RenderProperties.ParallaxDepth = (float)(appSettings.Data.RenderPreview.ParallaxDepth ?? RenderPreviewSettings.Default_ParallaxDepth);
        RenderProperties.ParallaxSamples = appSettings.Data.RenderPreview.ParallaxSamples ?? RenderPreviewSettings.Default_ParallaxSamples;

        RenderProperties.EnvironmentMapSize = appSettings.Data.RenderPreview.EnvironmentCubeSize ?? RenderPreviewSettings.Default_EnvironmentCubeSize;
        RenderProperties.IrradianceMapSize = appSettings.Data.RenderPreview.IrradianceCubeSize ?? RenderPreviewSettings.Default_IrradianceCubeSize;
        RenderProperties.SubSurfaceBlur = (float)(appSettings.Data.RenderPreview.SubSurfaceBlur ?? RenderPreviewSettings.Default_SubSurfaceBlur);

        if (appSettings.Data.RenderPreview.SelectedMode != null)
            if (RenderPreviewMode.TryParse(appSettings.Data.RenderPreview.SelectedMode, out var renderMode))
                RenderProperties.RenderMode = renderMode;
    }

    public async Task SaveRenderStateAsync(CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(RenderProperties);

        appSettings.Data.RenderPreview.SelectedMode = RenderPreviewMode.GetString(RenderProperties.RenderMode);

        await appSettings.SaveAsync(token);
    }

    public void ReloadShaders()
    {
        var shaderMgr = provider.GetRequiredService<IShaderByteCodeManager>();

        if (!shaderMgr.LoadAll(out var compileErrors))
            OnShaderCompileErrors(compileErrors);
    }

    public void UpdateShaders()
    {
        ArgumentNullException.ThrowIfNull(RenderProperties);

        RenderProperties.EffectsManager?.Dispose();
        RenderProperties.EffectsManager = new PreviewEffectsManager(provider);
    }

    public string? GetDeviceName()
    {
        ArgumentNullException.ThrowIfNull(RenderProperties);

        if (RenderProperties.EffectsManager == null) return null;

        var adapter = deviceFactory.Value.GetAdapter(RenderProperties.EffectsManager.AdapterIndex);
        return adapter.Description.Description;
    }

    private void ResetViewport()
    {
        ArgumentNullException.ThrowIfNull(SceneProperties);
        ArgumentNullException.ThrowIfNull(RenderProperties);
        ArgumentNullException.ThrowIfNull(RenderProperties.Camera);

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

    private void OnRenderModeChanged(object? sender, EventArgs e)
    {
        tabPreviewMgr.InvalidateAllMaterialBuilders(true);
    }

    private void OnRenderModelChanged(object? sender, EventArgs e)
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
    public ShaderCompileError[]? Errors {get; set;}
}
