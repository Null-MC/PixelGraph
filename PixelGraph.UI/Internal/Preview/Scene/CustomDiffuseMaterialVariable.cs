using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.ShaderManager;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    internal class CustomDiffuseMaterialVariable : MaterialVariable
    {
        private const int NUMTEXTURES = 2;
        private const int NUMSAMPLERS = 2;

        private const int
            DiffuseAlphaMapIdx = 0,
            EmissiveMapIdx = 1;

        private const int
            SurfaceSamplerIdx = 0,
            ShadowSamplerIdx = 1;

        private readonly CustomDiffuseMaterialCore material;

        private readonly ITextureResourceManager textureManager;
        private readonly IStatePoolManager statePoolManager;
        private readonly ShaderResourceViewProxy[] TextureResources;
        private readonly SamplerStateProxy[] SamplerResources;

        private int texDiffuseAlphaSlot, texEmissiveSlot, texShadowSlot;
        private int samplerSurfaceSlot, samplerShadowSlot;
        private uint textureIndex;

        public ShaderPass MaterialPass { private set; get; }
        public ShaderPass MaterialOITPass { private set; get; }
        public ShaderPass ShadowPass { get; }
        public ShaderPass WireframePass { get; } 
        public ShaderPass WireframeOITPass { get; }
        public ShaderPass DepthPass { get; }

        private bool HasTextures => textureIndex != 0;


        public CustomDiffuseMaterialVariable(IEffectsManager manager, IRenderTechnique technique, CustomDiffuseMaterialCore core,
            string materialPassName = CustomPassNames.Diffuse, string wireframePassName = DefaultPassNames.Wireframe,
            string materialOITPassName = CustomPassNames.DiffuseOIT, string wireframeOITPassName = DefaultPassNames.WireframeOITPass,
            string shadowPassName = DefaultPassNames.ShadowPass,
            string depthPassName = DefaultPassNames.DepthPrepass)
            : base(manager, technique, DefaultMeshConstantBufferDesc, core)
        {
            TextureResources = new ShaderResourceViewProxy[NUMTEXTURES];
            SamplerResources = new SamplerStateProxy[NUMSAMPLERS];

            textureManager = manager.MaterialTextureManager;
            statePoolManager = manager.StateManager;
            material = core;

            MaterialPass = technique[materialPassName];
            MaterialOITPass = technique[materialOITPassName];
            WireframePass = technique[wireframePassName];
            WireframeOITPass = technique[wireframeOITPassName];
            ShadowPass = technique[shadowPassName];
            DepthPass = technique[depthPassName];

            UpdateMappings(MaterialPass);
            CreateTextureViews();
            CreateSamplers();
        }

        protected override void OnInitialPropertyBindings()
        {
            AddPropertyBinding(nameof(CustomDiffuseMaterialCore.RenderShadowMap), () => {
                WriteValue(PhongPBRMaterialStruct.RenderShadowMapStr, material.RenderShadowMap ? 1 : 0);
            });

            AddPropertyBinding(nameof(CustomDiffuseMaterialCore.DiffuseAlphaMap), () => {
                CreateTextureView(material.DiffuseAlphaMap, DiffuseAlphaMapIdx);
            });

            AddPropertyBinding(nameof(CustomDiffuseMaterialCore.EmissiveMap), () => {
                CreateTextureView(material.EmissiveMap, EmissiveMapIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.SurfaceMapSampler), () => {
                CreateSampler(material.SurfaceMapSampler, SurfaceSamplerIdx);
            });
        }

        public override bool BindMaterialResources(RenderContext context, DeviceContextProxy deviceContext, ShaderPass shaderPass)
        {
            if (HasTextures) {
                OnBindMaterialTextures(context, deviceContext, shaderPass.PixelShader);
            }

            if (material.RenderShadowMap && context.IsShadowMapEnabled) {
                shaderPass.PixelShader.BindTexture(deviceContext, texShadowSlot, context.SharedResource.ShadowView);
                shaderPass.PixelShader.BindSampler(deviceContext, samplerShadowSlot, SamplerResources[ShadowSamplerIdx]);
            }

            return true;
        }

        public override ShaderPass GetPass(RenderType renderType, RenderContext context)
        {
            return renderType == RenderType.Transparent && context.IsOITPass
                ? MaterialOITPass : MaterialPass;
        }

        public override ShaderPass GetShadowPass(RenderType renderType, RenderContext context)
        {
            return ShadowPass;
        }

        public override ShaderPass GetWireframePass(RenderType renderType, RenderContext context)
        {
            return renderType == RenderType.Transparent && context.IsOITPass
                ? WireframeOITPass : WireframePass;
        }

        public override ShaderPass GetDepthPass(RenderType renderType, RenderContext context)
        {
            return DepthPass;
        }

        public override void Draw(DeviceContextProxy deviceContext, IAttachableBufferModel bufferModel, int instanceCount)
        {
            DrawIndexed(deviceContext, bufferModel.IndexBuffer.ElementCount, instanceCount);
        }

        private void CreateTextureViews()
        {
            if (material != null) {
                CreateTextureView(material.DiffuseAlphaMap, DiffuseAlphaMapIdx);
                CreateTextureView(material.EmissiveMap, EmissiveMapIdx);
            }
            else {
                for (var i = 0; i < NUMTEXTURES; ++i)
                    RemoveAndDispose(ref TextureResources[i]);

                textureIndex = 0;
            }
        }

        private void CreateSamplers()
        {
            var newSurfaceSampler = statePoolManager.Register(material.SurfaceMapSampler);
            var newShadowSampler = statePoolManager.Register(DefaultSamplers.ShadowSampler);
            RemoveAndDispose(ref SamplerResources[SurfaceSamplerIdx]);
            RemoveAndDispose(ref SamplerResources[ShadowSamplerIdx]);

            if (material != null) {
                SamplerResources[SurfaceSamplerIdx] = Collect(newSurfaceSampler);
                SamplerResources[ShadowSamplerIdx] = Collect(newShadowSampler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateTextureView(TextureModel texture, int index)
        {
            var newTexture = texture == null
                ? null : textureManager.Register(texture);

            RemoveAndDispose(ref TextureResources[index]);
            TextureResources[index] = Collect(newTexture);

            if (TextureResources[index] != null) {
                textureIndex |= 1u << index;
            }
            else {
                textureIndex &= ~(1u << index);
            }
        }

        private void CreateSampler(SamplerStateDescription desc, int index)
        {
            var newRes = statePoolManager.Register(desc);
            RemoveAndDispose(ref SamplerResources[index]);
            SamplerResources[index] = Collect(newRes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMappings(ShaderPass shaderPass)
        {
            texDiffuseAlphaSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot("tex_diffuse_alpha");
            texEmissiveSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot("tex_emissive");
            texShadowSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot("tex_shadow");

            samplerSurfaceSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot("sampler_surface");
            samplerShadowSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot("sampler_shadow");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnBindMaterialTextures(RenderContext context, DeviceContextProxy deviceContext, PixelShader shader)
        {
            if (shader.IsNULL) return;
            
            shader.BindTexture(deviceContext, texDiffuseAlphaSlot, TextureResources[DiffuseAlphaMapIdx]);
            shader.BindTexture(deviceContext, texEmissiveSlot, TextureResources[EmissiveMapIdx]);

            shader.BindSampler(deviceContext, samplerSurfaceSlot, SamplerResources[SurfaceSamplerIdx]);
        }
    }
}
