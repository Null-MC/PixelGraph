using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering.Minecraft
{
    public interface IMinecraftMesh
    {
        //bool IsRenderValid {get;}
        long LastUpdated {get;}

        void Apply(DeviceContextProxy deviceContext);
        //void ResetValidation();
    }

    internal class MinecraftMeshCore : RenderCore, IMinecraftMesh
    {
        private readonly ConstantBufferComponent buffer;
        private MinecraftMeshStruct data;
        private bool isRenderValid;

        //public bool IsRenderValid {get; private set;}
        public long LastUpdated {get; private set;}

        public int BlendMode {
            get => data.BlendMode;
            set {
                if (SetAffectsRender(ref data.BlendMode, value))
                    isRenderValid = false;
            }
        }

        public Vector3 TintColor {
            get => data.TintColor;
            set {
                if (SetAffectsRender(ref data.TintColor, value))
                    isRenderValid = false;
            }
        }

        public float ParallaxDepth {
            get => data.ParallaxDepth;
            set => SetAffectsRender(ref data.ParallaxDepth, value);
        }

        public int ParallaxSamples {
            get => data.ParallaxSamples;
            set => SetAffectsRender(ref data.ParallaxSamples, value);
        }

        public int WaterMode {
            get => data.WaterMode;
            set => SetAffectsRender(ref data.WaterMode, value);
        }


        public MinecraftMeshCore() : base(RenderType.PreProc)
        {
            var bufferDesc = new ConstantBufferDescription(CustomBufferNames.MinecraftMeshCB, MinecraftMeshStruct.SizeInBytes);
            buffer = AddComponent(new ConstantBufferComponent(bufferDesc));
        }

        public void Apply(DeviceContextProxy deviceContext)
        {
            buffer.Upload(deviceContext, ref data);
        }

        //public void ResetValidation()
        //{
        //    isRenderValid = true;
        //}

        protected override bool OnAttach(IRenderTechnique technique)
        {
            return true;
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            if (isRenderValid && !context.UpdateSceneGraphRequested) return;

            Apply(deviceContext);
            isRenderValid = true;
        }
    }
}
