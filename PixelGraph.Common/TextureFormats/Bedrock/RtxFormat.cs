using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Bedrock
{
    public class RtxFormat : ITextureFormatFactory
    {
        public const string Description = "The NVidia standard for Bedrock RTX PBR.";


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
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 128,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                    DefaultValue = 255,
                },

                //Height = new ResourcePackHeightChannelProperties {
                //    Texture = TextureTags.Height,
                //    Color = ColorChannel.Red,
                //    MinValue = 0,
                //    MaxValue = 255,
                //    RangeMin = 0,
                //    RangeMax = 255,
                //    Shift = 0,
                //    Power = 1m,
                //    Invert = true,
                //    DefaultValue = 0,
                //},

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.MER,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.MER,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Rough = new ResourcePackRoughChannelProperties {
                    Texture = TextureTags.MER,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },
            };
        }
    }
}
