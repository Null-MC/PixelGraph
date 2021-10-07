using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Helix.CubeMaps;
using PixelGraph.UI.Helix.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace PixelGraph.UI.Helix.Materials
{
    public class CustomPbrMaterialCore : MaterialCore
    {
        private TextureModel _albedoAlphaMap;
        private TextureModel _normalHeightMap;
        private TextureModel _roughF0OcclusionMap;
        private TextureModel _porositySssEmissiveMap;
        private TextureModel _brdfLutMap;
        private ICubeMapSource _environmentCubeSource;
        private ICubeMapSource _irradianceCubeSource;
        private SamplerStateDescription _surfaceMapSampler;
        private SamplerStateDescription _heightMapSampler;
        private SamplerStateDescription _shadowMapSampler;
        private SamplerStateDescription _lightMapSampler;
        private SamplerStateDescription _environmentMapSampler;
        private SamplerStateDescription _irradianceMapSampler;
        private SamplerStateDescription _brdfLutMapSampler;
        private Color4 _colorTint;
        private bool _renderEnvironmentMap;
        private bool _renderShadowMap;

        public string MaterialPassName {get;}
        public string MaterialOITPassName {get;}

        public TextureModel AlbedoAlphaMap {
            get => _albedoAlphaMap;
            set => Set(ref _albedoAlphaMap, value);
        }

        public TextureModel NormalHeightMap {
            get => _normalHeightMap;
            set => Set(ref _normalHeightMap, value);
        }

        public TextureModel RoughF0OcclusionMap {
            get => _roughF0OcclusionMap;
            set => Set(ref _roughF0OcclusionMap, value);
        }

        public TextureModel PorositySssEmissiveMap {
            get => _porositySssEmissiveMap;
            set => Set(ref _porositySssEmissiveMap, value);
        }

        public TextureModel BrdfLutMap {
            get => _brdfLutMap;
            set => Set(ref _brdfLutMap, value);
        }

        public ICubeMapSource EnvironmentCubeMapSource {
            get => _environmentCubeSource;
            set => Set(ref _environmentCubeSource, value);
        }

        public ICubeMapSource IrradianceCubeMapSource {
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


        public CustomPbrMaterialCore(string materialPassName, string materialOITPassName)
        {
            MaterialPassName = materialPassName ?? throw new ArgumentNullException(nameof(materialPassName));
            MaterialOITPassName = materialOITPassName ?? throw new ArgumentNullException(nameof(materialOITPassName));

            _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni16;
            _heightMapSampler = DefaultSamplers.LinearSamplerWrapAni16;
            _shadowMapSampler = CustomSamplerStates.Shadow;
            _lightMapSampler = CustomSamplerStates.Light;
            _environmentMapSampler = CustomSamplerStates.Environment;
            _irradianceMapSampler = CustomSamplerStates.Irradiance;
            _brdfLutMapSampler = CustomSamplerStates.BrdfLut;
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new CustomPbrMaterialVariable(manager, technique, this, MaterialPassName, MaterialOITPassName);
        }
    }
}
