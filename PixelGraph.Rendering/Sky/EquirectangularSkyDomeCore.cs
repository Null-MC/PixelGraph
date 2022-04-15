using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;

namespace PixelGraph.Rendering.Sky
{
    internal class EquirectangularSkyDomeCore : GeometryRenderCore
    {
        private ShaderResourceViewProxy textureView;
        private SamplerStateProxy textureSampler;
        private ShaderPass defaultShaderPass;
        private TextureModel _texture;
        private int textureSamplerSlot;
        private int textureSlot;

        public TextureModel Texture {
            get => _texture;
            set {
                if (SetAffectsRender(ref _texture, value) && IsAttached) {
                    UpdateTexture();
                }
            }
        }


        public EquirectangularSkyDomeCore() : base(RenderType.Opaque)
        {
            RasterDescription = DefaultRasterDescriptions.RSSkyDome;
            defaultShaderPass = ShaderPass.NullPass;
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            if (!base.OnAttach(technique)) return false;

            GeometryBuffer = Collect(new SkyDomeBufferModel());
            defaultShaderPass = technique[CustomPassNames.SkyFinal_ERP];

            OnDefaultPassChanged();
            UpdateTexture();

            textureSampler = technique.EffectsManager.StateManager.Register(DefaultSamplers.EnvironmentSampler);
            return true;
        }

        protected override void OnDetach()
        {
            RemoveAndDispose(ref textureSampler);
            RemoveAndDispose(ref textureView);
            GeometryBuffer = null;
            base.OnDetach();
        }

        protected void OnDefaultPassChanged()
        {
            textureSlot = defaultShaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.EquirectangularTB);
            textureSamplerSlot = defaultShaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.EnvironmentCubeSampler);
        }

        protected override void OnRender(RenderContext context, DeviceContextProxy deviceContext)
        {
            if (_texture == null) return;

            defaultShaderPass.BindShader(deviceContext);
            defaultShaderPass.BindStates(deviceContext, StateType.BlendState | StateType.DepthStencilState);
            defaultShaderPass.PixelShader.BindTexture(deviceContext, textureSlot, textureView);
            defaultShaderPass.PixelShader.BindSampler(deviceContext, textureSamplerSlot, textureSampler);
            deviceContext.DrawIndexed(GeometryBuffer.IndexBuffer.ElementCount, 0, 0);
        }

        protected sealed override void OnRenderCustom(RenderContext context, DeviceContextProxy deviceContext) {}

        protected sealed override void OnRenderShadow(RenderContext context, DeviceContextProxy deviceContext) {}

        protected sealed override void OnRenderDepth(RenderContext context, DeviceContextProxy deviceContext, ShaderPass customPass) {}

        private void UpdateTexture()
        {
            RemoveAndDispose(ref textureView);
            if (_texture == null) {
                
                return;
            }

            textureView = new ShaderResourceViewProxy(Device);
            textureView.CreateView(_texture);
        }
    }
}
