using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Common.Textures;
using PixelGraph.UI.Internal.Preview.Textures;
using System;

namespace PixelGraph.UI.Helix.Materials
{
    internal class NormalsMaterialBuilder : MaterialBuilderBase<IRenderNormalsPreviewBuilder>
    {
        public NormalsMaterialBuilder(IServiceProvider provider) : base(provider)
        {
            TextureMap[TextureTags.Color] = null;
            TextureMap[TextureTags.Normal] = null;
        }

        public override Material BuildMaterial()
        {
            var mat = new CustomNormalsMaterial {
                SurfaceMapSampler = ColorSampler,
                HeightMapSampler = HeightSampler,
            };

            if (TextureMap.TryGetValue(TextureTags.Color, out var opacityStream) && opacityStream != null)
                mat.OpacityMap = TextureModel.Create(opacityStream);

            if (TextureMap.TryGetValue(TextureTags.Normal, out var normalStream) && normalStream != null)
                mat.NormalHeightMap = TextureModel.Create(normalStream);

            return mat;
        }
    }
}
