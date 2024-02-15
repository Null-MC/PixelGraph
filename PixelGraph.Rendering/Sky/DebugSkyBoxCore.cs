using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.CubeMaps;
using SharpDX.Direct3D11;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.Rendering.Sky;

public class DebugSkyBoxCore : GeometryRenderCore
{
    private ShaderPass? defaultShaderPass;
    private ICubeMapSource? _cubeMapSource;
    private SamplerStateDescription samplerDescription;
    private SamplerStateProxy? textureSampler;
    private int cubeTextureSlot;
    private int textureSamplerSlot;
        
    public ICubeMapSource? CubeMapSource {
        get => _cubeMapSource;
        set => SetAffectsRender(ref _cubeMapSource, value);
    }

    public SamplerStateDescription SamplerDescription {
        get => samplerDescription;
        set {
            if (!SetAffectsRender(ref samplerDescription, value) || !IsAttached) return;

            var newSampler = EffectTechnique.EffectsManager.StateManager.Register(value);
            RemoveAndDispose(ref textureSampler);
            textureSampler = newSampler;
        }
    }


    public DebugSkyBoxCore()
    {
        RasterDescription = DefaultRasterDescriptions.RSSkybox;
        samplerDescription = DefaultSamplers.EnvironmentSampler;
    }

    protected override bool OnAttach(IRenderTechnique technique)
    {
        if (!base.OnAttach(technique)) return false;

        defaultShaderPass = technique[DefaultPassNames.Default];
        OnDefaultPassChanged(defaultShaderPass);

        GeometryBuffer = new SkyBoxBufferModel();
        textureSampler = technique.EffectsManager.StateManager.Register(SamplerDescription);
        return true;
    }

    protected override void OnDetach()
    {
        RemoveAndDispose(ref textureSampler);
        RemoveAndDispose(ref defaultShaderPass);

        RemoveAndDispose(GeometryBuffer);
        GeometryBuffer = null;

        base.OnDetach();
    }

    protected override void OnRender(RenderContext context, DeviceContextProxy deviceContext)
    {
        if (_cubeMapSource == null || defaultShaderPass == null) return;

        deviceContext.SetShaderResource(PixelShader.Type, cubeTextureSlot, _cubeMapSource.CubeMap);
        deviceContext.SetSampler(PixelShader.Type, textureSamplerSlot, textureSampler);

        if (context.Camera.CreateLeftHandSystem && RasterDescription.IsFrontCounterClockwise) {
            var desc = RasterDescription;
            desc.IsFrontCounterClockwise = false;
            RasterDescription = desc;
            RaiseInvalidateRender();
            return;
        }

        defaultShaderPass.BindShader(deviceContext);
        defaultShaderPass.BindStates(deviceContext, StateType.BlendState | StateType.DepthStencilState);
        defaultShaderPass.PixelShader.BindSampler(deviceContext, textureSamplerSlot, textureSampler);
        deviceContext.Draw(GeometryBuffer.VertexBuffer[0].ElementCount, 0);
    }

    protected override void OnRenderCustom(RenderContext context, DeviceContextProxy deviceContext) {}

    protected override void OnRenderShadow(RenderContext context, DeviceContextProxy deviceContext) {}

    protected override void OnRenderDepth(RenderContext context, DeviceContextProxy deviceContext, ShaderPass customPass) {}

    protected void OnDefaultPassChanged(ShaderPass pass)
    {
        cubeTextureSlot = pass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(DefaultBufferNames.CubeMapTB);
        textureSamplerSlot = pass.PixelShader.SamplerMapping.TryGetBindSlot(DefaultSamplerStateNames.CubeMapSampler);
    }
}