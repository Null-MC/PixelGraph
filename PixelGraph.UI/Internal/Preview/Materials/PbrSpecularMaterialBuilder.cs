using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Scene;
using PixelGraph.UI.Internal.Preview.Textures;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal class PbrSpecularMaterialBuilder : MaterialBuilderBase<IRenderPbrSpecularPreviewBuilder>
    {
        public PbrSpecularMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Albedo] = null;
            TextureMap[TextureTags.Normal] = null;
            TextureMap[TextureTags.Smooth] = null;
            TextureMap[TextureTags.Porosity] = null;
        }

        public override Material BuildMaterial()
        {
            var mat = new PbrSpecularMaterial {
                SurfaceMapSampler = CustomSamplerStates.Default,
                RenderEnvironmentMap = Model.Preview.EnableEnvironment,
                RenderShadowMap = true,
            };

            if (TextureMap.TryGetValue(TextureTags.Albedo, out var albedoStream) && albedoStream != null)
                mat.AlbedoAlphaMap = TextureModel.Create(albedoStream);

            if (TextureMap.TryGetValue(TextureTags.Normal, out var normalStream) && normalStream != null)
                mat.NormalHeightMap = TextureModel.Create(normalStream);

            if (TextureMap.TryGetValue(TextureTags.Smooth, out var smoothStream) && smoothStream != null)
                mat.SmoothF0OcclusionMap = TextureModel.Create(smoothStream);

            if (TextureMap.TryGetValue(TextureTags.Porosity, out var porosityStream) && porosityStream != null)
                mat.PorositySssEmissiveMap = TextureModel.Create(porosityStream);

            return mat;
        }
    }
}
