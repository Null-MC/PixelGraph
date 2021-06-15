using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.UI.Internal.Preview.Shaders;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.CubeMaps
{
    public class EnvironmentCubeNode : SceneNode, ICubeMapSource
    {
        public IMinecraftScene Scene {
            get => ((EnvironmentCubeCore)RenderCore).Scene;
            set => ((EnvironmentCubeCore)RenderCore).Scene = value;
        }

        public int FaceSize {
            get => ((EnvironmentCubeCore)RenderCore).FaceSize;
            set => ((EnvironmentCubeCore)RenderCore).FaceSize = value;
        }

        public ShaderResourceViewProxy CubeMap => ((EnvironmentCubeCore)RenderCore).CubeMap;
        public long LastUpdated => ((EnvironmentCubeCore)RenderCore)?.LastUpdated ?? 0;


        //public EnvironmentCubeNode()
        //{
        //    RenderOrder = 1_000;
        //}

        protected override RenderCore OnCreateRenderCore()
        {
            return new EnvironmentCubeCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);
            if (core is not EnvironmentCubeCore c) return;

            c.Scene = Scene;
            c.FaceSize = FaceSize;
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[CustomRenderTechniqueNames.DynamicSkybox];
        }

        protected override bool CanHitTest(HitTestContext context) => false;

        protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
