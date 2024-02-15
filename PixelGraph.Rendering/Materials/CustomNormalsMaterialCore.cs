using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using SharpDX.Direct3D11;

namespace PixelGraph.Rendering.Materials;

public class CustomNormalsMaterialCore : MaterialCore
{
    private TextureModel? _opacityMap;
    private TextureModel? _normalHeightMap;
    private SamplerStateDescription _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni16;
    private SamplerStateDescription _heightMapSampler = DefaultSamplers.LinearSamplerWrapAni16;

    public TextureModel? OpacityMap {
        get => _opacityMap;
        set => Set(ref _opacityMap, value);
    }

    public TextureModel? NormalHeightMap {
        get => _normalHeightMap;
        set => Set(ref _normalHeightMap, value);
    }

    public SamplerStateDescription SurfaceMapSampler {
        get => _surfaceMapSampler; 
        set => Set(ref _surfaceMapSampler, value); 
    }

    public SamplerStateDescription HeightMapSampler {
        get => _heightMapSampler;
        set => Set(ref _heightMapSampler, value); 
    }


    public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
    {
        return new CustomNormalsMaterialVariable(manager, technique, this);
    }
}