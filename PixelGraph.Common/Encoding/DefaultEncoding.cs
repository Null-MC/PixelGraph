using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class DefaultEncoding : IResourcePackEncoding
    {
        public void Apply(ResourcePackEncoding encoding)
        {
            encoding.Alpha = new ResourcePackChannelProperties(EncodingChannel.Alpha) {
                Texture = TextureTags.Diffuse,
                Color = ColorChannel.Alpha,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.DiffuseRed = new ResourcePackChannelProperties(EncodingChannel.DiffuseRed) {
                Texture = TextureTags.Diffuse,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.DiffuseGreen = new ResourcePackChannelProperties(EncodingChannel.DiffuseGreen) {
                Texture = TextureTags.Diffuse,
                Color = ColorChannel.Green,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.DiffuseBlue = new ResourcePackChannelProperties(EncodingChannel.DiffuseBlue) {
                Texture = TextureTags.Diffuse,
                Color = ColorChannel.Blue,
                MinValue = 0,
                MaxValue = 255,
            };
        }
    }
}
