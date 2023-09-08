using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java;

public class SpecularFormat : ITextureFormatFactory
{
    public const string Description = "Uses a diffuse color map, normal XYZ, and legacy grayscale specular channels.";


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

            Specular = new ResourcePackSpecularChannelProperties {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Red,
            },
        };
    }
}