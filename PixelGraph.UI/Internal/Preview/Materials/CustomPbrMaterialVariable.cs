using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.ShaderManager;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.UI.Internal.Preview.Shaders;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal class CustomPbrMaterialVariable : MaterialVariable
    {
        private const int NUMTEXTURES = 4;
        private const int NUMSAMPLERS = 4;

        private const int
            AlbedoAlphaMapIdx = 0,
            NormalHeightMapIdx = 1,
            RoughF0OcclusionMapIdx = 2,
            PorositySssEmissiveMapIdx = 3;

        private const int
            SurfaceSamplerIdx = 0,
            HeightSamplerIdx = 1,
            CubeSamplerIdx = 2,
            ShadowSamplerIdx = 3;

        private readonly CustomPbrMaterialCore material;

        private readonly ITextureResourceManager textureManager;
        private readonly IStatePoolManager statePoolManager;
        private readonly ShaderResourceViewProxy[] textureResources;
        private readonly SamplerStateProxy[] samplerResources;

        private int texAlbedoAlphaSlot, texNormalHeightSlot, texRoughF0OcclusionSlot, texPorositySssEmissiveSlot, texShadowSlot, texCubeSlot;
        private int samplerSurfaceSlot, samplerHeightSlot, samplerCubeSlot, samplerShadowSlot;
        private uint textureIndex;

        public ShaderPass MaterialPass { get; }
        public ShaderPass MaterialOITPass { get; }
        public ShaderPass ShadowPass { get; }
        public ShaderPass WireframePass { get; } 
        public ShaderPass WireframeOITPass { get; }
        public ShaderPass DepthPass { get; }

        private bool HasTextures => textureIndex != 0;


        public CustomPbrMaterialVariable(IEffectsManager manager, IRenderTechnique technique, CustomPbrMaterialCore core, string materialPassName, string materialOITPassName)
                : base(manager, technique, DefaultMeshConstantBufferDesc, core)
        {
            textureResources = new ShaderResourceViewProxy[NUMTEXTURES];
            samplerResources = new SamplerStateProxy[NUMSAMPLERS];

            textureManager = manager.MaterialTextureManager;
            statePoolManager = manager.StateManager;
            material = core;

            MaterialPass = technique[materialPassName];
            MaterialOITPass = technique[materialOITPassName];
            WireframePass = technique[DefaultPassNames.Wireframe];
            WireframeOITPass = technique[DefaultPassNames.WireframeOITPass];
            ShadowPass = technique[DefaultPassNames.ShadowPass];
            DepthPass = technique[DefaultPassNames.DepthPrepass];

            UpdateMappings(MaterialPass);
            CreateTextureViews();
            CreateSamplers();
        }

        protected override void OnInitialPropertyBindings()
        {
            AddPropertyBinding(nameof(CustomPbrMaterialCore.AlbedoAlphaMap), () => {
                CreateTextureView(material.AlbedoAlphaMap, AlbedoAlphaMapIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.NormalHeightMap), () => {
                CreateTextureView(material.NormalHeightMap, NormalHeightMapIdx);
            });
            
            AddPropertyBinding(nameof(CustomPbrMaterialCore.RoughF0OcclusionMap), () => {
                CreateTextureView(material.RoughF0OcclusionMap, RoughF0OcclusionMapIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.PorositySssEmissiveMap), () => {
                CreateTextureView(material.PorositySssEmissiveMap, PorositySssEmissiveMapIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.SurfaceMapSampler), () => {
                CreateSampler(material.SurfaceMapSampler, SurfaceSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.HeightMapSampler), () => {
                CreateSampler(material.HeightMapSampler, HeightSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.CubeMapSampler), () => {
                CreateSampler(material.CubeMapSampler, CubeSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.RenderShadowMap), () => {
                WriteValue(PhongPBRMaterialStruct.RenderShadowMapStr, material.RenderShadowMap ? 1 : 0);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.RenderEnvironmentMap), () => {
                WriteValue(PhongPBRMaterialStruct.HasCubeMapStr, material.RenderEnvironmentMap ? 1 : 0);
            });

            WriteValue(PhongPBRMaterialStruct.RenderPBR, true);
        }

        public override bool BindMaterialResources(RenderContext context, DeviceContextProxy deviceContext, ShaderPass shaderPass)
        {
            if (HasTextures) {
                OnBindMaterialTextures(context, deviceContext, shaderPass.PixelShader);
            }

            if (material.RenderShadowMap && context.IsShadowMapEnabled) {
                shaderPass.PixelShader.BindTexture(deviceContext, texShadowSlot, context.SharedResource.ShadowView);
                shaderPass.PixelShader.BindSampler(deviceContext, samplerShadowSlot, samplerResources[ShadowSamplerIdx]);
            }

            if (material.RenderEnvironmentMap && material.EnvironmentCube != null) {
                shaderPass.PixelShader.BindTexture(deviceContext, texCubeSlot, material.EnvironmentCube.CubeMap);
                shaderPass.PixelShader.BindSampler(deviceContext, samplerCubeSlot, samplerResources[CubeSamplerIdx]);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateTextureView(TextureModel texture, int index)
        {
            var newTexture = texture == null
                ? null : textureManager.Register(texture);

            RemoveAndDispose(ref textureResources[index]);
            textureResources[index] = Collect(newTexture);

            if (textureResources[index] != null) {
                textureIndex |= 1u << index;
            }
            else {
                textureIndex &= ~(1u << index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMappings(ShaderPass shaderPass)
        {
            texAlbedoAlphaSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.AlbedoAlphaTB);
            texNormalHeightSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.NormalHeightTB);
            texRoughF0OcclusionSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.RoughF0OcclusionTB);
            texPorositySssEmissiveSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.PorositySssEmissiveTB);
            texShadowSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.ShadowMapTB);
            texCubeSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.CubeMapTB);

            samplerSurfaceSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.SurfaceSampler);
            samplerHeightSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.HeightSampler);
            samplerShadowSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.ShadowMapSampler);
            samplerCubeSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.CubeMapSampler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnBindMaterialTextures(RenderContext context, DeviceContextProxy deviceContext, PixelShader shader)
        {
            if (shader.IsNULL) return;
            
            shader.BindTexture(deviceContext, texAlbedoAlphaSlot, textureResources[AlbedoAlphaMapIdx]);
            shader.BindTexture(deviceContext, texNormalHeightSlot, textureResources[NormalHeightMapIdx]);
            shader.BindTexture(deviceContext, texRoughF0OcclusionSlot, textureResources[RoughF0OcclusionMapIdx]);
            shader.BindTexture(deviceContext, texPorositySssEmissiveSlot, textureResources[PorositySssEmissiveMapIdx]);

            shader.BindSampler(deviceContext, samplerSurfaceSlot, samplerResources[SurfaceSamplerIdx]);
            shader.BindSampler(deviceContext, samplerHeightSlot, samplerResources[HeightSamplerIdx]);
            shader.BindSampler(deviceContext, samplerCubeSlot, samplerResources[CubeSamplerIdx]);
        }

        private void CreateTextureViews()
        {
            if (material != null) {
                CreateTextureView(material.AlbedoAlphaMap, AlbedoAlphaMapIdx);
                CreateTextureView(material.NormalHeightMap, NormalHeightMapIdx);
                CreateTextureView(material.RoughF0OcclusionMap, RoughF0OcclusionMapIdx);
                CreateTextureView(material.PorositySssEmissiveMap, PorositySssEmissiveMapIdx);
            }
            else {
                for (var i = 0; i < NUMTEXTURES; ++i)
                    RemoveAndDispose(ref textureResources[i]);

                textureIndex = 0;
            }
        }

        private void CreateSamplers()
        {
            var newSurfaceSampler = statePoolManager.Register(material.SurfaceMapSampler);
            var newHeightSampler = statePoolManager.Register(material.HeightMapSampler);
            var newShadowSampler = statePoolManager.Register(DefaultSamplers.ShadowSampler);
            var newCubeSampler = statePoolManager.Register(material.CubeMapSampler);

            RemoveAndDispose(ref samplerResources[SurfaceSamplerIdx]);
            RemoveAndDispose(ref samplerResources[HeightSamplerIdx]);
            RemoveAndDispose(ref samplerResources[ShadowSamplerIdx]);
            RemoveAndDispose(ref samplerResources[CubeSamplerIdx]);

            if (material != null) {
                samplerResources[SurfaceSamplerIdx] = Collect(newSurfaceSampler);
                samplerResources[HeightSamplerIdx] = Collect(newHeightSampler);
                samplerResources[ShadowSamplerIdx] = Collect(newShadowSampler);
                samplerResources[CubeSamplerIdx] = Collect(newCubeSampler);
            }
        }

        private void CreateSampler(SamplerStateDescription desc, int index)
        {
            var newRes = statePoolManager.Register(desc);
            RemoveAndDispose(ref samplerResources[index]);
            samplerResources[index] = Collect(newRes);
        }
    }
}
