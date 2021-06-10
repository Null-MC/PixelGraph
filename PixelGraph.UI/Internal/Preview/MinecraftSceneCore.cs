using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Preview.Shaders;
using SharpDX;

namespace PixelGraph.UI.Internal.Preview
{
    internal class MinecraftSceneCore : RenderCore
    {
        private readonly ConstantBufferComponent buffer;
        private MinecraftSceneStruct data;

        public float TimeOfDay {
            get => data.TimeOfDay;
            set => SetAffectsRender(ref data.TimeOfDay, value);
        }

        public Vector3 SunDirection {
            get => data.SunDirection;
            set => SetAffectsRender(ref data.SunDirection, value);
        }

        public float Wetness {
            get => data.Wetness;
            set => SetAffectsRender(ref data.Wetness, value);
        }

        public float ParallaxDepth {
            get => data.ParallaxDepth;
            set => SetAffectsRender(ref data.ParallaxDepth, value);
        }

        public int ParallaxSamplesMin {
            get => data.ParallaxSamplesMin;
            set => SetAffectsRender(ref data.ParallaxSamplesMin, value);
        }

        public int ParallaxSamplesMax {
            get => data.ParallaxSamplesMax;
            set => SetAffectsRender(ref data.ParallaxSamplesMax, value);
        }


        public MinecraftSceneCore() : base(RenderType.PreProc)
        {
            var bufferDesc = new ConstantBufferDescription(CustomBufferNames.MinecraftSceneCB, MinecraftSceneStruct.SizeInBytes);
            buffer = AddComponent(new ConstantBufferComponent(bufferDesc));
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            buffer.Upload(deviceContext, ref data);
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            return true;
        }
    }
}
