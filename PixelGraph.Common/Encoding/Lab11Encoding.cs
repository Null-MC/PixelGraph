using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    public class Lab11Encoding : ITextureEncodingFactory
    {
        public const string Description = "The first LabPBR standard.";


        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Alpha,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                AlbedoRed = new ResourcePackAlbedoRedChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                AlbedoGreen = new ResourcePackAlbedoGreenChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Green,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                AlbedoBlue = new ResourcePackAlbedoBlueChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 255m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Occlusion = new ResourcePackOcclusionChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Magnitude,
                    MinValue = 17,
                    MaxValue = 255,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 0.5m,
                    Invert = true,
                },

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Alpha,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = true,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 0.5m,
                    Invert = false,
                },

                F0 = new ResourcePackF0ChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Green,
                    MinValue = 0m,
                    MaxValue = 0.898m,
                    RangeMin = 0,
                    RangeMax = 229,
                    Shift = 0,
                    Power = 0.5m,
                    Invert = false,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Green,
                    Sampler = Sampler.Nearest,
                    MinValue = 230m,
                    MaxValue = 255m,
                    RangeMin = 230,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Porosity = new ResourcePackPorosityChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 64,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                SSS = new ResourcePackSssChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Blue,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 65,
                    RangeMax = 255,
                    Shift = 0,
                    Power = 1m,
                    Invert = false,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Alpha,
                    MinValue = 0m,
                    MaxValue = 1m,
                    RangeMin = 0,
                    RangeMax = 255,
                    Shift = -1,
                    Power = 1m,
                    Invert = false,
                },
            };
        }
    }
}
