using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats.Java
{
    public class SpecularFormat : ITextureFormatFactory
    {
        public const string Description = "Uses a diffuse color map, normal XYZ, and legacy grayscale specular channels.";


        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                DiffuseRed = new ResourcePackDiffuseRedChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                DiffuseGreen = new ResourcePackDiffuseGreenChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                DiffuseBlue = new ResourcePackDiffuseBlueChannelProperties {
                    Texture = TextureTags.Diffuse,
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

                Specular = new ResourcePackSpecularChannelProperties {
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
            };
        }
    }
}
