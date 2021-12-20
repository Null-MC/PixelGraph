using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class SpecularFormat : ITextureFormatFactory
    {
        public const string Description = "Uses a diffuse color map, normal XYZ, and legacy grayscale specular channels.";


        public TextureMappingCollection Create()
        {
            return new TextureMappingCollection {
                new() {
                    Name = (p, m) => m.Name,
                    Tag = TextureTags.Color,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.ColorRed,
                            Color = ColorChannel.Red,
                            MinValue = 0m,
                            MaxValue = 255m,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.ColorGreen,
                            Color = ColorChannel.Green,
                            MinValue = 0m,
                            MaxValue = 255m,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.ColorBlue,
                            Color = ColorChannel.Blue,
                            MinValue = 0m,
                            MaxValue = 255m,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.Opacity,
                            Color = ColorChannel.Alpha,
                            MinValue = 0m,
                            MaxValue = 255m,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                            DefaultValue = 255,
                        },
                    },
                },
                new() {
                    Name = (p, m) => $"{m.Name}_n",
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
                        },
                    },
                },
                new() {
                    Name = (p, m) => $"{m.Name}_s",
                    Tag = TextureTags.Specular,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Specular,
                            Color = ColorChannel.Red,
                            MinValue = 0,
                            MaxValue = 1,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                    },
                },
            };
        }
    }
}
