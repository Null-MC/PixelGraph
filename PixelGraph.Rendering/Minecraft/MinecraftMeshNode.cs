using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Render;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.Rendering.Minecraft
{
    public class MinecraftMeshNode : SceneNode, IMinecraftMesh
    {
        private MinecraftMeshCore MeshCore => RenderCore as MinecraftMeshCore;
        //public bool IsRenderValid => MeshCore.IsRenderValid;
        public long LastUpdated => MeshCore.LastUpdated;

        public int BlendMode {
            get => MeshCore.BlendMode;
            set => MeshCore.BlendMode = value;
        }

        public Vector3 TintColor {
            get => MeshCore.TintColor;
            set => MeshCore.TintColor = value;
        }

        public float ParallaxDepth {
            get => MeshCore.ParallaxDepth;
            set => MeshCore.ParallaxDepth = value;
        }

        public int ParallaxSamples {
            get => MeshCore.ParallaxSamples;
            set => MeshCore.ParallaxSamples = value;
        }

        public int WaterMode {
            get => MeshCore.WaterMode;
            set => MeshCore.WaterMode = value;
        }


        public void Apply(DeviceContextProxy deviceContext)
        {
            MeshCore?.Apply(deviceContext);
        }

        //public void ResetValidation()
        //{
        //    MeshCore?.ResetValidation();
        //}

        protected override RenderCore OnCreateRenderCore()
        {
            return new MinecraftMeshCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);
            if (core is not MinecraftMeshCore meshCore) return;

            meshCore.BlendMode = BlendMode;
            meshCore.TintColor = TintColor;
            meshCore.ParallaxDepth = ParallaxDepth;
            meshCore.ParallaxSamples = ParallaxSamples;
            meshCore.WaterMode = WaterMode;
        }

        protected override bool CanHitTest(HitTestContext context) => false;

        protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
