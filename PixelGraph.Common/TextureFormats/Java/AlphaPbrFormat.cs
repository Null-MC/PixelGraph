using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class AlphaPbrFormat : ITextureFormatFactory
    {
        public const string Description = "Uses a diffuse color map, with special encoding of the alpha channel for PBR materials. Proposed by Espen 2021";


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
                            DefaultValue = 255,
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
                            MinValue = 0m,
                            MaxValue = 255m,
                            RangeMin = 0,
                            RangeMax = 255,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                        },
                        new ChannelMapping {
                            Type = TextureTags.Opacity,
                            Color = ColorChannel.Alpha,
                            MinValue = 0m,
                            MaxValue = 1m,
                            RangeMin = 0,
                            RangeMax = 0,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                            Priority = 10,
                            //DefaultValue = 255,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.SubSurfaceScattering,
                            Color = ColorChannel.Alpha,
                            MinValue = 0m,
                            MaxValue = 1m,
                            RangeMin = 22,
                            RangeMax = 47,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                            Priority = 3,
                            ClipValue = 0m,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.Emissive,
                            Color = ColorChannel.Alpha,
                            MinValue = 0m,
                            MaxValue = 1m,
                            RangeMin = 48,
                            RangeMax = 72,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                            Priority = 4,
                            ClipValue = 0m,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.Smooth,
                            Color = ColorChannel.Alpha,
                            MinValue = 0m,
                            MaxValue = 1m,
                            RangeMin = 73,
                            RangeMax = 157,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                            Priority = 1,
                            DefaultValue = 0m,
                            //ClipValue = 0m,
                        },
                        new ChannelMapping {
                            Type = EncodingChannel.Metal,
                            Color = ColorChannel.Alpha,
                            MinValue = 230m,
                            MaxValue = 255m,
                            RangeMin = 251,
                            RangeMax = 251,
                            Shift = 0,
                            Power = 1m,
                            Invert = false,
                            Priority = 2,
                            //DefaultValue = 0m,
                            //ClipValue = 0m,
                        },
                    },
                },
            };
        }
    }
}
