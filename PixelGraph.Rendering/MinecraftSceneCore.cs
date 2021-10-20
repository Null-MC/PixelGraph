using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering
{
    public interface IMinecraftScene
    {
        bool IsRenderValid {get;}

        void Apply(DeviceContextProxy deviceContext);
        void ResetValidation();
    }

    internal class MinecraftSceneCore : RenderCore, IMinecraftScene
    {
        private readonly ConstantBufferComponent buffer;
        private MinecraftSceneStruct data;

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

        public bool EnableLinearSampling {
            get => data.EnableLinearSampling;
            set => SetAffectsRender(ref data.EnableLinearSampling, value);
        }

        public bool EnableSlopeNormals {
            get => data.EnableSlopeNormals;
            set => SetAffectsRender(ref data.EnableSlopeNormals, value);
        }

        public bool EnablePuddles {
            get => data.EnablePuddles;
            set => SetAffectsRender(ref data.EnablePuddles, value);
        }


        public MinecraftSceneCore() : base(RenderType.PreProc)
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
            Apply(deviceContext);
        }
    }
}
