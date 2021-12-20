using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats
{
    public class ColorFormat : ITextureFormatFactory
    {
        public const string Description = "Uses only the albedo color map and alpha channel.";


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
            };
        }
    }
}
