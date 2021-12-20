using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats
{
    public class RawFormat : ITextureFormatFactory
    {
        public const string Description = "Separates all encoding channels as separate textures.";


        public TextureMappingCollection Create()
        {
            return new TextureMappingCollection {
                new() {
                    Name = (p, m) => "color",
                    Tag = TextureTags.Color,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.ColorRed,
                            Color = ColorChannel.Red,
                            MinValue = 0,
                            MaxValue = 255,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.ColorGreen,
                            Color = ColorChannel.Green,
                            MinValue = 0,
                            MaxValue = 255,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.ColorBlue,
                            Color = ColorChannel.Blue,
                            MinValue = 0,
                            MaxValue = 255,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                    },
                },
                new() {
                    Name = (p, m) => "opacity",
                    Tag = TextureTags.Opacity,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Opacity,
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
                    },
                },
                new() {
                    Name = (p, m) => "height",
                    Tag = TextureTags.Height,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Height,
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
                    },
                },
                new() {
                    Name = (p, m) => "occlusion",
                    Tag = TextureTags.Occlusion,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Occlusion,
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
                    },
                },
                new() {
                    Name = (p, m) => "normal",
                    Tag = TextureTags.Normal,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.NormalX,
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
                        new ChannelMapping {
                            Type = EncodingChannel.NormalY,
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
                        new ChannelMapping {
                            Type = EncodingChannel.NormalZ,
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
                    },
                },
                new() {
                    Name = (p, m) => "specular",
                    Tag = TextureTags.Specular,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Specular,
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
                    },
                },
                new() {
                    Name = (p, m) => "smooth",
                    Tag = TextureTags.Smooth,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Smooth,
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
                    },
                },
                new() {
                    Name = (p, m) => "rough",
                    Tag = TextureTags.Rough,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Rough,
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
                    },
                },
                new() {
                    Name = (p, m) => "metal",
                    Tag = TextureTags.Metal,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Metal,
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
                        },
                    },
                },
                new() {
                    Name = (p, m) => "f0",
                    Tag = TextureTags.F0,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.F0,
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
                    },
                },
                new() {
                    Name = (p, m) => "porosity",
                    Tag = TextureTags.Porosity,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Porosity,
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
                    },
                },
                new() {
                    Name = (p, m) => "sss",
                    Tag = TextureTags.SubSurfaceScattering,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.SubSurfaceScattering,
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
                    },
                },
                new() {
                    Name = (p, m) => "emissive",
                    Tag = TextureTags.Emissive,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Emissive,
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
                    },
                },
            };
        }
    }
}
