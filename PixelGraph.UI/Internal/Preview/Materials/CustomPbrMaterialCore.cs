using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.UI.Internal.Preview.CubeMaps;
using SharpDX.Direct3D11;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    public class CustomPbrMaterialCore : MaterialCore
    {
        private ICubeMapSource _environmentCube;
        private TextureModel _albedoAlphaMap;
        private TextureModel _normalHeightMap;
        private TextureModel _roughF0OcclusionMap;
        private TextureModel _porositySssEmissive;
        private SamplerStateDescription _surfaceMapSampler;
        private SamplerStateDescription _heightMapSampler;
        private SamplerStateDescription _cubeMapSampler;
        private bool _renderEnvironmentMap;
        private bool _renderShadowMap;

        public string MaterialPassName {get;}
        public string MaterialOITPassName {get;}

        public ICubeMapSource EnvironmentCube {
            get => _environmentCube;
            set => Set(ref _environmentCube, value);
        }

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

        public SamplerStateDescription SurfaceMapSampler {
            get => _surfaceMapSampler;
            set => Set(ref _surfaceMapSampler, value); 
        }

        public SamplerStateDescription HeightMapSampler {
            get => _heightMapSampler;
            set => Set(ref _heightMapSampler, value); 
        }

        public SamplerStateDescription CubeMapSampler {
            get => _cubeMapSampler; 
            set => Set(ref _cubeMapSampler, value); 
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
            _cubeMapSampler = DefaultSamplers.IBLSampler;
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new CustomPbrMaterialVariable(manager, technique, this, MaterialPassName, MaterialOITPassName);
        }
    }
}
