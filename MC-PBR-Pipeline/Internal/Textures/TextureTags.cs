using McPbrPipeline.Internal.Input;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Textures
{
    internal static class TextureTags
    {
        public const string Albedo = "#albedo";
        public const string Height = "#height";
        public const string NormalGenerated = "#normal-generated";
        public const string Normal = "#normal";
        public const string Specular = "#specular";
        public const string Smooth = "#smooth";
        public const string Metal = "#metal";
        public const string Occlusion = "#occlusion";
        public const string Emissive = "#emissive";

        public static string[] All {get;} = {Albedo, Height, Normal, Specular, Emissive, Occlusion, Smooth, Metal};


        public static string Get(PbrProperties material, string tag)
        {
            return map.TryGetValue(tag, out var fileFunc)
                ? fileFunc(material) : null;
        }

        public static string GetMatchName(PbrProperties texture, string type)
        {
            return texture.UseGlobalMatching
                ? globalMatchMap[type](texture.Name)
                : localMatchMap[type];
        }

        private static readonly Dictionary<string, Func<PbrProperties, string>> map = new Dictionary<string, Func<PbrProperties, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = t => t.AlbedoTexture,
            [Height] = t => t.HeightTexture,
            [Normal] = t => t.NormalTexture,
            [Specular] = t => t.SpecularTexture,
            [Smooth] = t => t.SmoothTexture,
            [Metal] = t => t.MetalTexture,
            [Occlusion] = t => t.OcclusionTexture,
            [Emissive] = t => t.EmissiveTexture,
        };

        private static readonly Dictionary<string, string> localMatchMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = "albedo.*",
            [Height] = "height.*",
            [Normal] = "normal.*",
            [Specular] = "specular.*",
            [Smooth] = "smooth.*",
            [Metal] = "metal.*",
            [Occlusion] = "occlusion.*",
            [Emissive] = "emissive.*",
        };

        private static readonly Dictionary<string, Func<string, string>> globalMatchMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = item => $"{item}.*",
            [Height] = item => $"{item}_h.*",
            [Normal] = item => $"{item}_n.*",
            [Specular] = item => $"{item}_s.*",
            [Smooth] = item => $"{item}_smooth.*",
            [Metal] = item => $"{item}_metal.*",
            [Occlusion] = item => $"{item}_ao.*",
            [Emissive] = item => $"{item}_e.*",
        };
    }
}
