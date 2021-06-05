using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview
{
    public interface ILayerPreviewBuilder : ITexturePreviewBuilder {}

    internal class LayerPreviewBuilder : TexturePreviewBuilder, ILayerPreviewBuilder
    {
        public LayerPreviewBuilder(IServiceProvider provider) : base(provider)
        {
            TagMap = tagMap;
        }

        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Alpha] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Diffuse] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackDiffuseRedChannelProperties(TextureTags.Diffuse, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.DiffuseRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackDiffuseGreenChannelProperties(TextureTags.Diffuse, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.DiffuseGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackDiffuseBlueChannelProperties(TextureTags.Diffuse, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.DiffuseBlue?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Albedo] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackAlbedoRedChannelProperties(TextureTags.Albedo, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.AlbedoRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlbedoGreenChannelProperties(TextureTags.Albedo, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.AlbedoGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlbedoBlueChannelProperties(TextureTags.Albedo, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.AlbedoBlue?.Sampler,
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
