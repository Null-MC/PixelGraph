using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Minecraft;
//public interface IMinecraftSceneCore
//{
//    //bool IsRenderValid {get;}
//    long LastUpdated {get;}

//    void Apply(DeviceContextProxy deviceContext);
//    //void ResetValidation();
//}

public interface IMinecraftScene //: IMinecraftSceneCore
{
    long LastUpdated {get;}

    void Apply(DeviceContextProxy deviceContext);
}

internal class MinecraftSceneCore : RenderCore, IMinecraftScene
{
    private readonly ConstantBufferComponent buffer;
    private MinecraftSceneStruct data;
    private bool isRenderValid;

    public long LastUpdated {get; private set;}

    public bool EnableAtmosphere {
        get => data.EnableAtmosphere;
        set => SetAffectsRender(ref data.EnableAtmosphere, value);
    }

    public Vector3 SunDirection {
        get => data.SunDirection;
        set {
            if (SetAffectsRender(ref data.SunDirection, value))
                isRenderValid = false;
        }
    }

    public float SunStrength {
        get => data.SunStrength;
        set {
            if (SetAffectsRender(ref data.SunStrength, value))
                isRenderValid = false;
        }
    }

    public float TimeOfDay {
        get => data.TimeOfDay;
        set {
            if (SetAffectsRender(ref data.TimeOfDay, value))
                isRenderValid = false;
        }
    }

    public float Wetness {
        get => data.Wetness;
        set {
            if (SetAffectsRender(ref data.Wetness, value))
                isRenderValid = false;
        }
    }

    public float ErpExposure {
        get => data.ErpExposure;
        set {
            if (SetAffectsRender(ref data.ErpExposure, value))
                isRenderValid = false;
        }
    }


    public MinecraftSceneCore() : base(RenderType.PreProc)
    {
        var bufferDesc = new ConstantBufferDescription(CustomBufferNames.MinecraftSceneCB, MinecraftSceneStruct.SizeInBytes);
        buffer = AddComponent(new ConstantBufferComponent(bufferDesc));

        data.ErpExposure = 1f;
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

        LastUpdated = Environment.TickCount64;
        isRenderValid = true;
    }
}