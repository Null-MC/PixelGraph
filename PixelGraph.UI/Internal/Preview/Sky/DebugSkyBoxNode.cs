using System.Collections.Generic;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using PixelGraph.UI.Internal.Preview.CubeMaps;
using SharpDX;

namespace PixelGraph.UI.Internal.Preview.Sky
{
    public class DebugSkyBoxNode : SceneNode
    {
        public ICubeMapSource CubeMapSource {
            get => ((DebugSkyBoxCore)RenderCore).CubeMapSource;
            set => ((DebugSkyBoxCore)RenderCore).CubeMapSource = value;
        }


        public DebugSkyBoxNode()
        {
            RenderOrder = 1_000;
        }

        protected override RenderCore OnCreateRenderCore()
        {
            return new DebugSkyBoxCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[DefaultRenderTechniqueNames.Skybox];
        }

        public sealed override bool HitTest(HitTestContext context, ref List<HitTestResult> hits) => false;

        protected sealed override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
