using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.TextureFormats
{
    public class AlbedoFormat : ITextureFormatFactory
    {
        public const string Description = "Uses only the albedo color map and alpha channel.";


        public ResourcePackEncoding Create()
        {
            return new() {
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
            };
        }
    }
}
