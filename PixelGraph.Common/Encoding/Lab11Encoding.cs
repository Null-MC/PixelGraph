using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class Lab11Encoding : IResourcePackEncoding
    {
        public void Apply(ResourcePackEncoding encoding)
        {
            encoding.Alpha = new ResourcePackChannelProperties(EncodingChannel.Alpha) {
                Texture = TextureTags.Albedo,
                Color = ColorChannel.Alpha,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.AlbedoRed = new ResourcePackChannelProperties(EncodingChannel.AlbedoRed) {
                Texture = TextureTags.Albedo,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.AlbedoGreen = new ResourcePackChannelProperties(EncodingChannel.AlbedoGreen) {
                Texture = TextureTags.Albedo,
                Color = ColorChannel.Green,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.AlbedoBlue = new ResourcePackChannelProperties(EncodingChannel.AlbedoBlue) {
                Texture = TextureTags.Albedo,
                Color = ColorChannel.Blue,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.NormalX = new ResourcePackChannelProperties(EncodingChannel.NormalX) {
                Texture = TextureTags.Normal,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.NormalY = new ResourcePackChannelProperties(EncodingChannel.NormalY) {
                Texture = TextureTags.Normal,
                Color = ColorChannel.Green,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.NormalZ = new ResourcePackChannelProperties(EncodingChannel.NormalZ) {
                Texture = TextureTags.Normal,
                Color = ColorChannel.Blue,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Height = new ResourcePackChannelProperties(EncodingChannel.Height) {
                Texture = TextureTags.Normal,
                Color = ColorChannel.Alpha,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Smooth = new ResourcePackChannelProperties(EncodingChannel.Smooth) {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
                Power = 0.5f,
            };

            encoding.Metal = new ResourcePackChannelProperties(EncodingChannel.Metal) {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Green,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Emissive = new ResourcePackChannelProperties(EncodingChannel.Emissive) {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Blue,
                MinValue = 0,
                MaxValue = 255,
            };
        }
    }
}
