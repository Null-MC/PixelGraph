using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class LegacyEncoding : ITextureEncodingFactory
    {
        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                },

                DiffuseRed = new ResourcePackDiffuseRedChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                DiffuseGreen = new ResourcePackDiffuseGreenChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                DiffuseBlue = new ResourcePackDiffuseBlueChannelProperties {
                    Texture = TextureTags.Diffuse,
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

                Specular = new ResourcePackSpecularChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },
            };
        }
    }
}
