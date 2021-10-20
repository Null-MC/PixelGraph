using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Render;
using PixelGraph.Rendering.Sky;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PixelGraph.Rendering.CubeMaps
{
    internal class EnvironmentCubeCore : CubeMapRenderCore
    {
        private SkyDomeBufferModel geometryBuffer;
        private IMinecraftScene _scene;

        public IMinecraftScene Scene {
            get => _scene;
            set => SetAffectsRender(ref _scene, value);
        }


        public EnvironmentCubeCore() : base(RenderType.PreProc)
        {
            TextureDesc = new Texture2DDescription {
                Format = Format.R16G16B16A16_Float,
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
            if (_scene == null || _scene.IsRenderValid) {
                if (!context.UpdateSceneGraphRequested && !context.UpdatePerFrameRenderableRequested) return;
            }

            base.Render(context, deviceContext);
            
            deviceContext.GenerateMips(CubeMap);
            context.SharedResource.EnvironmentMapMipLevels = CubeMap.TextureView.Description.TextureCube.MipLevels;
            
            context.UpdatePerFrameData(true, false, deviceContext);
            _scene?.Apply(deviceContext);

            _scene?.ResetValidation();
        }

        protected override void RenderFace(RenderContext context, DeviceContextProxy deviceContext)
        {
            Scene.Apply(deviceContext);

            var vertexStartSlot = 0;
            geometryBuffer.AttachBuffers(deviceContext, ref vertexStartSlot, EffectTechnique.EffectsManager);

            deviceContext.DrawIndexed(geometryBuffer.IndexBuffer.ElementCount, 0, 0);
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            geometryBuffer = Collect(new SkyDomeBufferModel());

            return base.OnAttach(technique);
        }

        protected override void OnDetach()
        {
            geometryBuffer = null;

            base.OnDetach();
        }
    }
}
