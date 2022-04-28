using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;
using PixelGraph.Rendering.Sky;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.Rendering.CubeMaps
{
    internal class EquirectangularCubeMapCore : CubeMapRenderCore
    {
        private ShaderResourceViewProxy textureView;
        private SkyBoxBufferModel geometryBuffer;
        private SamplerStateProxy textureSampler;
        private TextureModel _texture;
        private int textureSamplerSlot;
        private int textureSlot;
        private float _intensity;
        //private bool isValid;

        public TextureModel Texture {
            get => _texture;
            set {
                if (SetAffectsRender(ref _texture, value) && IsAttached) {
                    UpdateTexture();
                    IsRenderValid = false;
                }
            }
        }

        public float Intensity {
            get => _intensity;
            set {
                if (SetAffectsRender(ref _intensity, value))
                    IsRenderValid = false;
            }
        }


        public EquirectangularCubeMapCore() : base(RenderType.PreProc)
        {
            PassName = CustomPassNames.Sky_ERP;

            TextureDesc = new Texture2DDescription {
                Format = Format.R16G16B16A16_Float,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                ArraySize = 6,
                MipLevels = 0,
            };
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            if (!base.OnAttach(technique)) return false;
            IsRenderValid = false;

            geometryBuffer = Collect(new SkyBoxBufferModel());
            //defaultShaderPass = technique[CustomPassNames.Sky_ERP];

            //OnDefaultPassChanged();
            UpdateTexture();

            textureSampler = technique.EffectsManager.StateManager.Register(DefaultSamplers.EnvironmentSampler);
            return true;
        }

        protected override void OnDetach()
        {
            RemoveAndDispose(ref textureSampler);
            RemoveAndDispose(ref textureView);
            geometryBuffer = null;
            //isValid = false;

            base.OnDetach();
        }

        protected override void OnDefaultPassChanged()
        {
            textureSlot = defaultShaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.EquirectangularTB);
            textureSamplerSlot = defaultShaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.EnvironmentCubeSampler);
            IsRenderValid = false;

            base.OnDefaultPassChanged();
        }

        protected override bool OnUpdateCanRenderFlag()
        {
            return base.OnUpdateCanRenderFlag() && geometryBuffer != null;
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            //if (_cubeMap != null && IsRenderValid) {
            //    //if (!context.UpdateSceneGraphRequested && !context.UpdatePerFrameRenderableRequested) return;
            //}

            if (IsRenderValid) return;

            if (CreateCubeMapResources()) {
                RaiseInvalidateRender();
                return;
            }

            base.Render(context, deviceContext);
            
            deviceContext.GenerateMips(_cubeMap);
            context.SharedResource.EnvironmentMapMipLevels = _cubeMap.TextureView.Description.TextureCube.MipLevels;
            
            //context.UpdatePerFrameData(true, false, deviceContext);
            //IsRenderValid = true;
        }

        protected override void RenderFace(RenderContext context, DeviceContextProxy deviceContext)
        {
            //defaultShaderPass.PixelShader.BindTexture(deviceContext, textureSlot, textureView);
            //defaultShaderPass.PixelShader.BindSampler(deviceContext, textureSamplerSlot, textureSampler);
            deviceContext.SetShaderResource(PixelShader.Type, textureSlot, textureView);
            deviceContext.SetSampler(PixelShader.Type, textureSamplerSlot, textureSampler);

            var vertexStartSlot = 0;
            geometryBuffer.AttachBuffers(deviceContext, ref vertexStartSlot, EffectTechnique.EffectsManager);
            deviceContext.Draw(geometryBuffer.VertexBuffer[0].ElementCount, 0);
        }

        private void UpdateTexture()
        {
            //MipMapLevels = 0;
            RemoveAndDispose(ref textureView);
            if (_texture == null) return;

            textureView = new ShaderResourceViewProxy(Device);
            textureView.CreateView(_texture);

            //if (cubeTextureRes.TextureView != null && cubeTextureRes.TextureView.Description.Dimension == ShaderResourceViewDimension.TextureCube) {
            //    MipMapLevels = cubeTextureRes.TextureView.Description.TextureCube.MipLevels;
            //}
        }
    }
}
