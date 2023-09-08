using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats;

public class RawFormat : ITextureFormatFactory
{
    public const string Description = "Separates all encoding channels as separate textures.";


    public PackEncoding Create()
    {
        return new() {
            Opacity = new ResourcePackOpacityChannelProperties {
                Texture = TextureTags.Opacity,
                Color = ColorChannel.Red,
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

            Height = new ResourcePackHeightChannelProperties {
                Texture = TextureTags.Height,
                Color = ColorChannel.Red,
                Invert = true,
            },

            Occlusion = new ResourcePackOcclusionChannelProperties {
                Texture = TextureTags.Occlusion,
                Color = ColorChannel.Red,
                Invert = true,
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

            Smooth = new ResourcePackSmoothChannelProperties {
                Texture = TextureTags.Smooth,
                Color = ColorChannel.Red,
            },

            Rough = new ResourcePackRoughChannelProperties {
                Texture = TextureTags.Rough,
                Color = ColorChannel.Red,
            },

            Metal = new ResourcePackMetalChannelProperties {
                Texture = TextureTags.Metal,
                Color = ColorChannel.Red,
            },

            HCM = new ResourcePackHcmChannelProperties {
                Texture = TextureTags.HCM,
                Color = ColorChannel.Red,
                Sampler = Samplers.Samplers.Nearest,
                MinValue = 230m,
                MaxValue = 255m,
                RangeMin = 230,
                RangeMax = 255,
                EnableClipping = true,
            },

            F0 = new ResourcePackF0ChannelProperties {
                Texture = TextureTags.F0,
                Color = ColorChannel.Red,
                //EnableClipping = true,
            },

            Porosity = new ResourcePackPorosityChannelProperties {
                Texture = TextureTags.Porosity,
                Color = ColorChannel.Red,
            },

            SSS = new ResourcePackSssChannelProperties {
                Texture = TextureTags.SubSurfaceScattering,
                Color = ColorChannel.Red,
            },

            Emissive = new ResourcePackEmissiveChannelProperties {
                Texture = TextureTags.Emissive,
                Color = ColorChannel.Red,
            },
        };
    }
}