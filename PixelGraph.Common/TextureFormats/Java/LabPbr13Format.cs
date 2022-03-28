using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class LabPbr13Format : ITextureFormatFactory
    {
        public const string Description = "The latest LabPbr standard.";


        public ResourcePackEncoding Create()
        {
            return new() {
                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    DefaultValue = 255,
                },

                ColorRed = new ResourcePackColorRedChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = true,
                },

                ColorGreen = new ResourcePackColorGreenChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Green,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = true,
                },

                ColorBlue = new ResourcePackColorBlueChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = true,
                },

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                },

                Occlusion = new ResourcePackOcclusionChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = true,
                    Perceptual = false,
                    DefaultValue = 0,
                },

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Alpha,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = true,
                    Perceptual = false,
                    DefaultValue = 0,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = true,
                },

                F0 = new ResourcePackF0ChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Green,
                    MinValue = 0m,
                    MaxValue = 0.9m,
                    RangeMin = 0,
                    RangeMax = 229,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    DefaultValue = 0.04m,
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
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    //ClipValue = 229m,
                    Priority = 1,
                    EnableClipping = true,
                },

                Porosity = new ResourcePackPorosityChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 64,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    EnableClipping = true,
                },

                SSS = new ResourcePackSssChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 65,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    ClipValue = 0m,
                    EnableClipping = true,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Alpha,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = -1,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    DefaultValue = 0m,
                },
            };
        }
    }
}
