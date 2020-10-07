using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Encoding
{
    internal class TextureEncoding
    {
        public string R {get; set;}
        public string G {get; set;}
        public string B {get; set;}
        public string A {get; set;}


        public bool Any()
        {
            if (!string.IsNullOrEmpty(R)) return true;
            if (!string.IsNullOrEmpty(G)) return true;
            if (!string.IsNullOrEmpty(B)) return true;
            if (!string.IsNullOrEmpty(A)) return true;
            return false;
        }

        public ColorChannel GetChannel(string encodingChannel)
        {
            if (string.Equals(R, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Red;
            if (string.Equals(G, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Green;
            if (string.Equals(B, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Blue;
            if (string.Equals(A, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Alpha;
            return ColorChannel.None;
        }

        public static TextureEncoding CreateOutput(PackProperties pack, string tag)
        {
            if (outputMap.TryGetValue(tag, out var encodingFunc))
                return encodingFunc(pack);

            throw new ApplicationException($"Unknown texture type '{tag}'!");
        }

        private static readonly Dictionary<string, Func<PackProperties, TextureEncoding>> outputMap = new Dictionary<string, Func<PackProperties, TextureEncoding>>(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Albedo] = pack => new TextureEncoding {
                R = pack.OutputAlbedoR,
                G = pack.OutputAlbedoG,
                B = pack.OutputAlbedoB,
                A = pack.OutputAlbedoA,
            },
            [TextureTags.Height] = pack => new TextureEncoding {
                R = pack.OutputHeightR,
                G = pack.OutputHeightG,
                B = pack.OutputHeightB,
                A = pack.OutputHeightA,
            },
            [TextureTags.Normal] = pack => new TextureEncoding {
                R = pack.OutputNormalR,
                G = pack.OutputNormalG,
                B = pack.OutputNormalB,
                A = pack.OutputNormalA,
            },
            [TextureTags.Specular] = pack => new TextureEncoding {
                R = pack.OutputSpecularR,
                G = pack.OutputSpecularG,
                B = pack.OutputSpecularB,
                A = pack.OutputSpecularA,
            },
            [TextureTags.Emissive] = pack => new TextureEncoding {
                R = pack.OutputEmissiveR,
                G = pack.OutputEmissiveG,
                B = pack.OutputEmissiveB,
                A = pack.OutputEmissiveA,
            },
            [TextureTags.Occlusion] = pack => new TextureEncoding {
                R = pack.OutputOcclusionR,
                G = pack.OutputOcclusionG,
                B = pack.OutputOcclusionB,
                A = pack.OutputOcclusionA,
            },
        };
    }
}
