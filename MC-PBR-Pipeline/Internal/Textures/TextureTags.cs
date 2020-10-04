using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Textures
{
    internal static class TextureTags
    {
        public const string Albedo = "#albedo";
        public const string Height = "#height";
        public const string Normal = "#normal";
        public const string Specular = "#specular";
        public const string Emissive = "#emissive";

        public static string[] All {get;} = {Albedo, Height, Normal, Specular, Emissive};


        public static string Get(PbrProperties material, string tag)
        {
            return map.TryGetValue(tag, out var fileFunc)
                ? fileFunc(material) : null;
        }

        public static string GetMatchName(PbrProperties texture, string type)
        {
            return texture.UseGlobalMatching
                ? GlobalMatchMap[type](texture.Name)
                : LocalMatchMap[type];
        }

        private static readonly Dictionary<string, Func<PbrProperties, string>> map = new Dictionary<string, Func<PbrProperties, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = t => t.AlbedoTexture,
            [Height] = t => t.HeightTexture,
            [Normal] = t => t.NormalTexture,
            [Specular] = t => t.SpecularTexture,
            [Emissive] = t => t.EmissiveTexture,
        };

        private static readonly Dictionary<string, string> LocalMatchMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = "albedo.*",
            [Height] = "height.*",
            [Normal] = "normal.*",
            [Specular] = "specular.*",
            [Emissive] = "emissive.*",
        };

        private static readonly Dictionary<string, Func<string, string>> GlobalMatchMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = item => $"{item}.*",
            [Height] = item => $"{item}_h.*",
            [Normal] = item => $"{item}_n.*",
            [Specular] = item => $"{item}_s.*",
            [Emissive] = item => $"{item}_e.*",
        };
    }
}
