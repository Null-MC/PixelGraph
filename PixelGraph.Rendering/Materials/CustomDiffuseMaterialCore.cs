using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using SharpDX.Direct3D11;

namespace PixelGraph.Rendering.Materials
{
    public class CustomDiffuseMaterialCore : MaterialCore
    {
        private TextureModel _diffuseAlphaMap;
        private TextureModel _emissiveMap;
        private ICubeMapSource _irradianceCubeMapSource;
        private SamplerStateDescription _surfaceMapSampler;
        private SamplerStateDescription _shadowMapSampler;
        private SamplerStateDescription _irradianceMapSampler;
        private Color4 _colorTint;
        private bool _renderShadowMap;
        private bool _renderEnvironmentMap;

        public TextureModel DiffuseAlphaMap {
            get => _diffuseAlphaMap;
            set => Set(ref _diffuseAlphaMap, value);
        }

        public TextureModel EmissiveMap {
            get => _emissiveMap;
            set => Set(ref _emissiveMap, value);
        }

        public ICubeMapSource IrradianceCubeMapSource {
            get => _irradianceCubeMapSource;
            set => Set(ref _irradianceCubeMapSource, value);
        }

        public SamplerStateDescription SurfaceMapSampler {
            get => _surfaceMapSampler; 
            set => Set(ref _surfaceMapSampler, value); 
        }

        public SamplerStateDescription ShadowMapSampler {
            get => _shadowMapSampler;
            set => Set(ref _shadowMapSampler, value);
        }

        public SamplerStateDescription IrradianceMapSampler {
            get => _irradianceMapSampler; 
            set => Set(ref _irradianceMapSampler, value); 
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


        public CustomDiffuseMaterialCore()
        {
            _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni16;
            _shadowMapSampler = CustomSamplerStates.Shadow;
            _irradianceMapSampler = DefaultSamplers.IBLSampler;
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new CustomDiffuseMaterialVariable(manager, technique, this);
        }
    }
}
