using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class AlphaPbrFormat : ITextureFormatFactory
    {
        public const string Description = "Uses a diffuse color map, with special encoding of the alpha channel for PBR materials. Proposed by Espen 2021";


        public PackEncoding Create()
        {
            return new() {
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

                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    RangeMin = 0,
                    RangeMax = 0,
                    Priority = 10,
                    //DefaultValue = 255,
                },

                SSS = new ResourcePackSssChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    RangeMin = 22,
                    RangeMax = 47,
                    Priority = 3,
                    ClipValue = 0m,
                    EnableClipping = true,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    RangeMin = 48,
                    RangeMax = 72,
                    Priority = 4,
                    ClipValue = 0m,
                    EnableClipping = true,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    RangeMin = 73,
                    RangeMax = 157,
                    Priority = 1,
                    DefaultValue = 0m,
                    //ClipValue = 0m,
                    EnableClipping = true,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    RangeMin = 251,
                    RangeMax = 251,
                    Priority = 2,
                    //DefaultValue = 0m,
                    ClipValue = 0m,
                    EnableClipping = true,
                },
            };
        }
    }
}
