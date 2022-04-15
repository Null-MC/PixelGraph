using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using System.Collections.Generic;

namespace PixelGraph.Rendering.LUTs
{
    public class DielectricBdrfLutNode : SceneNode, ILutMapSource
    {
        public int Resolution {
            get => LutMapCore.Resolution;
            set => LutMapCore.Resolution = value;
        }

        private DielectricBrdfLutMapCore LutMapCore => RenderCore as DielectricBrdfLutMapCore;
        public ShaderResourceViewProxy LutMap => LutMapCore?.LutMap;
        public long LastUpdated => LutMapCore?.LastUpdated ?? 0;


        //public DielectricBdrfLutNode()
        //{
        //    RenderOrder = 1_000;
        //}

        protected override RenderCore OnCreateRenderCore()
        {
            return new DielectricBrdfLutMapCore();
        }

        protected override void AssignDefaultValuesToCore(RenderCore core)
        {
            base.AssignDefaultValuesToCore(core);
            if (core is not DielectricBrdfLutMapCore c) return;

            c.Resolution = Resolution;
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[CustomRenderTechniqueNames.BdrfDielectricLut];
        }

        protected override bool CanHitTest(HitTestContext context) => false;

        protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
    }
}
