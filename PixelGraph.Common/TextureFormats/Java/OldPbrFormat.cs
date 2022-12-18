using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class OldPbrFormat : ITextureFormatFactory
    {
        public const string Description = "The pre-Lab standard for PBR, also known as \"Old PBR\".";


        public PackEncoding Create()
        {
            return new() {
                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    //Power = 2.2m,
                    DefaultValue = 1m,
                },

                ColorRed = new ResourcePackColorRedChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Red,
                    //Power = 2.2m,
                },

                ColorGreen = new ResourcePackColorGreenChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Green,
                    //Power = 2.2m,
                },

                ColorBlue = new ResourcePackColorBlueChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Blue,
                    //Power = 2.2m,
                },

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = -1m,
                    MaxValue = 1m,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = -1m,
                    MaxValue = 1m,
                },

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                },

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Alpha,
                    DefaultValue = 0,
                    MinValue = 0m,
                    MaxValue = 1m,
                    Invert = true,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Green,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Blue,
                },
            };
        }
    }
}
