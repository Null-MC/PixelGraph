using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class RawEncoding : IResourcePackEncoding
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

            encoding.Height = new ResourcePackChannelProperties(EncodingChannel.Height) {
                Texture = TextureTags.Height,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Occlusion = new ResourcePackChannelProperties(EncodingChannel.Occlusion) {
                Texture = TextureTags.Occlusion,
                Color = ColorChannel.Red,
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

            encoding.Specular = new ResourcePackChannelProperties(EncodingChannel.Specular) {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Smooth = new ResourcePackChannelProperties(EncodingChannel.Smooth) {
                Texture = TextureTags.Smooth,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Rough = new ResourcePackChannelProperties(EncodingChannel.Rough) {
                Texture = TextureTags.Rough,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Metal = new ResourcePackChannelProperties(EncodingChannel.Metal) {
                Texture = TextureTags.Metal,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Porosity = new ResourcePackChannelProperties(EncodingChannel.Porosity) {
                Texture = TextureTags.Porosity,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.SSS = new ResourcePackChannelProperties(EncodingChannel.SubSurfaceScattering) {
                Texture = TextureTags.SubSurfaceScattering,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };

            encoding.Emissive = new ResourcePackChannelProperties(EncodingChannel.Emissive) {
                Texture = TextureTags.Emissive,
                Color = ColorChannel.Red,
                MinValue = 0,
                MaxValue = 255,
            };
        }
    }
}
