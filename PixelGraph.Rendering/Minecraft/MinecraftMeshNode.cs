using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.Rendering.Minecraft;

public class MinecraftMeshNode : SceneNode
{
    private MinecraftMeshCore MeshCore => RenderCore as MinecraftMeshCore;

    public int BlendMode {
        get => MeshCore.BlendMode;
        set => MeshCore.BlendMode = value;
    }

    public Vector3 TintColor {
        get => MeshCore.TintColor;
        set => MeshCore.TintColor = value;
    }

    public float ParallaxDepth {
        get => MeshCore.ParallaxDepth;
        set => MeshCore.ParallaxDepth = value;
    }

    public int ParallaxSamples {
        get => MeshCore.ParallaxSamples;
        set => MeshCore.ParallaxSamples = value;
    }

    public bool EnableLinearSampling {
        get => MeshCore.EnableLinearSampling;
        set => MeshCore.EnableLinearSampling = value;
    }

    public bool EnableSlopeNormals {
        get => MeshCore.EnableSlopeNormals;
        set => MeshCore.EnableSlopeNormals = value;
    }

    public int WaterMode {
        get => MeshCore.WaterMode;
        set => MeshCore.WaterMode = value;
    }

    public float SubSurfaceBlur {
        get => MeshCore.SubSurfaceBlur;
        set => MeshCore.SubSurfaceBlur = value;
    }


    //public void Apply(DeviceContextProxy deviceContext)
    //{
    //    MeshCore?.Apply(deviceContext);
    //}

    protected override RenderCore OnCreateRenderCore()
    {
        return new MinecraftMeshCore();
    }

    protected override void AssignDefaultValuesToCore(RenderCore core)
    {
        base.AssignDefaultValuesToCore(core);
        if (core is not MinecraftMeshCore meshCore) return;

        meshCore.BlendMode = BlendMode;
        meshCore.TintColor = TintColor;
        meshCore.ParallaxDepth = ParallaxDepth;
        meshCore.ParallaxSamples = ParallaxSamples;
        meshCore.EnableLinearSampling = EnableLinearSampling;
        meshCore.EnableSlopeNormals = EnableSlopeNormals;
        meshCore.WaterMode = WaterMode;
        meshCore.SubSurfaceBlur = SubSurfaceBlur;
    }

    protected override bool CanHitTest(HitTestContext context) => false;

    protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
}