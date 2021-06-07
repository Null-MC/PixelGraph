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
                [TextureTags.Diffuse] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackDiffuseRedChannelProperties(TextureTags.Diffuse, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.AlbedoRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackDiffuseGreenChannelProperties(TextureTags.Diffuse, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.AlbedoGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackDiffuseBlueChannelProperties(TextureTags.Diffuse, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.AlbedoBlue?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlphaChannelProperties(TextureTags.Diffuse, ColorChannel.Alpha) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                },
            };
    }
}
