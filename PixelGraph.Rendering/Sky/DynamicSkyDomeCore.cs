using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.Shaders;

namespace PixelGraph.Rendering.Sky
{
    internal class DynamicSkyDomeCore : GeometryRenderCore
    {
        private ShaderPass defaultShaderPass;

        protected ShaderPass DefaultShaderPass {
            get => defaultShaderPass;
            private set => SetAffectsRender(ref defaultShaderPass, value);
        }


        public DynamicSkyDomeCore() : base(RenderType.Opaque)
        {
            RasterDescription = DefaultRasterDescriptions.RSSkyDome;

            defaultShaderPass = ShaderPass.NullPass;
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            if (!base.OnAttach(technique)) return false;

            DefaultShaderPass = technique[CustomPassNames.SkyFinal];
            GeometryBuffer = Collect(new SkyDomeBufferModel());
            return true;
        }

        protected override void OnRender(RenderContext context, DeviceContextProxy deviceContext)
        {
            DefaultShaderPass.BindShader(deviceContext);
            DefaultShaderPass.BindStates(deviceContext, StateType.BlendState | StateType.DepthStencilState);
            deviceContext.DrawIndexed(GeometryBuffer.IndexBuffer.ElementCount, 0, 0);
        }

        protected sealed override void OnRenderCustom(RenderContext context, DeviceContextProxy deviceContext) {}

        protected sealed override void OnRenderShadow(RenderContext context, DeviceContextProxy deviceContext) {}

        protected sealed override void OnRenderDepth(RenderContext context, DeviceContextProxy deviceContext, ShaderPass customPass) {}
    }
}
