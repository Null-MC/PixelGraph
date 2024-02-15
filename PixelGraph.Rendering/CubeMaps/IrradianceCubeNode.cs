using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using SharpDX.Direct3D11;

namespace PixelGraph.Rendering.CubeMaps;

public class IrradianceCubeNode : SceneNode, ICubeMapSource
{
    public ICubeMapSource? EnvironmentCubeMapSource {
        get => ((IrradianceCubeCore)RenderCore).EnvironmentCubeMapSource;
        set => ((IrradianceCubeCore)RenderCore).EnvironmentCubeMapSource = value;
    }

    public SamplerStateDescription SamplerDescription {
        get => ((IrradianceCubeCore)RenderCore).SamplerDescription;
        set => ((IrradianceCubeCore)RenderCore).SamplerDescription = value;
    }

    public int FaceSize {
        get => ((IrradianceCubeCore)RenderCore).FaceSize;
        set => ((IrradianceCubeCore)RenderCore).FaceSize = value;
    }

    public ShaderResourceViewProxy? CubeMap => ((IrradianceCubeCore)RenderCore).CubeMap;
    public long LastUpdated => ((IrradianceCubeCore)RenderCore).LastUpdated;


    //public IrradianceCubeNode()
    //{
    //    RenderOrder = 1_000;
    //}

    protected override RenderCore OnCreateRenderCore()
    {
        return new IrradianceCubeCore();
    }

    protected override void AssignDefaultValuesToCore(RenderCore core)
    {
        base.AssignDefaultValuesToCore(core);
        if (core is not IrradianceCubeCore c) return;

        c.EnvironmentCubeMapSource = EnvironmentCubeMapSource;
        c.SamplerDescription = SamplerDescription;
        c.FaceSize = FaceSize;
    }

    protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
    {
        return effectsManager[CustomRenderTechniqueNames.DynamicSkybox];
    }

    protected override bool CanHitTest(HitTestContext context) => false;

    protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
}