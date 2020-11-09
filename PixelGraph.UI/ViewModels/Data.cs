using PixelGraph.Common.Encoding;
using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewModels
{
    internal class EncodingFormatValues : Dictionary<string, string>
    {
        public EncodingFormatValues() : base(new Dictionary<string, string> {
            [""] = "None",
            ["default"] = "Default",
            ["lab-1.1"] = "LAB 1.1",
            ["lab-1.3"] = "LAB 1.3",
        }) {}
    }

    internal class TextureTagValues : Dictionary<string, string>
    {
        public TextureTagValues() : base(new Dictionary<string, string> {
            [TextureTags.Albedo] = "Albedo",
            [TextureTags.Height] = "Height",
            [TextureTags.Normal] = "Normal",
            [TextureTags.Occlusion] = "Occlusion",
            [TextureTags.Specular] = "Specular",
            [TextureTags.Rough] = "Rough",
            [TextureTags.Smooth] = "Smooth",
            [TextureTags.Metal] = "Metal",
            [TextureTags.Porosity] = "Porosity",
            [TextureTags.SubSurfaceScattering] = "SubSurface Scattering",
            [TextureTags.Emissive] = "Emissive",
        }) {}
    }

    internal class EncodingChannelValues : Dictionary<string, string>
    {
        public EncodingChannelValues() : base(new Dictionary<string, string> {
            [""] = "None",
            [EncodingChannel.Red] = "Red",
            [EncodingChannel.Green] = "Green",
            [EncodingChannel.Blue] = "Blue",
            [EncodingChannel.Alpha] = "Alpha",
            [EncodingChannel.Height] = "Height",
            [EncodingChannel.Occlusion] = "Occlusion [AO]",
            [EncodingChannel.NormalX] = "Normal X",
            [EncodingChannel.NormalY] = "Normal Y",
            [EncodingChannel.NormalZ] = "Normal Z",
            [EncodingChannel.Smooth] = "Smoothness",
            [EncodingChannel.PerceptualSmooth] = "Perceptual Smoothness",
            [EncodingChannel.Rough] = "Roughness",
            [EncodingChannel.Metal] = "Metalness",
            [EncodingChannel.Porosity] = "Porosity",
            [EncodingChannel.Porosity_SSS] = "Porosity + SSS",
            [EncodingChannel.SubSurfaceScattering] = "SubSurface Scattering",
            [EncodingChannel.Emissive] = "Emissive",
            [EncodingChannel.EmissiveClipped] = "Emissive Clipped",
            [EncodingChannel.EmissiveInverse] = "Emissive Inverse",
        }) {}
    }
}
