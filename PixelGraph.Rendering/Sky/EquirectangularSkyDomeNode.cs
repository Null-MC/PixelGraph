using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering.Sky;

public class EquirectangularSkyDomeNode : SceneNode
{
    public TextureModel? Texture {
        get => ((EquirectangularSkyDomeCore)RenderCore).Texture;
        set => ((EquirectangularSkyDomeCore)RenderCore).Texture = value;
    }


    public EquirectangularSkyDomeNode()
    {
        RenderOrder = 1_000;
    }

    protected override RenderCore OnCreateRenderCore()
    {
        return new EquirectangularSkyDomeCore();
    }

    protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
    {
        return effectsManager[CustomRenderTechniqueNames.DynamicSkybox];
    }

    public sealed override bool HitTest(HitTestContext context, ref List<HitTestResult> hits) => false;

    protected sealed override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;

    protected override bool CanRender(RenderContext context)
    {
        if (base.CanRender(context)) return true;
        context.SharedResource.EnvironementMap = null;
        return false;
    }
}