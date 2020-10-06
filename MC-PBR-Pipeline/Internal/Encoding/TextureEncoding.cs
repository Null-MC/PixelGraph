using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Encoding
{
    internal class TextureEncoding
    {
        //public string Tag {get; set;}
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

        public static TextureEncoding CreateOutput(PackProperties pack, string tag)
        {
            return outputMap.TryGetValue(tag, out var encodingFunc)
                ? encodingFunc(pack) : null;
        }

        private static readonly Dictionary<string, Func<PackProperties, TextureEncoding>> outputMap = new Dictionary<string, Func<PackProperties, TextureEncoding>>(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Albedo] = pack => new TextureEncoding {
                //Tag = TextureTags.Albedo,
                R = pack.AlbedoOutputR,
                G = pack.AlbedoOutputG,
                B = pack.AlbedoOutputB,
                A = pack.AlbedoOutputA,
            },
            [TextureTags.Normal] = pack => new TextureEncoding {
                //Tag = TextureTags.Normal,
                R = pack.NormalOutputR,
                G = pack.NormalOutputG,
                B = pack.NormalOutputB,
                A = pack.NormalOutputA,
            },
            [TextureTags.Specular] = pack => new TextureEncoding {
                //Tag = TextureTags.Specular,
                R = pack.SpecularOutputR,
                G = pack.SpecularOutputG,
                B = pack.SpecularOutputB,
                A = pack.SpecularOutputA,
            },
            [TextureTags.Emissive] = pack => new TextureEncoding {
                //Tag = TextureTags.Emissive,
                R = pack.EmissiveOutputR,
                G = pack.EmissiveOutputG,
                B = pack.EmissiveOutputB,
                A = pack.EmissiveOutputA,
            },
        };
    }
}
