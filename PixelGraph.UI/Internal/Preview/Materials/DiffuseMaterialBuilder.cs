using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Shaders;
using PixelGraph.UI.Internal.Preview.Textures;
using System;

namespace PixelGraph.UI.Internal.Preview.Materials
{
    internal class DiffuseMaterialBuilder : MaterialBuilderBase<IRenderDiffusePreviewBuilder>
    {
        public DiffuseMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Diffuse] = null;
            TextureMap[TextureTags.Emissive] = null;
        }

        public override Material BuildMaterial(string passName = null)
        {
            var mat = new CustomDiffuseMaterial {
                SurfaceMapSampler = CustomSamplerStates.Color,
                RenderShadowMap = true,
            };

            if (TextureMap.TryGetValue(TextureTags.Diffuse, out var diffuseStream) && diffuseStream != null)
                mat.DiffuseAlphaMap = TextureModel.Create(diffuseStream);

            if (TextureMap.TryGetValue(TextureTags.Emissive, out var emissiveStream) && emissiveStream != null)
                mat.EmissiveMap = TextureModel.Create(emissiveStream);

            return mat;
        }
    }
}
