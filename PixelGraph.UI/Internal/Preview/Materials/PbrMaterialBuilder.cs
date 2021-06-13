using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Preview.Textures;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal class PbrMaterialBuilder : MaterialBuilderBase<IRenderPbrPreviewBuilder>
    {
        public PbrMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Albedo] = null;
            TextureMap[TextureTags.Normal] = null;
            TextureMap[TextureTags.Rough] = null;
            TextureMap[TextureTags.Porosity] = null;
        }

        public override Material BuildMaterial(string passName = null)
        {
            var mat = new CustomPbrMaterial(passName) {
                //SurfaceMapSampler = CustomSamplerStates.Default,
                RenderEnvironmentMap = Model.Preview.EnableEnvironment,
                EnvironmentCube = Model.Preview.EnvironmentCube,
                RenderShadowMap = false,
            };

            if (TextureMap.TryGetValue(TextureTags.Albedo, out var albedoStream) && albedoStream != null)
                mat.AlbedoAlphaMap = TextureModel.Create(albedoStream);

            if (TextureMap.TryGetValue(TextureTags.Normal, out var normalStream) && normalStream != null)
                mat.NormalHeightMap = TextureModel.Create(normalStream);

            if (TextureMap.TryGetValue(TextureTags.Rough, out var roughStream) && roughStream != null)
                mat.RoughF0OcclusionMap = TextureModel.Create(roughStream);

            if (TextureMap.TryGetValue(TextureTags.Porosity, out var porosityStream) && porosityStream != null)
                mat.PorositySssEmissiveMap = TextureModel.Create(porosityStream);

            return mat;
        }
    }
}
