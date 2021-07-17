using System;
using System.Collections.Generic;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface ILayerPreviewBuilder : ITexturePreviewBuilder {}

    internal class LayerPreviewBuilder : TexturePreviewBuilderBase, ILayerPreviewBuilder
    {
        public LayerPreviewBuilder(IServiceProvider provider) : base(provider)
        {
            TagMap = tagMap;
        }

        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Opacity] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackOpacityChannelProperties(TextureTags.Opacity, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Color] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackColorRedChannelProperties(TextureTags.Color, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.ColorRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackColorGreenChannelProperties(TextureTags.Color, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.ColorGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackColorBlueChannelProperties(TextureTags.Color, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.ColorBlue?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Height] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                },
                [TextureTags.Occlusion] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Red), //{Invert = true};
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Green), //{Invert = true};
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Blue), //{Invert = true};
                },
                [TextureTags.Normal] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red),
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green),
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue),
                },
                [TextureTags.Specular] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Red),
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Green),
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Blue),
                },
                [TextureTags.Smooth] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Red),
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Green),
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Blue),
                },
                [TextureTags.Rough] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red),
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Green),
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Blue),
                },
                [TextureTags.Metal] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Red),
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Green),
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Blue),
                },
                [TextureTags.F0] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Red),
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Green),
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Blue),
                },
                [TextureTags.Porosity] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red),
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Green),
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Blue),
                },
                [TextureTags.SubSurfaceScattering] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Red),
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Green),
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Blue),
                },
                [TextureTags.Emissive] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Green),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Blue),
                },
            };
    }
}
