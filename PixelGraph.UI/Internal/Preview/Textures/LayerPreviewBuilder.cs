using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface ILayerPreviewBuilder : ITexturePreviewBuilder {}

    internal class LayerPreviewBuilder : TexturePreviewBuilderBase, ILayerPreviewBuilder
    {
        public LayerPreviewBuilder(IServiceProvider provider) : base(provider)
        {
            TagMap = tagMap;
        }

        private static readonly Dictionary<string, Func<PublishProfileProperties, MaterialProperties, PackEncodingChannel[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Opacity] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Red) {
                        //Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                        //Power = 2.2m,
                        DefaultValue = 1m,
                    },
                },
                [TextureTags.Color] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackColorRedChannelProperties(TextureTags.Color, ColorChannel.Red) {
                        //Sampler = mat?.Color?.InputRed?.Sampler ?? profile?.Encoding?.ColorRed?.Sampler,
                        //Power = 2.2m,
                    },
                    new ResourcePackColorGreenChannelProperties(TextureTags.Color, ColorChannel.Green) {
                        //Sampler = mat?.Color?.InputGreen?.Sampler ?? profile?.Encoding?.ColorGreen?.Sampler,
                        //Power = 2.2m,
                    },
                    new ResourcePackColorBlueChannelProperties(TextureTags.Color, ColorChannel.Blue) {
                        //Sampler = mat?.Color?.InputBlue?.Sampler ?? profile?.Encoding?.ColorBlue?.Sampler,
                        //Power = 2.2m,
                    },
                },
                [TextureTags.Height] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Red) {
                        //Sampler = mat?.Height?.Input?.Sampler ?? profile?.Encoding?.Height?.Sampler,
                        DefaultValue = 0m,
                        Invert = true,
                    },
                },
                [TextureTags.Occlusion] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Red) {
                        //Sampler = mat?.Occlusion?.Input?.Sampler ?? profile?.Encoding?.Occlusion?.Sampler,
                        //Invert = true,
                        DefaultValue = 0m,
                    },
                },
                [TextureTags.Normal] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red) {
                        //Sampler = mat?.Normal?.InputX?.Sampler ?? profile?.Encoding?.NormalX?.Sampler,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green) {
                        //Sampler = mat?.Normal?.InputY?.Sampler ?? profile?.Encoding?.NormalY?.Sampler,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue) {
                        //Sampler = mat?.Normal?.InputZ?.Sampler ?? profile?.Encoding?.NormalZ?.Sampler,
                        MinValue = 0m,
                        MaxValue = 1m,
                        DefaultValue = 1m,
                    },
                },
                [TextureTags.Specular] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Red) {
                        //Sampler = mat?.Specular?.Input?.Sampler ?? profile?.Encoding?.Specular?.Sampler,
                    },
                },
                [TextureTags.Smooth] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Red) {
                        //Sampler = mat?.Smooth?.Input?.Sampler ?? profile?.Encoding?.Smooth?.Sampler,
                    },
                },
                [TextureTags.Rough] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red) {
                        //Sampler = mat?.Rough?.Input?.Sampler ?? profile?.Encoding?.Rough?.Sampler,
                    },
                },
                [TextureTags.Metal] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Red) {
                        //Sampler = mat?.Metal?.Input?.Sampler ?? profile?.Encoding?.Metal?.Sampler,
                    },
                },
                [TextureTags.HCM] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackHcmChannelProperties(TextureTags.HCM, ColorChannel.Red) {
                        //Sampler = mat?.HCM?.Input?.Sampler ?? profile?.Encoding?.HCM?.Sampler,
                        //Sampler = Samplers.Nearest,
                        MinValue = 230m,
                        MaxValue = 255m,
                        RangeMin = 230,
                        RangeMax = 255,
                        EnableClipping = true,
                    },
                },
                [TextureTags.F0] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Red) {
                        //Sampler = mat?.F0?.Input?.Sampler ?? profile?.Encoding?.F0?.Sampler,
                    },
                },
                [TextureTags.Porosity] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red) {
                        //Sampler = mat?.Porosity?.Input?.Sampler ?? profile?.Encoding?.Porosity?.Sampler,
                    },
                },
                [TextureTags.SubSurfaceScattering] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Red) {
                        //Sampler = mat?.SSS?.Input?.Sampler ?? profile?.Encoding?.SSS?.Sampler,
                    },
                },
                [TextureTags.Emissive] = (profile, mat) => new PackEncodingChannel[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red) {
                        //Sampler = mat?.Emissive?.Input?.Sampler ?? profile?.Encoding?.Emissive?.Sampler,
                    },
                },
            };
    }
}
