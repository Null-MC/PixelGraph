using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    public class DiffuseEncoding : ITextureEncodingFactory
    {
        public const string Description = "Uses only a diffuse color map.";


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
                    //Perceptual = false,
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
                    //Perceptual = false,
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
                    //Perceptual = false,
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
                    //Perceptual = false,
                    Invert = false,
                },
            };
        }
    }
}
