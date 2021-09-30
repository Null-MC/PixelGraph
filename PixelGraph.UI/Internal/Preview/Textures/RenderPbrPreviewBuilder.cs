using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
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
        
        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, MaterialProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Color] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackColorRedChannelProperties(TextureTags.Color, ColorChannel.Red) {
                        Sampler = mat?.Color?.InputRed?.Sampler ?? profile?.Encoding?.ColorRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackColorGreenChannelProperties(TextureTags.Color, ColorChannel.Green) {
                        Sampler = mat?.Color?.InputGreen?.Sampler ?? profile?.Encoding?.ColorGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackColorBlueChannelProperties(TextureTags.Color, ColorChannel.Blue) {
                        Sampler = mat?.Color?.InputBlue?.Sampler ?? profile?.Encoding?.ColorBlue?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackOpacityChannelProperties(TextureTags.Color, ColorChannel.Alpha) {
                        Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255m,
                        DefaultValue = 255m,
                    },
                },
                [TextureTags.Normal] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red) {
                        Sampler = mat?.Normal?.InputX?.Sampler ?? profile?.Encoding?.NormalX?.Sampler,
                        DefaultValue = 0.5m,
                    },
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green) {
                        Sampler = mat?.Normal?.InputY?.Sampler ?? profile?.Encoding?.NormalY?.Sampler,
                        DefaultValue = 0.5m,
                    },
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue) {
                        Sampler = mat?.Normal?.InputZ?.Sampler ?? profile?.Encoding?.NormalZ?.Sampler,
                        DefaultValue = 1m,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Normal, ColorChannel.Alpha) {
                        Sampler = mat?.Height?.Input?.Sampler ?? profile?.Encoding?.Height?.Sampler,
                        DefaultValue = 0m,
                        Invert = true,
                    },
                },
                [TextureTags.Rough] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red) {
                        Sampler = mat?.Rough?.Input?.Sampler ?? profile?.Encoding?.Rough?.Sampler,
                        DefaultValue = 1m,
                    },
                    new ResourcePackF0ChannelProperties(TextureTags.Rough, ColorChannel.Green) {
                        Sampler = mat?.F0?.Input?.Sampler ?? profile?.Encoding?.F0?.Sampler,
                        MaxValue = 0.9m,
                        RangeMax = 229,
                    },
                    new ResourcePackMetalChannelProperties(TextureTags.Rough, ColorChannel.Green) {
                        Sampler = mat?.Metal?.Input?.Sampler ?? profile?.Encoding?.Metal?.Sampler,
                        MinValue = 230m,
                        MaxValue = 255m,
                        RangeMin = 230,
                        RangeMax = 255,
                    },
                    new ResourcePackOcclusionChannelProperties(TextureTags.Rough, ColorChannel.Blue) {
                        Sampler = mat?.Occlusion?.Input?.Sampler ?? profile?.Encoding?.Occlusion?.Sampler,
                        Invert = true,
                        DefaultValue = 0,
                    },
                },
                [TextureTags.Porosity] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red) {
                        Sampler = mat?.Porosity?.Input?.Sampler ?? profile?.Encoding?.Porosity?.Sampler,
                    },
                    new ResourcePackSssChannelProperties(TextureTags.Porosity, ColorChannel.Green) {
                        Sampler = mat?.SSS?.Input?.Sampler ?? profile?.Encoding?.SSS?.Sampler,
                    },
                    new ResourcePackEmissiveChannelProperties(TextureTags.Porosity, ColorChannel.Blue) {
                        Sampler = mat?.Emissive?.Input?.Sampler ?? profile?.Encoding?.Emissive?.Sampler,
                    },
                },
            };
    }
}
