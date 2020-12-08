using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class Lab11Encoding : ITextureEncodingFactory
    {
        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                },

                AlbedoRed = new ResourcePackAlbedoRedChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                AlbedoGreen = new ResourcePackAlbedoGreenChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                AlbedoBlue = new ResourcePackAlbedoBlueChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                },

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                    Invert = true,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    Power = 0.5m,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                },
            };
        }
    }
}
