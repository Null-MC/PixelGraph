using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class OldPbrFormat : ITextureFormatFactory
    {
        public const string Description = "The pre-Lab standard for PBR, also known as \"Old PBR\".";


        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                AlbedoRed = new ResourcePackAlbedoRedChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                AlbedoGreen = new ResourcePackAlbedoGreenChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                AlbedoBlue = new ResourcePackAlbedoBlueChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

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

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = true,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 1,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Green,
                    MinValue = 255,
                    MaxValue = 255,
                    RangeMin = 255,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Specular,
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
