using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using MahApps.Metro.Controls;
using PixelGraph.Common.Textures;
using PixelGraph.Rendering.Materials;
using PixelGraph.UI.Internal.Preview.Textures;
using System;

namespace PixelGraph.UI.Helix.Materials
{
    internal class DiffuseMaterialBuilder : MaterialBuilderBase<IRenderDiffusePreviewBuilder>
    {
        public DiffuseMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Color] = null;
            TextureMap[TextureTags.Emissive] = null;
        }

        public override Material BuildMaterial()
        {
            var mat = new CustomDiffuseMaterial {
                IrradianceCubeMapSource = EnvironmentCubeMapSource,
                RenderEnvironmentMap = RenderEnvironmentMap,
                SurfaceMapSampler = ColorSampler,
                RenderShadowMap = false,
            };

            if (Material.Color?.PreviewTint != null) {
                var tint = ColorHelper.ColorFromString(Material.Color.PreviewTint);
                if (tint.HasValue) mat.ColorTint = tint.Value.ToColor4();
            }

            if (TextureMap.TryGetValue(TextureTags.Color, out var diffuseStream) && diffuseStream != null)
                mat.DiffuseAlphaMap = TextureModel.Create(diffuseStream);

            if (TextureMap.TryGetValue(TextureTags.Emissive, out var emissiveStream) && emissiveStream != null)
                mat.EmissiveMap = TextureModel.Create(emissiveStream);

            return mat;
        }
    }
}
