using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Render;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.Rendering.Minecraft
{
    public class MinecraftSceneNode : SceneNode, IMinecraftScene
    {
        private MinecraftSceneCore SceneCore => RenderCore as MinecraftSceneCore;
        //public bool IsRenderValid => SceneCore.IsRenderValid;
        public long LastUpdated => SceneCore.LastUpdated;

        public bool EnableAtmosphere {
            get => SceneCore.EnableAtmosphere;
            set => SceneCore.EnableAtmosphere = value;
        }

        public float TimeOfDay {
            get => SceneCore.TimeOfDay;
            set => SceneCore.TimeOfDay = value;
        }

        public Vector3 SunDirection {
            get => SceneCore.SunDirection;
            set => SceneCore.SunDirection = value;
        }

        public float SunStrength {
            get => SceneCore.SunStrength;
            set => SceneCore.SunStrength = value;
        }

        public float Wetness {
            get => SceneCore.Wetness;
            set => SceneCore.Wetness = value;
        }

        //public float ParallaxDepth {
        //    get => SceneCore.ParallaxDepth;
        //    set => SceneCore.ParallaxDepth = value;
        //}

        //public int ParallaxSamples {
        //    get => SceneCore.ParallaxSamples;
        //    set => SceneCore.ParallaxSamples = value;
        //}

        public bool EnableLinearSampling {
            get => SceneCore.EnableLinearSampling;
            set => SceneCore.EnableLinearSampling = value;
        }

        public bool EnableSlopeNormals {
            get => SceneCore.EnableSlopeNormals;
            set => SceneCore.EnableSlopeNormals = value;
        }

        //public int WaterMode {
        //    get => SceneCore.WaterMode;
        //    set => SceneCore.WaterMode = value;
        //}

        public float ErpExposure {
            get => SceneCore.ErpExposure;
            set => SceneCore.ErpExposure = value;
        }


        public void Apply(DeviceContextProxy deviceContext)
        {
            SceneCore?.Apply(deviceContext);
        }

        //public void ResetValidation()
        //{
        //    SceneCore?.ResetValidation();
        //}

        protected override RenderCore OnCreateRenderCore()
        {
            return new MinecraftSceneCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);
            if (core is not MinecraftSceneCore sceneCore) return;

            sceneCore.EnableAtmosphere = EnableAtmosphere;
            sceneCore.TimeOfDay = TimeOfDay;
            sceneCore.SunDirection = SunDirection;
            sceneCore.Wetness = Wetness;
            //sceneCore.ParallaxDepth = ParallaxDepth;
            //sceneCore.ParallaxSamples = ParallaxSamples;
            sceneCore.EnableLinearSampling = EnableLinearSampling;
            sceneCore.EnableSlopeNormals = EnableSlopeNormals;
            //sceneCore.WaterMode = WaterMode;
            sceneCore.ErpExposure = ErpExposure;
        }

        protected override bool CanHitTest(HitTestContext context) => false;

        protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
