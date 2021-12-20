using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class OldPbrFormat : ITextureFormatFactory
    {
        public const string Description = "The pre-Lab standard for PBR, also known as \"Old PBR\".";


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
                        new ChannelMapping {
                            Type = EncodingChannel.Opacity,
                            Color = ColorChannel.Alpha,
                            MinValue = 0,
                            MaxValue = 255,
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
                        new ChannelMapping {
                            Type = EncodingChannel.Height,
                            Color = ColorChannel.Alpha,
                            MinValue = 0m,
                            MaxValue = 1m,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = true,
                        },
                    },
                },
                new() {
                    Name = (p, m) => $"{m.Name}_s",
                    Tag = TextureTags.Specular,
                    Channels = {
                        new ChannelMapping {
                            Type = EncodingChannel.Smooth,
                            Color = ColorChannel.Red,
                            MinValue = 0,
                            MaxValue = 1,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.Metal,
                            Color = ColorChannel.Green,
                            MinValue = 255,
                            MaxValue = 255,
                            RangeMin = 255,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.Emissive,
                            Color = ColorChannel.Blue,
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
