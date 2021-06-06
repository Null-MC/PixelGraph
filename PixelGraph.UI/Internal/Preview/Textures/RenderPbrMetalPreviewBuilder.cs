using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface IRenderPbrMetalPreviewBuilder : ITexturePreviewBuilder {}

    internal class RenderPbrMetalPreviewBuilder : TexturePreviewBuilderBase, IRenderPbrMetalPreviewBuilder
    {
        public RenderPbrMetalPreviewBuilder(IServiceProvider provider) : base(provider)
        {
            TagMap = tagMap;
        }
        
        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
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
                    new ResourcePackAlphaChannelProperties(TextureTags.Albedo, ColorChannel.Alpha) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
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
                [TextureTags.Normal] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red),
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green),
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue),
                },
                [TextureTags.Rough] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackOcclusionChannelProperties(TextureTags.Rough, ColorChannel.Red) {Invert = true},
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Green),
                    new ResourcePackMetalChannelProperties(TextureTags.Rough, ColorChannel.Blue),
                },
                [TextureTags.Emissive] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Green),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Blue),
                },
            };
    }
}
