using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.UI.Internal.Preview.Shaders;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.CubeMaps
{
    public class IrradianceCubeNode : SceneNode, ICubeMapSource
    {
        public ICubeMapSource EnvironmentCubeMapSource {
            get => ((IrradianceCubeCore)RenderCore).EnvironmentCubeMapSource;
            set => ((IrradianceCubeCore)RenderCore).EnvironmentCubeMapSource = value;
        }

        public int FaceSize {
            get => ((IrradianceCubeCore)RenderCore).FaceSize;
            set => ((IrradianceCubeCore)RenderCore).FaceSize = value;
        }

        public ShaderResourceViewProxy CubeMap => ((IrradianceCubeCore)RenderCore).CubeMap;
        public long LastUpdated => ((IrradianceCubeCore)RenderCore)?.LastUpdated ?? 0;


        //public IrradianceCubeNode()
        //{
        //    RenderOrder = 1_000;
        //}

        protected override RenderCore OnCreateRenderCore()
        {
            return new IrradianceCubeCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);
            if (core is not IrradianceCubeCore c) return;

            c.EnvironmentCubeMapSource = EnvironmentCubeMapSource;
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
