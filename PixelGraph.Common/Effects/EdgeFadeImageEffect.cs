using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;

namespace PixelGraph.Common.Effects
{
    public interface IEdgeFadeImageEffect
    {
        void Apply(Image image, string tag, Rectangle? bounds = null, bool isGrayscale = false);
        void Apply<TPixel>(Image<TPixel> image, string tag, Rectangle? bounds = null) where TPixel : unmanaged, IPixel<TPixel>;
    }

    internal class EdgeFadeImageEffect : IEdgeFadeImageEffect
    {
        private static readonly Type[] grayscalePixelTypes = {typeof(L8), typeof(L16), typeof(La16), typeof(La32)};

        private readonly ITextureGraphContext context;


        public EdgeFadeImageEffect(ITextureGraphContext context)
        {
            this.context = context;
        }

        public void Apply<TPixel>(Image<TPixel> image, string tag, Rectangle? bounds = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var isGrayscale = grayscalePixelTypes.Contains(typeof(TPixel));
            Apply(image, tag, bounds, isGrayscale);
        }

        public void Apply(Image image, string tag, Rectangle? bounds = null, bool isGrayscale = false)
        {
            var hasEdgeSizeX = context.Material.Height?.EdgeFadeX != null
                               || context.Material.Height?.EdgeFadeLeft != null
                               || context.Material.Height?.EdgeFadeRight != null;

            var hasEdgeSizeY = context.Material.Height?.EdgeFadeY != null
                               || context.Material.Height?.EdgeFadeTop != null
                               || context.Material.Height?.EdgeFadeBottom != null;

            if (!hasEdgeSizeX && !hasEdgeSizeY) return;

            var heightChannels = context.OutputEncoding
                .Where(c => TextureTags.Is(c.Texture, tag))
                .Where(c => EncodingChannel.Is(c.ID, EncodingChannel.Height))
                .Select(c => c.Color ?? ColorChannel.None).ToArray();

            if (!heightChannels.Any()) return;

            var processor = new HeightEdgeProcessor {
                //SizeX = (float?)context.Material.Height?.EdgeFadeX ?? 0f,
                //SizeY = (float?)context.Material.Height?.EdgeFadeY ?? 0f,
                SizeTop = (float?)context.Material.Height?.EdgeFadeTop ?? (float?)context.Material.Height?.EdgeFadeY ?? 0f,
                SizeBottom = (float?)context.Material.Height?.EdgeFadeBottom ?? (float?)context.Material.Height?.EdgeFadeY ?? 0f,
                SizeLeft = (float?)context.Material.Height?.EdgeFadeLeft ?? (float?)context.Material.Height?.EdgeFadeX ?? 0f,
                SizeRight = (float?)context.Material.Height?.EdgeFadeRight ?? (float?)context.Material.Height?.EdgeFadeX ?? 0f,
                Strength = (float?)context.Material.Height?.EdgeFadeStrength ?? 1f,
                Bounds = bounds ?? image.Bounds(),
                IsGrayscale = isGrayscale,
                Colors = heightChannels,
            };

            processor.Apply(image);
        }
    }
}
