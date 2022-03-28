using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Textures;
using PixelGraph.UI.Internal.Utilities;
using System;

namespace PixelGraph.UI.Helix.Materials
{
    internal class LabPbrMaterialBuilder : MaterialBuilderBase<IRenderLabPbrPreviewBuilder>
    {
        public LabPbrMaterialBuilder(IServiceProvider provider) : base(provider)
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

            if (Material.TintColor != null) {
                //if (!tint_hex.StartsWith('#')) tint_hex = '#'+tint_hex;
                var tint = ColorHelper.RGBFromHex(Material.TintColor);
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
