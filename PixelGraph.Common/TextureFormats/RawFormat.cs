using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats
{
    public class RawFormat : ITextureFormatFactory
    {
        public const string Description = "Separates all encoding channels as separate textures.";


        public ResourcePackEncoding Create()
        {
            return new() {
                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Opacity,
                    Color = ColorChannel.Red,
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

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = true,
                    Perceptual = false,
                },

                Occlusion = new ResourcePackOcclusionChannelProperties {
                    Texture = TextureTags.Occlusion,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = true,
                    Perceptual = false,
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

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                },

                Specular = new ResourcePackSpecularChannelProperties {
                    Texture = TextureTags.Specular,
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

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Smooth,
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

                Rough = new ResourcePackRoughChannelProperties {
                    Texture = TextureTags.Rough,
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

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Metal,
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

                HCM = new ResourcePackHcmChannelProperties {
                    Texture = TextureTags.HCM,
                    Color = ColorChannel.Red,
                    Sampler = Samplers.Samplers.Nearest,
                    MinValue = 230m,
                    MaxValue = 255m,
                    RangeMin = 230,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    EnableClipping = true,
                },

                F0 = new ResourcePackF0ChannelProperties {
                    Texture = TextureTags.F0,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    Perceptual = false,
                    EnableClipping = true,
                },

                Porosity = new ResourcePackPorosityChannelProperties {
                    Texture = TextureTags.Porosity,
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

                SSS = new ResourcePackSssChannelProperties {
                    Texture = TextureTags.SubSurfaceScattering,
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

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Emissive,
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
            };
        }
    }
}
