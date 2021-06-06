using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using SharpDX.Direct3D11;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    public class PbrSpecularMaterialCore : MaterialCore
    {
        private TextureModel _albedoAlphaMap;
        private TextureModel _normalHeightMap;
        private TextureModel _roughF0OcclusionMap;
        private TextureModel _porositySssEmissive;
        private SamplerStateDescription _surfaceMapSampler;
        private bool _renderShadowMap;
        private bool _renderEnvironmentMap;

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
            get => _porositySssEmissive;
            set => Set(ref _porositySssEmissive, value);
        }

        public bool RenderShadowMap {
            get => _renderShadowMap; 
            set => Set(ref _renderShadowMap, value);
        }

        public bool RenderEnvironmentMap {
            get => _renderEnvironmentMap;
            set => Set(ref _renderEnvironmentMap, value);
        }

        public SamplerStateDescription SurfaceMapSampler {
            get => _surfaceMapSampler; 
            set => Set(ref _surfaceMapSampler, value); 
        }


        public PbrSpecularMaterialCore()
        {
            _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni4;
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new PbrSpecularMaterialVariable(manager, technique, this);
        }
    }
}
