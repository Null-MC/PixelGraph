using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview
{
    public class MinecraftSceneNode : SceneNode
    {
        public float TimeOfDay {
            get => (RenderCore as MinecraftSceneCore).TimeOfDay;
            set => (RenderCore as MinecraftSceneCore).TimeOfDay = value;
        }

        public Vector3 SunDirection {
            get => (RenderCore as MinecraftSceneCore).SunDirection;
            set => (RenderCore as MinecraftSceneCore).SunDirection = value;
        }

        public float Wetness {
            get => (RenderCore as MinecraftSceneCore).Wetness;
            set => (RenderCore as MinecraftSceneCore).Wetness = value;
        }

        public float ParallaxDepth {
            get => (RenderCore as MinecraftSceneCore).ParallaxDepth;
            set => (RenderCore as MinecraftSceneCore).ParallaxDepth = value;
        }

        public int ParallaxSamplesMin {
            get => (RenderCore as MinecraftSceneCore).ParallaxSamplesMin;
            set => (RenderCore as MinecraftSceneCore).ParallaxSamplesMin = value;
        }

        public int ParallaxSamplesMax {
            get => (RenderCore as MinecraftSceneCore).ParallaxSamplesMax;
            set => (RenderCore as MinecraftSceneCore).ParallaxSamplesMax = value;
        }


        protected override RenderCore OnCreateRenderCore()
        {
            return new MinecraftSceneCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);

            if (core is MinecraftSceneCore c) {
                c.TimeOfDay = TimeOfDay;
                c.SunDirection = SunDirection;
                c.Wetness = Wetness;
                c.ParallaxDepth = ParallaxDepth;
                c.ParallaxSamplesMin = ParallaxSamplesMin;
                c.ParallaxSamplesMax = ParallaxSamplesMax;
            }
        }

        protected override bool CanHitTest(HitTestContext context) => false;

        protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
