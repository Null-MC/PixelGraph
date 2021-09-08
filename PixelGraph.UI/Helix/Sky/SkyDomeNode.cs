using System.Collections.Generic;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using PixelGraph.UI.Helix.Shaders;
using SharpDX;

namespace PixelGraph.UI.Helix.Sky
{
    public class SkyDomeNode : SceneNode
    {
        public SkyDomeNode()
        {
            RenderOrder = 1_000;
        }

        protected override RenderCore OnCreateRenderCore()
        {
            return new SkyDomeCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[CustomRenderTechniqueNames.DynamicSkybox];
        }

        public sealed override bool HitTest(HitTestContext context, ref List<HitTestResult> hits) => false;

        protected sealed override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
