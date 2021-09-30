using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class VanillaPbrFormat : ITextureFormatFactory
    {
        public const string Description = "Uses a diffuse color map, with special encoding of the alpha channel for PBR materials. Proposed by Espen 2021";


        public ResourcePackEncoding Create()
        {
            return new() {
                ColorRed = new ResourcePackColorRedChannelProperties {
                    Texture = TextureTags.Color,
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

                ColorGreen = new ResourcePackColorGreenChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
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
                },

                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Color,
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

                SSS = new ResourcePackSssChannelProperties {
                    Texture = TextureTags.Color,
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

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Color,
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

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Color,
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

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Color,
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
            };
        }
    }
}
