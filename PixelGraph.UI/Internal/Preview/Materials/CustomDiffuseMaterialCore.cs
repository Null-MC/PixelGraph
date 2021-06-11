using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using SharpDX.Direct3D11;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    public class CustomDiffuseMaterialCore : MaterialCore
    {
        private TextureModel _diffuseAlphaMap;
        private TextureModel _emissiveMap;
        private SamplerStateDescription _surfaceMapSampler;
        private bool _renderShadowMap;

        public TextureModel DiffuseAlphaMap {
            get => _diffuseAlphaMap;
            set => Set(ref _diffuseAlphaMap, value);
        }

        public TextureModel EmissiveMap {
            get => _emissiveMap;
            set => Set(ref _emissiveMap, value);
        }

        public bool RenderShadowMap {
            get => _renderShadowMap; 
            set => Set(ref _renderShadowMap, value);
        }

        public SamplerStateDescription SurfaceMapSampler {
            get => _surfaceMapSampler; 
            set => Set(ref _surfaceMapSampler, value); 
        }


        public CustomDiffuseMaterialCore()
        {
            _surfaceMapSampler = DefaultSamplers.LinearSamplerWrapAni4;
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new CustomDiffuseMaterialVariable(manager, technique, this);
        }
    }
}
