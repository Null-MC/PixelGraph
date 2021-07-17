using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface IRenderDiffusePreviewBuilder : ITexturePreviewBuilder {}

    internal class RenderDiffusePreviewBuilder : TexturePreviewBuilderBase, IRenderDiffusePreviewBuilder
    {
        public RenderDiffusePreviewBuilder(IServiceProvider provider) : base(provider)
        {
            TagMap = tagMap;
        }
        
        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
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
                    new ResourcePackOpacityChannelProperties(TextureTags.Color, ColorChannel.Alpha) {
                        Sampler = profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Emissive] = _ => new ResourcePackChannelProperties[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red),
                },
            };
    }
}
