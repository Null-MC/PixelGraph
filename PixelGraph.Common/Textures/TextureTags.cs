using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Textures
{
    public static class TextureTags
    {
        public const string None = "none";
        public const string Opacity = "opacity";
        public const string Color = "color";
        public const string Height = "height";
        public const string Bump = "bump";
        public const string Normal = "normal";
        public const string Occlusion = "occlusion";
        public const string Specular = "specular";
        public const string Rough = "rough";
        public const string Smooth = "smooth";

        public const string Metal = "metal";
        public const string HCM = "hcm";
        //public const string Metal = "metal";

        public const string F0 = "f0";
        public const string Porosity = "porosity";
        public const string SubSurfaceScattering = "sss";
        public const string Emissive = "emissive";
        public const string MER = "mer";

        // Internal
        public const string General = "general";
        public const string Item = "item";
        public const string NormalGenerated = "normal-generated";
        public const string MagnitudeBuffer = "magnitude-buffer";
        public const string OcclusionGenerated = "occlusion-generated";

        public static string[] All {get;} = {Opacity, Color, Height, Bump, Normal, Occlusion, Specular, Smooth, Rough, Metal, HCM, F0, Porosity, SubSurfaceScattering, Emissive, Item, MER};


        public static string Get(MaterialProperties material, string tag)
        {
            return map.TryGetValue(tag, out var fileFunc)
                ? fileFunc(material) : null;
        }


        public static bool Is(string tagActual, string tagExpected) => string.Equals(tagActual, tagExpected, StringComparison.InvariantCultureIgnoreCase);

        private static readonly Dictionary<string, Func<MaterialProperties, string>> map = new(StringComparer.InvariantCultureIgnoreCase) {
            [Opacity] = mat => mat.Opacity?.Texture,
            [Color] = mat => mat.Color?.Texture,
            [Height] = mat => mat.Height?.Texture,
            [Bump] = mat => mat.Bump?.Texture,
            [Normal] = mat => mat.Normal?.Texture,
            [Occlusion] = mat => mat.Occlusion?.Texture,
            [Specular] = mat => mat.Specular?.Texture,
            [Smooth] = mat => mat.Smooth?.Texture,
            [Rough] = mat => mat.Rough?.Texture,
            [Metal] = mat => mat.Metal?.Texture,
            [HCM] = mat => mat.HCM?.Texture,
            [F0] = mat => mat.F0?.Texture,
            [Porosity] = mat => mat.Porosity?.Texture,
            [SubSurfaceScattering] = mat => mat.SSS?.Texture,
            [Emissive] = mat => mat.Emissive?.Texture,
        };
    }
}
