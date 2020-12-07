using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Textures
{
    public static class TextureTags
    {
        public const string Albedo = "albedo";
        public const string Diffuse = "diffuse";
        public const string Height = "height";
        public const string Normal = "normal";
        public const string NormalGenerated = "normal-generated";
        public const string Occlusion = "occlusion";
        public const string OcclusionGenerated = "occlusion-generated";
        public const string Specular = "specular";
        public const string Rough = "rough";
        public const string Smooth = "smooth";
        public const string Metal = "metal";
        public const string Porosity = "porosity";
        public const string SubSurfaceScattering = "sss";
        public const string Emissive = "emissive";

        public static string[] All {get;} = {Albedo, Diffuse, Height, Normal, Occlusion, Specular, Smooth, Rough, Metal, Porosity, SubSurfaceScattering, Emissive};


        public static string Get(MaterialProperties material, string tag)
        {
            return map.TryGetValue(tag, out var fileFunc)
                ? fileFunc(material) : null;
        }


        public static bool Is(string tagActual, string tagExpected) => string.Equals(tagActual, tagExpected, StringComparison.InvariantCultureIgnoreCase);

        private static readonly Dictionary<string, Func<MaterialProperties, string>> map = new Dictionary<string, Func<MaterialProperties, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Albedo] = mat => mat.Albedo?.Texture,
            [Diffuse] = mat => mat.Diffuse?.Texture,
            [Height] = mat => mat.Height?.Texture,
            [Normal] = mat => mat.Normal?.Texture,
            [Occlusion] = mat => mat.Occlusion?.Texture,
            [Specular] = mat => mat.Specular?.Texture,
            [Smooth] = mat => mat.Smooth?.Texture,
            [Rough] = mat => mat.Rough?.Texture,
            [Metal] = mat => mat.Metal?.Texture,
            [Porosity] = mat => mat.Porosity?.Texture,
            [SubSurfaceScattering] = mat => mat.SSS?.Texture,
            [Emissive] = mat => mat.Emissive?.Texture,
        };
    }
}
