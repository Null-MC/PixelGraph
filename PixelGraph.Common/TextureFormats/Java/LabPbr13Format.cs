﻿using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java;

public class LabPbr13Format : ITextureFormatFactory
{
    public const string Description = "The latest LabPbr standard.";


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

            Occlusion = new ResourcePackOcclusionChannelProperties {
                Texture = TextureTags.Normal,
                Color = ColorChannel.Blue,
                MinValue = 0m,
                MaxValue = 1m,
                DefaultValue = 0,
                Invert = true,
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

            F0 = new ResourcePackF0ChannelProperties {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Green,
                MinValue = 0m,
                MaxValue = 0.9m,
                DefaultValue = 0.04m,
                RangeMin = 0,
                RangeMax = 229,
                ClipValue = 0m,
                EnableClipping = true,
            },

            HCM = new ResourcePackHcmChannelProperties {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Green,
                Sampler = Samplers.Samplers.Nearest,
                MinValue = 230m,
                MaxValue = 255m,
                RangeMin = 230,
                RangeMax = 255,
                //ClipValue = 229m,
                EnableClipping = true,
                Priority = 1,
            },

            Porosity = new ResourcePackPorosityChannelProperties {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Blue,
                RangeMin = 0,
                RangeMax = 64,
                EnableClipping = true,
            },

            SSS = new ResourcePackSssChannelProperties {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Blue,
                RangeMin = 65,
                RangeMax = 255,
                //EnableClipping = true,
                ClipValue = 0m,
            },

            Emissive = new ResourcePackEmissiveChannelProperties {
                Texture = TextureTags.Specular,
                Color = ColorChannel.Alpha,
                Shift = -1,
                DefaultValue = 0m,
            },
        };
    }
}