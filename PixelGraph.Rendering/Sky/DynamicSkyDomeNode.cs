using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering.Sky;

public class DynamicSkyDomeNode : SceneNode
{
    public DynamicSkyDomeNode()
    {
        RenderOrder = 1_000;
    }

    protected override RenderCore OnCreateRenderCore()
    {
        return new DynamicSkyDomeCore();
    }

    protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
    {
        return effectsManager[CustomRenderTechniqueNames.DynamicSkybox];
    }

    public sealed override bool HitTest(HitTestContext context, ref List<HitTestResult> hits) => false;

    protected sealed override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
}