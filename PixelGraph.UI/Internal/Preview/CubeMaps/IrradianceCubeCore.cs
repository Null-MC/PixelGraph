using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Preview.Sky;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.UI.Internal.Preview.CubeMaps
{
    internal class IrradianceCubeCore : CubeMapRenderCore
    {
        private SkyBoxBufferModel geometryBuffer;
        private ICubeMapSource _source;
        private SamplerStateDescription _samplerDescription;
        private SamplerStateProxy textureSampler;
        private long sourceLastUpdated;
        private int cubeTextureSlot;
        private int textureSamplerSlot;

        public ICubeMapSource Source {
            get => _source;
            set => SetAffectsRender(ref _source, value);
        }

        public SamplerStateDescription SamplerDescription {
            get => _samplerDescription;
            set {
                if (!SetAffectsRender(ref _samplerDescription, value) || !IsAttached) return;

                var newSampler = EffectTechnique.EffectsManager.StateManager.Register(value);
                RemoveAndDispose(ref textureSampler);
                textureSampler = Collect(newSampler);
            }
        }


        public IrradianceCubeCore() : base(RenderType.PreProc)
        {
            PassName = CustomPassNames.SkyIrradiance;
            _samplerDescription = DefaultSamplers.IBLSampler;

            TextureDesc = new Texture2DDescription {
                Format = Format.R8G8B8A8_UNorm,
                ArraySize = 6,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 0,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
            };
        }

        protected override bool OnUpdateCanRenderFlag()
        {
            return base.OnUpdateCanRenderFlag() && geometryBuffer != null;
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            if (_source == null || sourceLastUpdated == _source.LastUpdated) {
                if (!context.UpdateSceneGraphRequested && !context.UpdatePerFrameRenderableRequested) return;
            }

            base.Render(context, deviceContext);

            sourceLastUpdated = _source.LastUpdated;

            //deviceContext.GenerateMips(cubeMap);
            //context.SharedResource.EnvironmentMapMipLevels = cubeMap.TextureView.Description.TextureCube.MipLevels;

            //context.UpdatePerFrameData(true, false, deviceContext);
            //_scene?.Apply(deviceContext);

            //_scene?.ResetValidation();
        }

        protected override void RenderFace(RenderContext context, DeviceContextProxy deviceContext)
        {
            deviceContext.SetShaderResource(PixelShader.Type, cubeTextureSlot, _source.CubeMap);
            deviceContext.SetSampler(PixelShader.Type, textureSamplerSlot, textureSampler);

            defaultShaderPass.PixelShader.BindSampler(deviceContext, textureSamplerSlot, textureSampler);

            var vertexStartSlot = 0;
            geometryBuffer.AttachBuffers(deviceContext, ref vertexStartSlot, EffectTechnique.EffectsManager);

            deviceContext.Draw(geometryBuffer.VertexBuffer[0].ElementCount, 0);
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            geometryBuffer = Collect(new SkyBoxBufferModel());
            textureSampler = Collect(technique.EffectsManager.StateManager.Register(SamplerDescription));

            return base.OnAttach(technique);
        }

        protected override void OnDetach()
        {
            textureSampler = null;
            geometryBuffer = null;

            base.OnDetach();
        }

        protected override void OnDefaultPassChanged(ShaderPass pass)
        {
            cubeTextureSlot = pass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.CubeMapTB);
            textureSamplerSlot = pass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.CubeMapSampler);
        }
    }
}
