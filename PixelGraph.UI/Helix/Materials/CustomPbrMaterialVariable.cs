using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.ShaderManager;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.UI.Helix.Shaders;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.UI.Helix.Materials
{
    internal class CustomPbrMaterialVariable : MaterialVariable
    {
        private const int NUMTEXTURES = 5;
        private const int NUMSAMPLERS = 7;

        private const int
            AlbedoAlphaMapIdx = 0,
            NormalHeightMapIdx = 1,
            RoughF0OcclusionMapIdx = 2,
            PorositySssEmissiveMapIdx = 3,
            BrdfLutMapIdx = 4;

        private const int
            SurfaceSamplerIdx = 0,
            HeightSamplerIdx = 1,
            EnvironmentSamplerIdx = 2,
            IrradianceSamplerIdx = 3,
            BrdfLutSamplerIdx = 4,
            ShadowSamplerIdx = 5,
            LightSamplerIdx = 6;

        private readonly CustomPbrMaterialCore material;

        private readonly ITextureResourceManager textureManager;
        private readonly IStatePoolManager statePoolManager;
        private readonly ShaderResourceViewProxy[] textureResources;
        private readonly SamplerStateProxy[] samplerResources;

        private int texShadowSlot, texEnvironmentSlot, texIrradianceSlot, texBrdfLutSlot;
        private int texAlbedoAlphaSlot, texNormalHeightSlot, texRoughF0OcclusionSlot, texPorositySssEmissiveSlot;
        private int samplerSurfaceSlot, samplerHeightSlot, samplerEnvironmentSlot, samplerIrradianceSlot, samplerBrdfLutSlot, samplerShadowSlot, samplerLightSlot;
        private uint textureIndex;

        private int texShadowAlbedoAlphaSlot, texShadowNormalHeightSlot;
        private int samplerShadowSurfaceSlot, samplerShadowHeightSlot;

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

            UpdateMappings();

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

            AddPropertyBinding(nameof(CustomPbrMaterialCore.BrdfLutMap), () => {
                CreateTextureView(material.BrdfLutMap, BrdfLutMapIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.SurfaceMapSampler), () => {
                CreateSampler(material.SurfaceMapSampler, SurfaceSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.HeightMapSampler), () => {
                CreateSampler(material.HeightMapSampler, HeightSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.EnvironmentMapSampler), () => {
                CreateSampler(material.EnvironmentMapSampler, EnvironmentSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.IrradianceMapSampler), () => {
                CreateSampler(material.IrradianceMapSampler, IrradianceSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.BrdfLutMapSampler), () => {
                CreateSampler(material.BrdfLutMapSampler, BrdfLutSamplerIdx);
            });

            AddPropertyBinding(nameof(CustomPbrMaterialCore.ColorTint), () => {
                WriteValue(PhongPBRMaterialStruct.DiffuseStr, material.ColorTint);
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

            if (material.RenderEnvironmentMap) {
                if (material.EnvironmentCubeMapSource != null) {
                    shaderPass.PixelShader.BindTexture(deviceContext, texEnvironmentSlot, material.EnvironmentCubeMapSource.CubeMap);
                    shaderPass.PixelShader.BindSampler(deviceContext, samplerEnvironmentSlot, samplerResources[EnvironmentSamplerIdx]);
                }

                if (material.IrradianceCubeMapSource != null) {
                    shaderPass.PixelShader.BindTexture(deviceContext, texIrradianceSlot, material.IrradianceCubeMapSource.CubeMap);
                    shaderPass.PixelShader.BindSampler(deviceContext, samplerIrradianceSlot, samplerResources[IrradianceSamplerIdx]);
                }
            }

            if (material.RenderShadowMap && context.IsShadowMapEnabled) {
                shaderPass.PixelShader.BindTexture(deviceContext, texShadowSlot, context.SharedResource.ShadowView);
                shaderPass.PixelShader.BindSampler(deviceContext, samplerShadowSlot, samplerResources[ShadowSamplerIdx]);
                shaderPass.PixelShader.BindSampler(deviceContext, samplerLightSlot, samplerResources[LightSamplerIdx]);
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
        private void UpdateMappings()
        {
            var shadowPassTexMap = ShadowPass.PixelShader.ShaderResourceViewMapping;
            texShadowAlbedoAlphaSlot = shadowPassTexMap.TryGetBindSlot(CustomBufferNames.AlbedoAlphaTB);
            texShadowNormalHeightSlot = shadowPassTexMap.TryGetBindSlot(CustomBufferNames.NormalHeightTB);

            var shadowPassSamplerMap = ShadowPass.PixelShader.ShaderResourceViewMapping;
            samplerShadowSurfaceSlot = shadowPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.SurfaceSampler);
            samplerShadowHeightSlot = shadowPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.HeightSampler);

            var pbrPassTexMap = MaterialPass.PixelShader.ShaderResourceViewMapping;
            texAlbedoAlphaSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.AlbedoAlphaTB);
            texNormalHeightSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.NormalHeightTB);
            texRoughF0OcclusionSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.RoughF0OcclusionTB);
            texPorositySssEmissiveSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.PorositySssEmissiveTB);
            texBrdfLutSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.BrdfLutTB);
            texShadowSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.ShadowMapTB);
            texEnvironmentSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.EnvironmentCubeTB);
            texIrradianceSlot = pbrPassTexMap.TryGetBindSlot(CustomBufferNames.IrradianceCubeTB);

            var pbrPassSamplerMap = MaterialPass.PixelShader.SamplerMapping;
            samplerSurfaceSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.SurfaceSampler);
            samplerHeightSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.HeightSampler);
            samplerShadowSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.ShadowMapSampler);
            samplerLightSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.LightMapSampler);
            samplerEnvironmentSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.EnvironmentCubeSampler);
            samplerIrradianceSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.IrradianceCubeSampler);
            samplerBrdfLutSlot = pbrPassSamplerMap.TryGetBindSlot(CustomSamplerStateNames.BrdfLutSampler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnBindMaterialTextures(RenderContext context, DeviceContextProxy deviceContext, PixelShader shader)
        {
            if (shader.IsNULL) return;

            if (shader.Name == CustomShaderManager.Name_ShadowPixel) {
                shader.BindTexture(deviceContext, texShadowAlbedoAlphaSlot, textureResources[AlbedoAlphaMapIdx]);
                shader.BindTexture(deviceContext, texShadowNormalHeightSlot, textureResources[NormalHeightMapIdx]);

                shader.BindSampler(deviceContext, samplerShadowSurfaceSlot, samplerResources[SurfaceSamplerIdx]);
                shader.BindSampler(deviceContext, samplerShadowHeightSlot, samplerResources[HeightSamplerIdx]);
            }
            else {
                shader.BindTexture(deviceContext, texAlbedoAlphaSlot, textureResources[AlbedoAlphaMapIdx]);
                shader.BindTexture(deviceContext, texNormalHeightSlot, textureResources[NormalHeightMapIdx]);
                shader.BindTexture(deviceContext, texRoughF0OcclusionSlot, textureResources[RoughF0OcclusionMapIdx]);
                shader.BindTexture(deviceContext, texPorositySssEmissiveSlot, textureResources[PorositySssEmissiveMapIdx]);
                shader.BindTexture(deviceContext, texBrdfLutSlot, textureResources[BrdfLutMapIdx]);

                shader.BindSampler(deviceContext, samplerSurfaceSlot, samplerResources[SurfaceSamplerIdx]);
                shader.BindSampler(deviceContext, samplerHeightSlot, samplerResources[HeightSamplerIdx]);
                shader.BindSampler(deviceContext, samplerEnvironmentSlot, samplerResources[EnvironmentSamplerIdx]);
                shader.BindSampler(deviceContext, samplerIrradianceSlot, samplerResources[IrradianceSamplerIdx]);
                shader.BindSampler(deviceContext, samplerBrdfLutSlot, samplerResources[BrdfLutSamplerIdx]);
            }
        }

        private void CreateTextureViews()
        {
            if (material != null) {
                CreateTextureView(material.AlbedoAlphaMap, AlbedoAlphaMapIdx);
                CreateTextureView(material.NormalHeightMap, NormalHeightMapIdx);
                CreateTextureView(material.RoughF0OcclusionMap, RoughF0OcclusionMapIdx);
                CreateTextureView(material.PorositySssEmissiveMap, PorositySssEmissiveMapIdx);
                CreateTextureView(material.BrdfLutMap, BrdfLutMapIdx);
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
            var newShadowSampler = statePoolManager.Register(material.ShadowMapSampler);
            var newLightSampler = statePoolManager.Register(material.LightMapSampler);
            var newSkySampler = statePoolManager.Register(material.EnvironmentMapSampler);
            var newIrradianceSampler = statePoolManager.Register(material.IrradianceMapSampler);
            var newBrdfLutSampler = statePoolManager.Register(material.BrdfLutMapSampler);

            RemoveAndDispose(ref samplerResources[SurfaceSamplerIdx]);
            RemoveAndDispose(ref samplerResources[HeightSamplerIdx]);
            RemoveAndDispose(ref samplerResources[ShadowSamplerIdx]);
            RemoveAndDispose(ref samplerResources[LightSamplerIdx]);
            RemoveAndDispose(ref samplerResources[EnvironmentSamplerIdx]);
            RemoveAndDispose(ref samplerResources[IrradianceSamplerIdx]);
            RemoveAndDispose(ref samplerResources[BrdfLutSamplerIdx]);

            if (material != null) {
                samplerResources[SurfaceSamplerIdx] = Collect(newSurfaceSampler);
                samplerResources[HeightSamplerIdx] = Collect(newHeightSampler);
                samplerResources[ShadowSamplerIdx] = Collect(newShadowSampler);
                samplerResources[LightSamplerIdx] = Collect(newLightSampler);
                samplerResources[EnvironmentSamplerIdx] = Collect(newSkySampler);
                samplerResources[IrradianceSamplerIdx] = Collect(newIrradianceSampler);
                samplerResources[BrdfLutSamplerIdx] = Collect(newBrdfLutSampler);
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
