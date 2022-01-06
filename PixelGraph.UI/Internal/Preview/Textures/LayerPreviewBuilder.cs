using PixelGraph.Common.Material;
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

        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, MaterialProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Opacity] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Red) {
                        //Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                        DefaultValue = 255m,
                    },
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Green) {
                        //Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                        DefaultValue = 255m,
                    },
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Blue) {
                        //Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                        DefaultValue = 255m,
                    },
                },
                [TextureTags.Color] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackColorRedChannelProperties(TextureTags.Color, ColorChannel.Red) {
                        //Sampler = mat?.Color?.InputRed?.Sampler ?? profile?.Encoding?.ColorRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackColorGreenChannelProperties(TextureTags.Color, ColorChannel.Green) {
                        //Sampler = mat?.Color?.InputGreen?.Sampler ?? profile?.Encoding?.ColorGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackColorBlueChannelProperties(TextureTags.Color, ColorChannel.Blue) {
                        //Sampler = mat?.Color?.InputBlue?.Sampler ?? profile?.Encoding?.ColorBlue?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Height] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Red) {
                        //Sampler = mat?.Height?.Input?.Sampler ?? profile?.Encoding?.Height?.Sampler,
                        DefaultValue = 0m,
                        Invert = true,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Green) {
                        //Sampler = mat?.Height?.Input?.Sampler ?? profile?.Encoding?.Height?.Sampler,
                        DefaultValue = 0m,
                        Invert = true,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Blue) {
                        //Sampler = mat?.Height?.Input?.Sampler ?? profile?.Encoding?.Height?.Sampler,
                        DefaultValue = 0m,
                        Invert = true,
                    },
                },
                [TextureTags.Occlusion] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Red) {
                        //Sampler = mat?.Occlusion?.Input?.Sampler ?? profile?.Encoding?.Occlusion?.Sampler,
                        //Invert = true,
                        DefaultValue = 0m,
                    },
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Green){
                        //Sampler = mat?.Occlusion?.Input?.Sampler ?? profile?.Encoding?.Occlusion?.Sampler,
                        //Invert = true,
                        DefaultValue = 0m,
                    },
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Blue){
                        //Sampler = mat?.Occlusion?.Input?.Sampler ?? profile?.Encoding?.Occlusion?.Sampler,
                        //Invert = true,
                        DefaultValue = 0m,
                    },
                },
                [TextureTags.Normal] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red) {
                        //Sampler = mat?.Normal?.InputX?.Sampler ?? profile?.Encoding?.NormalX?.Sampler,
                    },
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green) {
                        //Sampler = mat?.Normal?.InputY?.Sampler ?? profile?.Encoding?.NormalY?.Sampler,
                    },
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue) {
                        //Sampler = mat?.Normal?.InputZ?.Sampler ?? profile?.Encoding?.NormalZ?.Sampler,
                    },
                },
                [TextureTags.Specular] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Red) {
                        //Sampler = mat?.Specular?.Input?.Sampler ?? profile?.Encoding?.Specular?.Sampler,
                    },
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Green) {
                        //Sampler = mat?.Specular?.Input?.Sampler ?? profile?.Encoding?.Specular?.Sampler,
                    },
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Blue) {
                        //Sampler = mat?.Specular?.Input?.Sampler ?? profile?.Encoding?.Specular?.Sampler,
                    },
                },
                [TextureTags.Smooth] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Red) {
                        //Sampler = mat?.Smooth?.Input?.Sampler ?? profile?.Encoding?.Smooth?.Sampler,
                    },
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Green) {
                        //Sampler = mat?.Smooth?.Input?.Sampler ?? profile?.Encoding?.Smooth?.Sampler,
                    },
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Blue) {
                        //Sampler = mat?.Smooth?.Input?.Sampler ?? profile?.Encoding?.Smooth?.Sampler,
                    },
                },
                [TextureTags.Rough] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red) {
                        //Sampler = mat?.Rough?.Input?.Sampler ?? profile?.Encoding?.Rough?.Sampler,
                    },
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Green) {
                        //Sampler = mat?.Rough?.Input?.Sampler ?? profile?.Encoding?.Rough?.Sampler,
                    },
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Blue) {
                        //Sampler = mat?.Rough?.Input?.Sampler ?? profile?.Encoding?.Rough?.Sampler,
                    },
                },
                [TextureTags.Metal] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Red) {
                        //Sampler = mat?.Metal?.Input?.Sampler ?? profile?.Encoding?.Metal?.Sampler,
                    },
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Green) {
                        //Sampler = mat?.Metal?.Input?.Sampler ?? profile?.Encoding?.Metal?.Sampler,
                    },
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Blue) {
                        //Sampler = mat?.Metal?.Input?.Sampler ?? profile?.Encoding?.Metal?.Sampler,
                    },
                },
                [TextureTags.F0] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Red) {
                        //Sampler = mat?.F0?.Input?.Sampler ?? profile?.Encoding?.F0?.Sampler,
                    },
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Green) {
                        //Sampler = mat?.F0?.Input?.Sampler ?? profile?.Encoding?.F0?.Sampler,
                    },
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Blue) {
                        //Sampler = mat?.F0?.Input?.Sampler ?? profile?.Encoding?.F0?.Sampler,
                    },
                },
                [TextureTags.Porosity] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red) {
                        //Sampler = mat?.Porosity?.Input?.Sampler ?? profile?.Encoding?.Porosity?.Sampler,
                    },
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Green) {
                        //Sampler = mat?.Porosity?.Input?.Sampler ?? profile?.Encoding?.Porosity?.Sampler,
                    },
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Blue) {
                        Sampler = mat?.Porosity?.Input?.Sampler ?? profile?.Encoding?.Porosity?.Sampler,
                    },
                },
                [TextureTags.SubSurfaceScattering] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Red) {
                        //Sampler = mat?.SSS?.Input?.Sampler ?? profile?.Encoding?.SSS?.Sampler,
                    },
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Green) {
                        //Sampler = mat?.SSS?.Input?.Sampler ?? profile?.Encoding?.SSS?.Sampler,
                    },
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Blue) {
                        //Sampler = mat?.SSS?.Input?.Sampler ?? profile?.Encoding?.SSS?.Sampler,
                    },
                },
                [TextureTags.Emissive] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red) {
                        //Sampler = mat?.Emissive?.Input?.Sampler ?? profile?.Encoding?.Emissive?.Sampler,
                    },
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Green) {
                        //Sampler = mat?.Emissive?.Input?.Sampler ?? profile?.Encoding?.Emissive?.Sampler,
                    },
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Blue) {
                        //Sampler = mat?.Emissive?.Input?.Sampler ?? profile?.Encoding?.Emissive?.Sampler,
                    },
                },
            };
    }
}
