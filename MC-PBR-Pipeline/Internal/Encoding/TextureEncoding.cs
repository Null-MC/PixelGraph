using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace McPbrPipeline.Internal.Encoding
{
    internal class TextureEncoding
    {
        public string R {get; set;}
        public string G {get; set;}
        public string B {get; set;}
        public string A {get; set;}


        //public bool Any()
        //{
        //    if (!string.IsNullOrEmpty(R)) return true;
        //    if (!string.IsNullOrEmpty(G)) return true;
        //    if (!string.IsNullOrEmpty(B)) return true;
        //    if (!string.IsNullOrEmpty(A)) return true;
        //    return false;
        //}

        public string Get(ColorChannel color)
        {
            return color switch {
                ColorChannel.Red => R,
                ColorChannel.Green => G,
                ColorChannel.Blue => B,
                ColorChannel.Alpha => A,
                _ => null,
            };
        }

        public ColorChannel GetChannel(string encodingChannel)
        {
            //if (string.Equals(R, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Red;
            //if (string.Equals(G, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Green;
            //if (string.Equals(B, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Blue;
            //if (string.Equals(A, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) return ColorChannel.Alpha;
            //return ColorChannel.None;
            return GetChannels(encodingChannel).FirstOrDefault();
        }

        public IEnumerable<ColorChannel> GetChannels(string encodingChannel)
        {
            if (string.Equals(R, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Red;
            if (string.Equals(G, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Green;
            if (string.Equals(B, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Blue;
            if (string.Equals(A, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Alpha;
        }

        public static TextureEncoding CreateOutput(EncodingProperties encoding, string tag)
        {
            if (outputMap.TryGetValue(tag, out var encodingFunc))
                return encodingFunc(encoding);

            throw new ApplicationException($"Unknown texture type '{tag}'!");
        }

        private static readonly Dictionary<string, Func<EncodingProperties, TextureEncoding>> outputMap = new Dictionary<string, Func<EncodingProperties, TextureEncoding>>(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Albedo] = encoding => new TextureEncoding {
                R = encoding.OutputAlbedoR,
                G = encoding.OutputAlbedoG,
                B = encoding.OutputAlbedoB,
                A = encoding.OutputAlbedoA,
            },
            [TextureTags.Height] = encoding => new TextureEncoding {
                R = encoding.OutputHeightR,
                G = encoding.OutputHeightG,
                B = encoding.OutputHeightB,
                A = encoding.OutputHeightA,
            },
            [TextureTags.Normal] = encoding => new TextureEncoding {
                R = encoding.OutputNormalR,
                G = encoding.OutputNormalG,
                B = encoding.OutputNormalB,
                A = encoding.OutputNormalA,
            },
            [TextureTags.Specular] = encoding => new TextureEncoding {
                R = encoding.OutputSpecularR,
                G = encoding.OutputSpecularG,
                B = encoding.OutputSpecularB,
                A = encoding.OutputSpecularA,
            },
            [TextureTags.Emissive] = encoding => new TextureEncoding {
                R = encoding.OutputEmissiveR,
                G = encoding.OutputEmissiveG,
                B = encoding.OutputEmissiveB,
                A = encoding.OutputEmissiveA,
            },
            [TextureTags.Occlusion] = encoding => new TextureEncoding {
                R = encoding.OutputOcclusionR,
                G = encoding.OutputOcclusionG,
                B = encoding.OutputOcclusionB,
                A = encoding.OutputOcclusionA,
            },
        };
    }
}
