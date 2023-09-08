using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Minecraft;
using PixelGraph.Rendering.Sky;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PixelGraph.Rendering.CubeMaps;

internal class DynamicSkyCubeCore : CubeMapRenderCore
{
    private SkyDomeBufferModel geometryBuffer;
    private IMinecraftScene _scene;
    private long lastSceneUpdate;

    public IMinecraftScene Scene {
        get => _scene;
        set => SetAffectsRender(ref _scene, value);
    }

    public override ShaderResourceViewProxy CubeMap => _cubeMap;


    public DynamicSkyCubeCore() : base(RenderType.PreProc)
    {
        TextureDesc = new Texture2DDescription {
            Format = Format.R16G16B16A16_Float,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
            SampleDescription = new SampleDescription(1, 0),
            CpuAccessFlags = CpuAccessFlags.None,
            Usage = ResourceUsage.Default,
            ArraySize = 6,
            MipLevels = 0,
        };
    }


    protected override bool OnAttach(IRenderTechnique technique)
    {
        if (!base.OnAttach(technique)) return false;

        geometryBuffer = new SkyDomeBufferModel();

        return true;
    }

    protected override void OnDetach()
    {
        RemoveAndDispose(geometryBuffer);
        geometryBuffer = null;

        base.OnDetach();
    }

    protected override bool OnUpdateCanRenderFlag()
    {
        return base.OnUpdateCanRenderFlag() && geometryBuffer != null;
    }

    public override void Render(RenderContext context, DeviceContextProxy deviceContext)
    {
        if (_scene == null) return;
        if (IsRenderValid && _scene.LastUpdated == lastSceneUpdate) return;

        //if (_scene.IsRenderValid && _cubeMap != null) {
        //if (!context.UpdateSceneGraphRequested && !context.UpdatePerFrameRenderableRequested) return;
        //}

        if (CreateCubeMapResources()) {
            RaiseInvalidateRender();
            return;
        }

        base.Render(context, deviceContext);
            
        deviceContext.GenerateMips(_cubeMap);
        context.SharedResource.EnvironmentMapMipLevels = _cubeMap.TextureView.Description.TextureCube.MipLevels;
            
        context.UpdatePerFrameData(true, false, deviceContext);
        _scene.Apply(deviceContext);

        //_scene.ResetValidation();
        lastSceneUpdate = _scene.LastUpdated;
        //IsRenderValid = true;
    }

    protected override void RenderFace(RenderContext context, DeviceContextProxy deviceContext)
    {
        var vertexStartSlot = 0;
        geometryBuffer.AttachBuffers(deviceContext, ref vertexStartSlot, EffectTechnique.EffectsManager);

        Scene.Apply(deviceContext);

        deviceContext.DrawIndexed(geometryBuffer.IndexBuffer.ElementCount, 0, 0);
    }
}