using PixelGraph.Common.Material;
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
        
        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, MaterialProperties, ResourcePackChannelProperties[]>> tagMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
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
                    new ResourcePackOpacityChannelProperties(TextureTags.Color, ColorChannel.Alpha) {
                        //Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                        MaxValue = 255,
                        DefaultValue = 255m,
                    },
                },
                [TextureTags.Emissive] = (profile, mat) => new ResourcePackChannelProperties[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red) {
                        //Sampler = mat?.Emissive?.Input?.Sampler ?? profile?.Encoding?.Emissive?.Sampler,
                    },
                },
            };
    }
}
