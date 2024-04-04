using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft;

internal class MinecraftMeshCore : RenderCore
{
    private readonly ConstantBufferComponent buffer;
    private MinecraftMeshStruct data;
    private bool isRenderValid;

    public int BlendMode {
        get => data.BlendMode;
        set {
            if (SetAffectsRender(ref data.BlendMode, value))
                isRenderValid = false;
        }
    }

    public Vector3 TintColor {
        get => data.TintColor;
        set {
            if (SetAffectsRender(ref data.TintColor, value))
                isRenderValid = false;
        }
    }

    public bool EnableLinearSampling {
        get => data.EnableLinearSampling;
        set {
            if (SetAffectsRender(ref data.EnableLinearSampling, value))
                isRenderValid = false;
        }
    }

    public bool EnableSlopeNormals {
        get => data.EnableSlopeNormals;
        set {
            if (SetAffectsRender(ref data.EnableSlopeNormals, value))
                isRenderValid = false;
        }
    }

    public float ParallaxDepth {
        get => data.ParallaxDepth;
        set => SetAffectsRender(ref data.ParallaxDepth, value);
    }

    public int ParallaxSamples {
        get => data.ParallaxSamples;
        set => SetAffectsRender(ref data.ParallaxSamples, value);
    }

    public int WaterMode {
        get => data.WaterMode;
        set => SetAffectsRender(ref data.WaterMode, value);
    }

    public float SubSurfaceBlur {
        get => data.SubSurfaceBlur;
        set => SetAffectsRender(ref data.SubSurfaceBlur, value);
    }


    public MinecraftMeshCore() : base(RenderType.PreProc)
    {
        var bufferDesc = new ConstantBufferDescription(CustomBufferNames.MinecraftMeshCB, MinecraftMeshStruct.SizeInBytes);
        buffer = AddComponent(new ConstantBufferComponent(bufferDesc));
    }

    public void Apply(DeviceContextProxy deviceContext)
    {
        buffer.Upload(deviceContext, ref data);
    }

    protected override bool OnAttach(IRenderTechnique technique)
    {
        return true;
    }

    protected override void OnDetach()
    {
        // TODO: ?
    }

    public override void Render(RenderContext context, DeviceContextProxy deviceContext)
    {
        if (isRenderValid && !context.updateSceneGraphRequested) return;

        Apply(deviceContext);
        isRenderValid = true;
    }
}