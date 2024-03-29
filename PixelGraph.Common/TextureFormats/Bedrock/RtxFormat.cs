﻿using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Bedrock;

public class RtxFormat : ITextureFormatFactory
{
    public const string Description = "The NVidia standard for Bedrock RTX PBR.";


    public PackEncoding Create()
    {
        return new() {
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

            Opacity = new ResourcePackOpacityChannelProperties {
                Texture = TextureTags.Color,
                Color = ColorChannel.Alpha,
                //Power = 2.2m,
                DefaultValue = 1m,
            },

            Height = new ResourcePackHeightChannelProperties {
                Texture = TextureTags.Height,
                Color = ColorChannel.Red,
                Invert = true,
                DefaultValue = 0,
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

            Metal = new ResourcePackMetalChannelProperties {
                Texture = TextureTags.MER,
                Color = ColorChannel.Red,
            },

            Emissive = new ResourcePackEmissiveChannelProperties {
                Texture = TextureTags.MER,
                Color = ColorChannel.Green,
            },

            Rough = new ResourcePackRoughChannelProperties {
                Texture = TextureTags.MER,
                Color = ColorChannel.Blue,
            },
        };
    }
}