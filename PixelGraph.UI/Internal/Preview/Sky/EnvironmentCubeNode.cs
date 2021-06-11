using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.UI.Internal.Preview.Shaders;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Sky
{
    public class EnvironmentCubeNode : SceneNode, IEnvironmentCube
    {
        public IMinecraftScene Scene {
            get => ((EnvironmentCubeCore)RenderCore).Scene;
            set => ((EnvironmentCubeCore)RenderCore).Scene = value;
        }

        //public Vector3 SunDirection {
        //    get => ((EnvironmentCubeCore)RenderCore).SunDirection;
        //    set => ((EnvironmentCubeCore)RenderCore).SunDirection = value;
        //}

        //public float SunStrength {
        //    get => ((EnvironmentCubeCore)RenderCore).SunStrength;
        //    set => ((EnvironmentCubeCore)RenderCore).SunStrength = value;
        //}

        //public float TimeOfDay {
        //    get => ((EnvironmentCubeCore)RenderCore).TimeOfDay;
        //    set => ((EnvironmentCubeCore)RenderCore).TimeOfDay = value;
        //}

        //public float Wetness {
        //    get => ((EnvironmentCubeCore)RenderCore).Wetness;
        //    set => ((EnvironmentCubeCore)RenderCore).Wetness = value;
        //}

        public int FaceSize {
            get => ((EnvironmentCubeCore)RenderCore).FaceSize;
            set => ((EnvironmentCubeCore)RenderCore).FaceSize = value;
        }

        public ShaderResourceViewProxy CubeMap => ((EnvironmentCubeCore)RenderCore).CubeMap;


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
            //c.SunDirection = SunDirection;
            //c.SunStrength = SunStrength;
            //c.TimeOfDay = TimeOfDay;
            //c.Wetness = Wetness;
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
