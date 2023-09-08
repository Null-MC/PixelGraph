using HelixToolkit.SharpDX.Core;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PixelGraph.Rendering.LUTs;

internal class DielectricBrdfLutMapCore : LutMapRenderCore
{
    public DielectricBrdfLutMapCore() : base(RenderType.PreProc)
    {
        //PassName = CustomRenderTechniqueNames.BdrfDielectricLut;

        TextureDesc = new Texture2DDescription {
            Format = Format.R16G16_Float,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            OptionFlags = ResourceOptionFlags.None,
            SampleDescription = new SampleDescription(1, 0),
            CpuAccessFlags = CpuAccessFlags.None,
            Usage = ResourceUsage.Default,
            ArraySize = 1,
            MipLevels = 0,
        };
    }

    //protected override bool OnUpdateCanRenderFlag()
    //{
    //    return base.OnUpdateCanRenderFlag() && geometryBuffer != null;
    //}

    //public override void Render(RenderContext context, DeviceContextProxy deviceContext)
    //{
    //    if (_scene == null || _scene.IsRenderValid) {
    //        if (!context.UpdateSceneGraphRequested && !context.UpdatePerFrameRenderableRequested) return;
    //    }

    //    base.Render(context, deviceContext);
            
    //    //deviceContext.GenerateMips(CubeMap);
    //    //context.SharedResource.EnvironmentMapMipLevels = CubeMap.TextureView.Description.TextureCube.MipLevels;
            
    //    context.UpdatePerFrameData(true, false, deviceContext);
    //    //_scene?.Apply(deviceContext);

    //    //_scene?.ResetValidation();
    //}

    //protected override void RenderLut(RenderContext context, DeviceContextProxy deviceContext)
    //{
    //    //Scene.Apply(deviceContext);

    //    var vertexStartSlot = 0;
    //    geometryBuffer.AttachBuffers(deviceContext, ref vertexStartSlot, EffectTechnique.EffectsManager);

    //    deviceContext.DrawIndexed(geometryBuffer.IndexBuffer.ElementCount, 0, 0);
    //}

    //protected override bool OnAttach(IRenderTechnique technique)
    //{
    //    geometryBuffer = Collect(new SkyDomeBufferModel());

    //    return base.OnAttach(technique);
    //}

    //protected override void OnDetach()
    //{
    //    geometryBuffer = null;

    //    base.OnDetach();
    //}
}