﻿using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using System.Linq;

namespace PixelGraph.Common.Effects
{
    public interface IEdgeFadeImageEffect
    {
        void Apply(Image image, string tag, Rectangle? bounds = null);
    }

    internal class EdgeFadeImageEffect : IEdgeFadeImageEffect
    {
        private readonly ITextureGraphContext context;


        public EdgeFadeImageEffect(ITextureGraphContext context)
        {
            this.context = context;
        }

        public void Apply(Image image, string tag, Rectangle? bounds = null)
        {
            var hasEdgeSizeX = context.Material.Height?.EdgeFadeX.HasValue ?? false;
            var hasEdgeSizeY = context.Material.Height?.EdgeFadeY.HasValue ?? false;
            if (!hasEdgeSizeX && !hasEdgeSizeY) return;

            var heightChannels = context.OutputEncoding
                .Where(c => TextureTags.Is(c.Texture, tag))
                .Where(c => EncodingChannel.Is(c.ID, EncodingChannel.Height))
                .Select(c => c.Color ?? ColorChannel.None).ToArray();

            if (!heightChannels.Any()) return;

            var processor = new HeightEdgeProcessor {
                SizeX = (float?)context.Material.Height?.EdgeFadeX ?? 0f,
                SizeY = (float?)context.Material.Height?.EdgeFadeY ?? 0f,
                Strength = (float?)context.Material.Height?.EdgeFadeStrength ?? 1f,
                Bounds = bounds ?? image.Bounds(),
                Colors = heightChannels,
            };

            processor.Apply(image);
        }
    }
}
