using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Minecraft;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering.CubeMaps;

public class DynamicSkyCubeNode : SceneNode, ICubeMapSource
{
    public IMinecraftScene Scene {
        get => EnvCubeCore.Scene;
        set => EnvCubeCore.Scene = value;
    }

    public int FaceSize {
        get => EnvCubeCore.FaceSize;
        set => EnvCubeCore.FaceSize = value;
    }

    private DynamicSkyCubeCore EnvCubeCore => RenderCore as DynamicSkyCubeCore;
    public ShaderResourceViewProxy CubeMap => EnvCubeCore?.CubeMap;
    public long LastUpdated => EnvCubeCore?.LastUpdated ?? 0;


    //public EnvironmentCubeNode()
    //{
    //    RenderOrder = 1_000;
    //}

    protected override RenderCore OnCreateRenderCore()
    {
        return new DynamicSkyCubeCore();
    }

    protected override void AssignDefaultValuesToCore(RenderCore core)
    {
        base.AssignDefaultValuesToCore(core);
        if (core is not DynamicSkyCubeCore c) return;

        c.Scene = Scene;
        c.FaceSize = FaceSize;
    }

    protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
    {
        return effectsManager[CustomRenderTechniqueNames.DynamicSkybox];
    }

    protected override bool CanHitTest(HitTestContext context) => false;

    protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
}