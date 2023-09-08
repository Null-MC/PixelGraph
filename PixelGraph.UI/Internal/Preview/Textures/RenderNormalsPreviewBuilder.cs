using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Preview.Textures;

public interface IRenderNormalsPreviewBuilder : ITexturePreviewBuilder {}

internal class RenderNormalsPreviewBuilder : TexturePreviewBuilderBase, IRenderNormalsPreviewBuilder
{
    public RenderNormalsPreviewBuilder(IServiceProvider provider) : base(provider)
    {
        TagMap = tagMap;
    }
        
    private static readonly Dictionary<string, Func<PublishProfileProperties, MaterialProperties, PackEncodingChannel[]>> tagMap =
        new(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Color] = (profile, mat) => new PackEncodingChannel[] {
                new ResourcePackOpacityChannelProperties(TextureTags.Color, ColorChannel.Alpha) {
                    //Sampler = mat?.Opacity?.Input?.Sampler ?? profile?.Encoding?.Opacity?.Sampler,
                    //Power = 2.2m,
                    DefaultValue = 1m,
                },
            },
            [TextureTags.Normal] = (profile, mat) => new PackEncodingChannel[] {
                new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red) {
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green) {
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue) {
                    MinValue = 0m,
                    MaxValue = 1m,
                    DefaultValue = 1m,
                },
                new ResourcePackHeightChannelProperties(TextureTags.Normal, ColorChannel.Alpha) {
                    DefaultValue = 0m,
                    Invert = true,
                },
            },
        };
}