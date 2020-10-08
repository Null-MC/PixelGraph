using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Output
{
    internal static class OutputNamingStructure
    {
        private static readonly Dictionary<string, Func<string, string>> localMap;
        private static readonly Dictionary<string, Func<string, string>> globalMap;


        static OutputNamingStructure()
        {
            globalMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = name => $"{name}.png",
                [TextureTags.Height] = name => $"{name}_h.png",
                [TextureTags.Normal] = name => $"{name}_n.png",
                [TextureTags.Specular] = name => $"{name}_s.png",
                [TextureTags.Emissive] = name => $"{name}_e.png",
                [TextureTags.Occlusion] = name => $"{name}_ao.png",
            };

            localMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = name => "albedo.png",
                [TextureTags.Height] = name => "height.png",
                [TextureTags.Normal] = name => "normal.png",
                [TextureTags.Specular] = name => "specular.png",
                [TextureTags.Emissive] = name => "emissive.png",
                [TextureTags.Occlusion] = name => "occlusion.png",
            };
        }

        public static string Get(string textureName, string tag, bool global)
        {
            var dict = global ? globalMap : localMap;
            return dict[tag](textureName);
        }
    }
}
