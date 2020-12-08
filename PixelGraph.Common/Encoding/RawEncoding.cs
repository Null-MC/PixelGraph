using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class RawEncoding : ITextureEncodingFactory
    {
        public ResourcePackEncoding Create()
        {
            return new ResourcePackEncoding {
                Alpha = new ResourcePackAlphaChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Alpha,
                    MinValue = 0,
                    MaxValue = 255,
                },

                DiffuseRed = new ResourcePackDiffuseRedChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                DiffuseGreen = new ResourcePackDiffuseGreenChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                DiffuseBlue = new ResourcePackDiffuseBlueChannelProperties {
                    Texture = TextureTags.Diffuse,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                },

                AlbedoRed = new ResourcePackAlbedoRedChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                AlbedoGreen = new ResourcePackAlbedoGreenChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                AlbedoBlue = new ResourcePackAlbedoBlueChannelProperties {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Height = new ResourcePackHeightChannelProperties {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    Invert = true,
                },

                Occlusion = new ResourcePackOcclusionChannelProperties {
                    Texture = TextureTags.Occlusion,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                    Invert = true,
                },

                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = 0,
                    MaxValue = 255,
                },

                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Specular = new ResourcePackSpecularChannelProperties {
                    Texture = TextureTags.Specular,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Smooth,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Rough = new ResourcePackRoughChannelProperties {
                    Texture = TextureTags.Rough,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Metal,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Porosity = new ResourcePackPorosityChannelProperties {
                    Texture = TextureTags.Porosity,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                SSS = new ResourcePackSssChannelProperties {
                    Texture = TextureTags.SubSurfaceScattering,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },

                Emissive = new ResourcePackEmissiveChannelProperties {
                    Texture = TextureTags.Emissive,
                    Color = ColorChannel.Red,
                    MinValue = 0,
                    MaxValue = 255,
                },
            };
        }
    }
}
