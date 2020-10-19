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
        public const string Rough = "#rough";
        public const string Smooth = "#smooth";
        public const string Metal = "#metal";
        public const string Occlusion = "#occlusion";
        public const string Emissive = "#emissive";

        public static string[] All {get;} = {Albedo, Height, Normal, Specular, Smooth, Rough, Metal, Occlusion, Emissive};


        public static string Get(PbrProperties material, string tag)
        {
            return map.TryGetValue(tag, out var fileFunc)
                ? fileFunc(material) : null;
        }


        public static bool Is(string tagActual, string tagExpected) => string.Equals(tagActual, tagExpected, StringComparison.InvariantCultureIgnoreCase);

        private static readonly Dictionary<string, Func<PbrProperties, string>> map = new Dictionary<string, Func<PbrProperties, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = t => t.AlbedoTexture,
            [Height] = t => t.HeightTexture,
            [Normal] = t => t.NormalTexture,
            [Specular] = t => t.SpecularTexture,
            [Smooth] = t => t.SmoothTexture,
            [Rough] = t => t.RoughTexture,
            [Metal] = t => t.MetalTexture,
            [Occlusion] = t => t.OcclusionTexture,
            [Emissive] = t => t.EmissiveTexture,
        };
    }
}
