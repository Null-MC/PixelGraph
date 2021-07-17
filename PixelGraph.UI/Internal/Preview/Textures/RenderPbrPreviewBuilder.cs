﻿using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface IRenderPbrPreviewBuilder : ITexturePreviewBuilder {}

    internal class RenderPbrPreviewBuilder : TexturePreviewBuilderBase, IRenderPbrPreviewBuilder
    {
        public RenderPbrPreviewBuilder(IServiceProvider provider) : base(provider)
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
                    new ResourcePackF0ChannelProperties(TextureTags.Rough, ColorChannel.Green) {
                        MaxValue = 0.9m,
                        RangeMax = 229,
                    },
                    new ResourcePackMetalChannelProperties(TextureTags.Rough, ColorChannel.Green) {
                        MinValue = 230m,
                        MaxValue = 255m,
                        RangeMin = 230,
                        RangeMax = 255,
                    },
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
