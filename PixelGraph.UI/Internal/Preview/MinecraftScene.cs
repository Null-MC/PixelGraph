using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Preview.Shaders;
using SharpDX;

namespace PixelGraph.UI.Internal.Preview
{
    public interface IMinecraftScene
    {
        //event EventHandler EnvironmentChanged;
        bool IsRenderValid {get;}

        //Vector3 SunDirection {get; set;}
        //float SunStrength {get; set;}
        //float TimeOfDay {get; set;}
        //float Wetness {get; set;}
        //float ParallaxDepth {get; set;}
        //int ParallaxSamplesMin {get; set;}
        //int ParallaxSamplesMax {get; set;}

        void Apply(DeviceContextProxy deviceContext);
        void ResetValidation();
    }

    internal class MinecraftSceneCore : RenderCore, IMinecraftScene
    {
        private readonly ConstantBufferComponent buffer;
        private MinecraftSceneStruct data;

        //public event EventHandler EnvironmentChanged;

        public bool IsRenderValid {get; private set;}

        public Vector3 SunDirection {
            get => data.SunDirection;
            set {
                if (SetAffectsRender(ref data.SunDirection, value))
                    IsRenderValid = false;
            }
        }

        public float SunStrength {
            get => data.SunStrength;
            set {
                if (SetAffectsRender(ref data.SunStrength, value))
                    IsRenderValid = false;
            }
        }

        public float TimeOfDay {
            get => data.TimeOfDay;
            set {
                if (SetAffectsRender(ref data.TimeOfDay, value))
                    IsRenderValid = false;
            }
        }

        public float Wetness {
            get => data.Wetness;
            set {
                if (SetAffectsRender(ref data.Wetness, value))
                    IsRenderValid = false;
            }
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


        public MinecraftSceneCore() : base(RenderType.None)
        {
            var bufferDesc = new ConstantBufferDescription(CustomBufferNames.MinecraftSceneCB, MinecraftSceneStruct.SizeInBytes);
            buffer = AddComponent(new ConstantBufferComponent(bufferDesc));
        }

        public void Apply(DeviceContextProxy deviceContext)
        {
            buffer.Upload(deviceContext, ref data);
        }

        public void ResetValidation()
        {
            IsRenderValid = true;
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            return true;
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            //
        }
    }
}
