using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.ShaderManager;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using PixelShader = HelixToolkit.SharpDX.Core.Shaders.PixelShader;

namespace PixelGraph.Rendering.Materials;

internal class CustomDiffuseMaterialVariable : MaterialVariable
{
    private const int NUMTEXTURES = 2;
    private const int NUMSAMPLERS = 3;

    private const int
        DiffuseAlphaMapIdx = 0,
        EmissiveMapIdx = 1;

    private const int
        SurfaceSamplerIdx = 0,
        IrradianceSamplerIdx = 1,
        ShadowSamplerIdx = 2;

    private readonly CustomDiffuseMaterialCore material;

    private readonly ITextureResourceManager textureManager;
    private readonly IStatePoolManager statePoolManager;
    private readonly ShaderResourceViewProxy[] textureResources;
    private readonly SamplerStateProxy[] samplerResources;

    private int texDiffuseAlphaSlot, texEmissiveSlot, texIrradianceSlot, texShadowSlot;
    private int samplerSurfaceSlot, samplerIrradianceSlot, samplerShadowSlot;
    private uint textureIndex;

    private readonly ShaderPass materialPass;
    private readonly ShaderPass shadowPass;
    private readonly ShaderPass wireframePass;
    private readonly ShaderPass depthPass;

    private bool HasTextures => textureIndex != 0;


    public CustomDiffuseMaterialVariable(IEffectsManager manager, IRenderTechnique technique, CustomDiffuseMaterialCore core)
        : base(manager, technique, DefaultMeshConstantBufferDesc, core)
    {
        textureResources = new ShaderResourceViewProxy[NUMTEXTURES];
        samplerResources = new SamplerStateProxy[NUMSAMPLERS];

        textureManager = manager.MaterialTextureManager;
        statePoolManager = manager.StateManager;
        material = core;

        materialPass = technique[CustomPassNames.Diffuse];
        wireframePass = technique[DefaultPassNames.Wireframe];
        shadowPass = technique[DefaultPassNames.ShadowPass];
        depthPass = technique[DefaultPassNames.DepthPrepass];

        UpdateMappings(materialPass);
        CreateTextureViews();
        CreateSamplers();
    }

    protected override void OnDispose(bool disposeManagedResources)
    {
        for (var i = 0; i < NUMSAMPLERS; ++i)
            RemoveAndDispose(ref samplerResources[i]);

        for (var i = 0; i < NUMTEXTURES; ++i)
            RemoveAndDispose(ref textureResources[i]);


        base.OnDispose(disposeManagedResources);
    }

    protected override void OnInitialPropertyBindings()
    {
        AddPropertyBinding(nameof(CustomDiffuseMaterialCore.DiffuseAlphaMap), () => {
            CreateTextureView(material.DiffuseAlphaMap, DiffuseAlphaMapIdx);
        });

        AddPropertyBinding(nameof(CustomDiffuseMaterialCore.EmissiveMap), () => {
            CreateTextureView(material.EmissiveMap, EmissiveMapIdx);
        });

        AddPropertyBinding(nameof(CustomPbrMaterialCore.SurfaceMapSampler), () => {
            CreateSampler(material.SurfaceMapSampler, SurfaceSamplerIdx);
        });

        AddPropertyBinding(nameof(CustomPbrMaterialCore.IrradianceMapSampler), () => {
            CreateSampler(material.IrradianceMapSampler, IrradianceSamplerIdx);
        });

        AddPropertyBinding(nameof(CustomDiffuseMaterialCore.ColorTint), () => {
            WriteValue(PhongPBRMaterialStruct.DiffuseStr, material.ColorTint);
        });

        AddPropertyBinding(nameof(CustomDiffuseMaterialCore.RenderShadowMap), () => {
            WriteValue(PhongPBRMaterialStruct.RenderShadowMapStr, material.RenderShadowMap ? 1 : 0);
        });

        AddPropertyBinding(nameof(CustomPbrMaterialCore.RenderEnvironmentMap), () => {
            WriteValue(PhongPBRMaterialStruct.HasCubeMapStr, material.RenderEnvironmentMap ? 1 : 0);
        });
    }

    public override bool BindMaterialResources(RenderContext context, DeviceContextProxy deviceContext, ShaderPass shaderPass)
    {
        if (HasTextures) {
            OnBindMaterialTextures(deviceContext, shaderPass.PixelShader);
        }

        if (material.RenderShadowMap && context.IsShadowMapEnabled) {
            shaderPass.PixelShader.BindTexture(deviceContext, texShadowSlot, context.SharedResource.ShadowView);
            shaderPass.PixelShader.BindSampler(deviceContext, samplerShadowSlot, samplerResources[ShadowSamplerIdx]);
        }

        if (material.RenderEnvironmentMap && material.IrradianceCubeMapSource != null) {
            shaderPass.PixelShader.BindTexture(deviceContext, texIrradianceSlot, material.IrradianceCubeMapSource.CubeMap);
            shaderPass.PixelShader.BindSampler(deviceContext, samplerIrradianceSlot, samplerResources[IrradianceSamplerIdx]);
        }

        return true;
    }

    public override ShaderPass GetPass(RenderType renderType, RenderContext context) => materialPass;

    public override ShaderPass GetShadowPass(RenderType renderType, RenderContext context) => shadowPass;

    public override ShaderPass GetWireframePass(RenderType renderType, RenderContext context) => wireframePass;

    public override ShaderPass GetDepthPass(RenderType renderType, RenderContext context) => depthPass;

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
        textureResources[index] = newTexture;

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
        texDiffuseAlphaSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.DiffuseAlphaTB);
        texEmissiveSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.EmissiveTB);
        texShadowSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.ShadowMapTB);
        texIrradianceSlot = shaderPass.PixelShader.ShaderResourceViewMapping.TryGetBindSlot(CustomBufferNames.IrradianceCubeTB);

        samplerSurfaceSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.SurfaceSampler);
        samplerShadowSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.ShadowMapSampler);
        samplerIrradianceSlot = shaderPass.PixelShader.SamplerMapping.TryGetBindSlot(CustomSamplerStateNames.IrradianceCubeSampler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnBindMaterialTextures(DeviceContextProxy deviceContext, PixelShader shader)
    {
        if (shader.IsNULL) return;
            
        shader.BindTexture(deviceContext, texDiffuseAlphaSlot, textureResources[DiffuseAlphaMapIdx]);
        shader.BindTexture(deviceContext, texEmissiveSlot, textureResources[EmissiveMapIdx]);

        shader.BindSampler(deviceContext, samplerSurfaceSlot, samplerResources[SurfaceSamplerIdx]);
        shader.BindSampler(deviceContext, samplerIrradianceSlot, samplerResources[IrradianceSamplerIdx]);
    }

    private void CreateTextureViews()
    {
        if (material != null) {
            CreateTextureView(material.DiffuseAlphaMap, DiffuseAlphaMapIdx);
            CreateTextureView(material.EmissiveMap, EmissiveMapIdx);
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
        var newShadowSampler = statePoolManager.Register(DefaultSamplers.ShadowSampler);
        var newIrradianceSampler = statePoolManager.Register(material.IrradianceMapSampler);

        RemoveAndDispose(ref samplerResources[SurfaceSamplerIdx]);
        RemoveAndDispose(ref samplerResources[ShadowSamplerIdx]);
        RemoveAndDispose(ref samplerResources[IrradianceSamplerIdx]);

        if (material != null) {
            samplerResources[SurfaceSamplerIdx] = newSurfaceSampler;
            samplerResources[ShadowSamplerIdx] = newShadowSampler;
            samplerResources[IrradianceSamplerIdx] = newIrradianceSampler;
        }
    }

    private void CreateSampler(SamplerStateDescription desc, int index)
    {
        var newRes = statePoolManager.Register(desc);
        RemoveAndDispose(ref samplerResources[index]);
        samplerResources[index] = newRes;
    }
}