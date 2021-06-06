using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Textures;
using SharpDX;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal class DiffuseMaterialBuilder : MaterialBuilderBase<IRenderDiffusePreviewBuilder>
    {
        public DiffuseMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Diffuse] = null;
        }

        public override Material BuildMaterial()
        {
            var mat = new PhongMaterial {
                DiffuseMapSampler = DefaultSampler,
                RenderDiffuseMap = false,
                RenderDiffuseAlphaMap = true,
                RenderNormalMap = false,
                RenderSpecularColorMap = false,
                RenderEmissiveMap = false,
                RenderEnvironmentMap = false,
                RenderShadowMap = true,

                EnableTessellation = false,
                RenderDisplacementMap = false,

                AmbientColor = Color.White,
                SpecularColor = Color.Black,
            };

            if (TextureMap.TryGetValue(TextureTags.Diffuse, out var diffuseStream) && diffuseStream != null)
                mat.DiffuseAlphaMap = TextureModel.Create(diffuseStream);

            return mat;
        }
    }
}
