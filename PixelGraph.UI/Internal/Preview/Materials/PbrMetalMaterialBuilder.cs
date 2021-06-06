using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Textures;
using SharpDX;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal class PbrMetalMaterialBuilder : MaterialBuilderBase<IRenderPbrMetalPreviewBuilder>
    {
        public PbrMetalMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Albedo] = null;
            TextureMap[TextureTags.Height] = null;
            TextureMap[TextureTags.Normal] = null;
            TextureMap[TextureTags.Rough] = null;
            TextureMap[TextureTags.Emissive] = null;
        }

        public override Material BuildMaterial()
        {
            var mat = new PBRMaterial {
                SurfaceMapSampler = DefaultSampler,
                RenderEnvironmentMap = Model.Preview.EnableEnvironment,
                RenderShadowMap = true,
                EnableAutoTangent = true,

                EnableTessellation = false,
                RenderDisplacementMap = false,
                //MinTessellationDistance = 1,
                //MinDistanceTessellationFactor = 128,
                //MaxTessellationDistance = 40,
                //MaxDistanceTessellationFactor = 2,
                //DisplacementMapScaleMask = new Vector4(-0.3f, -0.3f, -0.3f, 0f),
                
                MetallicFactor = 1.0,
                RoughnessFactor = 1.0,
                ReflectanceFactor = 0.8,
                //AmbientOcclusionFactor = 0.8,
                EmissiveColor = new Color4(1f, 1f, 1f, 0f),
            };

            if (TextureMap.TryGetValue(TextureTags.Albedo, out var albedoStream) && albedoStream != null)
                mat.AlbedoMap = TextureModel.Create(albedoStream);

            if (TextureMap.TryGetValue(TextureTags.Height, out var heightStream) && heightStream != null)
                mat.DisplacementMap = TextureModel.Create(heightStream);

            if (TextureMap.TryGetValue(TextureTags.Normal, out var normalStream) && normalStream != null)
                mat.NormalMap = TextureModel.Create(normalStream);

            if (TextureMap.TryGetValue(TextureTags.Rough, out var roughStream) && roughStream != null)
                mat.AmbientOcculsionMap = mat.RoughnessMetallicMap = TextureModel.Create(roughStream);

            if (TextureMap.TryGetValue(TextureTags.Emissive, out var emissiveStream) && emissiveStream != null)
                mat.EmissiveMap = TextureModel.Create(emissiveStream);

            return mat;
        }
    }
}
