using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using MahApps.Metro.Controls;
using PixelGraph.Common.Textures;
using PixelGraph.Rendering.Materials;
using PixelGraph.UI.Internal.Preview.Textures;
using System;

namespace PixelGraph.UI.Helix.Materials
{
    internal class PbrMaterialBuilder : MaterialBuilderBase<IRenderPbrPreviewBuilder>
    {
        public PbrMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Color] = null;
            TextureMap[TextureTags.Normal] = null;
            TextureMap[TextureTags.Rough] = null;
            TextureMap[TextureTags.Porosity] = null;
        }

        public override Material BuildMaterial()
        {
            var mat = new CustomPbrMaterial(PassName, PassNameOIT) {
                EnvironmentCubeMapSource = EnvironmentCubeMapSource,
                IrradianceCubeMapSource = IrradianceCubeMapSource,
                RenderEnvironmentMap = RenderEnvironmentMap,
                BrdfLutMap = BrdfLutMap,
                SurfaceMapSampler = ColorSampler,
                HeightMapSampler = HeightSampler,
                RenderShadowMap = true,
            };

            if (Material.Color?.PreviewTint != null) {
                var tint = ColorHelper.ColorFromString(Material.Color.PreviewTint);
                if (tint.HasValue) mat.ColorTint = tint.Value.ToColor4();
            }

            if (TextureMap.TryGetValue(TextureTags.Color, out var albedoStream) && albedoStream != null) {
                mat.AlbedoAlphaMap = TextureModel.Create(albedoStream);
            }

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
