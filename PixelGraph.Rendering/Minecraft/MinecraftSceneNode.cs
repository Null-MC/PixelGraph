using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Render;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft;

public class MinecraftSceneNode : SceneNode, IMinecraftScene
{
    private MinecraftSceneCore SceneCore => (MinecraftSceneCore)RenderCore;
    public long LastUpdated => SceneCore.LastUpdated;

    public bool EnableAtmosphere {
        get => SceneCore.EnableAtmosphere;
        set => SceneCore.EnableAtmosphere = value;
    }

    public float TimeOfDay {
        get => SceneCore.TimeOfDay;
        set => SceneCore.TimeOfDay = value;
    }

    public Vector3 SunDirection {
        get => SceneCore.SunDirection;
        set => SceneCore.SunDirection = value;
    }

    public float SunStrength {
        get => SceneCore.SunStrength;
        set => SceneCore.SunStrength = value;
    }

    public float Wetness {
        get => SceneCore.Wetness;
        set => SceneCore.Wetness = value;
    }

    public float ErpExposure {
        get => SceneCore.ErpExposure;
        set => SceneCore.ErpExposure = value;
    }


    public void Apply(DeviceContextProxy deviceContext)
    {
        SceneCore.Apply(deviceContext);
    }

    protected override RenderCore OnCreateRenderCore()
    {
        return new MinecraftSceneCore();
    }

    protected override void AssignDefaultValuesToCore(RenderCore core)
    {
        base.AssignDefaultValuesToCore(core);
        if (core is not MinecraftSceneCore sceneCore) return;

        sceneCore.EnableAtmosphere = EnableAtmosphere;
        sceneCore.TimeOfDay = TimeOfDay;
        sceneCore.SunDirection = SunDirection;
        sceneCore.Wetness = Wetness;
        sceneCore.ErpExposure = ErpExposure;
    }

    protected override bool CanHitTest(HitTestContext context) => false;

    protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
}