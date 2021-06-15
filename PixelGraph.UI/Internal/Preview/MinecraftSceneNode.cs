using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Render;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview
{
    public class MinecraftSceneNode : SceneNode, IMinecraftScene
    {
        public bool IsRenderValid => ((MinecraftSceneCore) RenderCore).IsRenderValid;

        public float TimeOfDay {
            get => ((MinecraftSceneCore) RenderCore).TimeOfDay;
            set => ((MinecraftSceneCore) RenderCore).TimeOfDay = value;
        }

        public Vector3 SunDirection {
            get => ((MinecraftSceneCore) RenderCore).SunDirection;
            set => ((MinecraftSceneCore) RenderCore).SunDirection = value;
        }

        public float SunStrength {
            get => ((MinecraftSceneCore) RenderCore).SunStrength;
            set => ((MinecraftSceneCore) RenderCore).SunStrength = value;
        }

        public float Wetness {
            get => ((MinecraftSceneCore) RenderCore).Wetness;
            set => ((MinecraftSceneCore) RenderCore).Wetness = value;
        }

        public float ParallaxDepth {
            get => ((MinecraftSceneCore) RenderCore).ParallaxDepth;
            set => ((MinecraftSceneCore) RenderCore).ParallaxDepth = value;
        }

        public int ParallaxSamplesMin {
            get => ((MinecraftSceneCore) RenderCore).ParallaxSamplesMin;
            set => ((MinecraftSceneCore) RenderCore).ParallaxSamplesMin = value;
        }

        public int ParallaxSamplesMax {
            get => ((MinecraftSceneCore) RenderCore).ParallaxSamplesMax;
            set => ((MinecraftSceneCore) RenderCore).ParallaxSamplesMax = value;
        }

        public bool EnableLinearSampling {
            get => ((MinecraftSceneCore) RenderCore).EnableLinearSampling;
            set => ((MinecraftSceneCore) RenderCore).EnableLinearSampling = value;
        }


        public void Apply(DeviceContextProxy deviceContext)
        {
            if (RenderCore is MinecraftSceneCore sceneCore)
                sceneCore.Apply(deviceContext);
        }

        public void ResetValidation()
        {
            if (RenderCore is MinecraftSceneCore sceneCore)
                sceneCore.ResetValidation();
        }

        protected override RenderCore OnCreateRenderCore()
        {
            return new MinecraftSceneCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);
            if (core is not MinecraftSceneCore sceneCore) return;

            sceneCore.TimeOfDay = TimeOfDay;
            sceneCore.SunDirection = SunDirection;
            sceneCore.Wetness = Wetness;
            sceneCore.ParallaxDepth = ParallaxDepth;
            sceneCore.ParallaxSamplesMin = ParallaxSamplesMin;
            sceneCore.ParallaxSamplesMax = ParallaxSamplesMax;
            sceneCore.EnableLinearSampling = EnableLinearSampling;
        }

        protected override bool CanHitTest(HitTestContext context) => false;

        protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
