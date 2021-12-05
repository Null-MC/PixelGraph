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
        bool IsRenderValid {get;}

        void Apply(DeviceContextProxy deviceContext);
        void ResetValidation();
    }

    internal class MinecraftMeshCore : RenderCore, IMinecraftMesh
    {
        private readonly ConstantBufferComponent buffer;
        private MinecraftMeshStruct data;

        public bool IsRenderValid {get; private set;}

        public int BlendMode {
            get => data.BlendMode;
            set => SetAffectsRender(ref data.BlendMode, value);
        }

        public Vector3 TintColor {
            get => data.TintColor;
            set => SetAffectsRender(ref data.TintColor, value);
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
