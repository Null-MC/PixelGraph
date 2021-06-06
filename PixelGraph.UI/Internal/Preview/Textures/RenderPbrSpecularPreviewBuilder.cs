using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface IRenderPbrSpecularPreviewBuilder : ITexturePreviewBuilder {}

    internal class RenderPbrSpecularPreviewBuilder : TexturePreviewBuilderBase, IRenderPbrSpecularPreviewBuilder
    {
        public RenderPbrSpecularPreviewBuilder(IServiceProvider provider) : base(provider)
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
                [TextureTags.Normal] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red),
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green),
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue),
                    new ResourcePackHeightChannelProperties(TextureTags.Normal, ColorChannel.Alpha) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                },
                [TextureTags.Rough] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red),
                    new ResourcePackF0ChannelProperties(TextureTags.Rough, ColorChannel.Green),
                    new ResourcePackOcclusionChannelProperties(TextureTags.Rough, ColorChannel.Blue) {
                        Invert = true,
                    },
                },
                [TextureTags.Porosity] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red),
                    new ResourcePackSssChannelProperties(TextureTags.Porosity, ColorChannel.Green),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Porosity, ColorChannel.Blue),
                },
            };
    }
}
