using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.LUTs;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using SharpDX.Direct3D11;

namespace PixelGraph.Rendering.Materials;

public class CustomPbrMaterialCore(string materialPassName) : MaterialCore
{
    private TextureModel? _albedoAlphaMap;
    private TextureModel? _normalHeightMap;
    private TextureModel? _roughF0OcclusionMap;
    private TextureModel? _porositySssEmissiveMap;
    private ILutMapSource? _dielectricBdrfLutSource;
    private ICubeMapSource? _environmentCubeSource;
    private ICubeMapSource? _irradianceCubeSource;
    private SamplerStateDescription _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni16;
    private SamplerStateDescription _heightMapSampler = DefaultSamplers.LinearSamplerWrapAni16;
    private SamplerStateDescription _shadowMapSampler = CustomSamplerStates.Shadow;
    private SamplerStateDescription _lightMapSampler = CustomSamplerStates.Light;
    private SamplerStateDescription _environmentMapSampler = CustomSamplerStates.Environment;
    private SamplerStateDescription _irradianceMapSampler = CustomSamplerStates.Irradiance;
    private SamplerStateDescription _brdfLutMapSampler = CustomSamplerStates.BrdfLut;
    private Color4 _colorTint;
    private bool _renderEnvironmentMap;
    private bool _renderShadowMap;

    public string MaterialPassName { get; } = materialPassName ?? throw new ArgumentNullException(nameof(materialPassName));
    //public string MaterialOITPassName {get;}

    public TextureModel? AlbedoAlphaMap {
        get => _albedoAlphaMap;
        set => Set(ref _albedoAlphaMap, value);
    }

    public TextureModel? NormalHeightMap {
        get => _normalHeightMap;
        set => Set(ref _normalHeightMap, value);
    }

    public TextureModel? RoughF0OcclusionMap {
        get => _roughF0OcclusionMap;
        set => Set(ref _roughF0OcclusionMap, value);
    }

    public TextureModel? PorositySssEmissiveMap {
        get => _porositySssEmissiveMap;
        set => Set(ref _porositySssEmissiveMap, value);
    }

    public ILutMapSource? DielectricBdrfLutSource {
        get => _dielectricBdrfLutSource;
        set => Set(ref _dielectricBdrfLutSource, value);
    }

    public ICubeMapSource? EnvironmentCubeMapSource {
        get => _environmentCubeSource;
        set => Set(ref _environmentCubeSource, value);
    }

    public ICubeMapSource? IrradianceCubeMapSource {
        get => _irradianceCubeSource;
        set => Set(ref _irradianceCubeSource, value);
    }

    public SamplerStateDescription SurfaceMapSampler {
        get => _surfaceMapSampler;
        set => Set(ref _surfaceMapSampler, value); 
    }

    public SamplerStateDescription HeightMapSampler {
        get => _heightMapSampler;
        set => Set(ref _heightMapSampler, value); 
    }

    public SamplerStateDescription ShadowMapSampler {
        get => _shadowMapSampler;
        set => Set(ref _shadowMapSampler, value);
    }

    public SamplerStateDescription LightMapSampler {
        get => _lightMapSampler;
        set => Set(ref _lightMapSampler, value);
    }

    public SamplerStateDescription EnvironmentMapSampler {
        get => _environmentMapSampler;
        set => Set(ref _environmentMapSampler, value);
    }

    public SamplerStateDescription IrradianceMapSampler {
        get => _irradianceMapSampler;
        set => Set(ref _irradianceMapSampler, value);
    }

    public SamplerStateDescription BrdfLutMapSampler {
        get => _brdfLutMapSampler;
        set => Set(ref _brdfLutMapSampler, value);
    }

    public Color4 ColorTint {
        get => _colorTint;
        set => Set(ref _colorTint, value);
    }

    public bool RenderShadowMap {
        get => _renderShadowMap; 
        set => Set(ref _renderShadowMap, value);
    }

    public bool RenderEnvironmentMap {
        get => _renderEnvironmentMap;
        set => Set(ref _renderEnvironmentMap, value);
    }

    public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
    {
        return new CustomPbrMaterialVariable(manager, technique, this, MaterialPassName);
    }
}
