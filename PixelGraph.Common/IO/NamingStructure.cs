using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.IO
{
    public static class NamingStructure
    {
        private static readonly Dictionary<string, string> LocalMap;
        private static readonly Dictionary<string, string> LocalLookup;
        private static readonly Dictionary<string, Func<string, string>> GlobalMap;
        private static readonly Dictionary<string, string> GlobalLookup;

        //private static readonly Regex ignorePattern = new Regex(@"[^.]ignore[$]", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);


        static NamingStructure()
        {
            LocalMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Opacity] = "opacity",
                [TextureTags.Color] = "color",
                [TextureTags.Height] = "height",
                [TextureTags.Bump] = "bump",
                [TextureTags.Normal] = "normal",
                [TextureTags.Occlusion] = "occlusion",
                [TextureTags.Specular] = "specular",
                [TextureTags.Smooth] = "smooth",
                [TextureTags.Rough] = "rough",
                [TextureTags.Metal] = "metal",
                [TextureTags.F0] = "f0",
                [TextureTags.Porosity] = "porosity",
                [TextureTags.SubSurfaceScattering] = "sss",
                [TextureTags.Emissive] = "emissive",
                [TextureTags.MER] = "mer",
                
                // Internal
                [TextureTags.Item] = "item",
            };

            LocalLookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Opacity] = @"(^|[\s-_.])(opacity|alpha)($|[\s-_.])",
                [TextureTags.Color] = @"(^|[\s-_.])((base)?color|albedo|diffuse)($|[\s-_.])",
                [TextureTags.Height] = @"(^|[\s-_.])height($|[\s-_.])",
                [TextureTags.Bump] = @"(^|[\s-_.])bump($|[\s-_.])",
                [TextureTags.Normal] = @"(^|[\s-_.])normal($|[\s-_.])",
                [TextureTags.Occlusion] = @"(^|[\s-_.])((ambient)?occlusion|ao)($|[\s-_.])",
                [TextureTags.Specular] = @"^specular$",
                [TextureTags.Smooth] = @"(^|[\s-_.])smooth(ness)?($|[\s-_.])",
                [TextureTags.Rough] = @"(^|[\s-_.])rough(ness)?($|[\s-_.])",
                [TextureTags.Metal] = @"(^|[\s-_.])metal(lic|ness)?($|[\s-_.])",
                [TextureTags.F0] = @"(^|[\s-_.])f0($|[\s-_.])",
                [TextureTags.Porosity] = @"(^|[\s-_.])porosity($|[\s-_.])",
                [TextureTags.SubSurfaceScattering] = @"(^|[\s-_.])(sss|scattering)($|[\s-_.])",
                [TextureTags.Emissive] = @"(^|[\s-_.])emissi(ve|on)($|[\s-_.])",
                [TextureTags.MER] = @"^mer$",
                
                // Deprecated/Internal
                [TextureTags.Item] = @"^(inventory|item)$",
            };

            GlobalMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Color] = name => name,
                [TextureTags.Opacity] = name => $"{name}_a",
                [TextureTags.Height] = name => $"{name}_h",
                [TextureTags.Bump] = name => $"{name}_b",
                [TextureTags.Normal] = name => $"{name}_n",
                [TextureTags.Occlusion] = name => $"{name}_ao",
                [TextureTags.Specular] = name => $"{name}_s",
                [TextureTags.Smooth] = name => $"{name}_smooth",
                [TextureTags.Rough] = name => $"{name}_rough",
                [TextureTags.Metal] = name => $"{name}_metal",
                [TextureTags.F0] = name => $"{name}_f0",
                [TextureTags.Porosity] = name => $"{name}_p",
                [TextureTags.SubSurfaceScattering] = name => $"{name}_sss",
                [TextureTags.Emissive] = name => $"{name}_e",
                [TextureTags.MER] = name => $"{name}_mer",

                // Internal
                [TextureTags.Item] = name => $"{name}_item",
            };

            GlobalLookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                [string.Empty] = TextureTags.Color,
                ["o"] = TextureTags.Opacity,
                ["opacity"] = TextureTags.Opacity,
                ["h"] = TextureTags.Height,
                ["height"] = TextureTags.Height,
                ["b"] = TextureTags.Bump,
                ["bump"] = TextureTags.Bump,
                ["n"] = TextureTags.Normal,
                ["normal"] = TextureTags.Normal,
                ["ao"] = TextureTags.Occlusion,
                ["s"] = TextureTags.Specular,
                ["spec"] = TextureTags.Specular,
                ["specular"] = TextureTags.Specular,
                ["sm"] = TextureTags.Smooth,
                ["smooth"] = TextureTags.Smooth,
                ["r"] = TextureTags.Rough,
                ["rough"] = TextureTags.Rough,
                ["m"] = TextureTags.Metal,
                ["metal"] = TextureTags.Metal,
                ["f0"] = TextureTags.F0,
                ["p"] = TextureTags.Porosity,
                ["porosity"] = TextureTags.Porosity,
                ["sss"] = TextureTags.SubSurfaceScattering,
                ["e"] = TextureTags.Emissive,
                ["mer"] = TextureTags.MER,

                // Internal
                ["item"] = TextureTags.Item,

                // Substance
                //["basecolor"] = TextureTags.Color,
                //["ambientocclusion"] = TextureTags.Occlusion,
                //["height"] = TextureTags.Height,
                //["metal"] = TextureTags.Metal,
                //["metallic"] = TextureTags.Metal,
                //["normal"] = TextureTags.Normal,
                //["porosity"] = TextureTags.Porosity,
                //["roughness"] = TextureTags.Rough,
                //["SSS"] = TextureTags.SubSurfaceScattering,
                //["emissive"] = TextureTags.Emissive,

                // Deprecated
                ["a"] = TextureTags.Opacity,
                ["alpha"] = TextureTags.Opacity,
                ["inventory"] = TextureTags.Item,
            };
        }

        public static bool IsLocalFileTag(string localFile, string tag = null)
        {
            var name = Path.GetFileNameWithoutExtension(localFile);
            if (name.EndsWith(".ignore") || name.EndsWith("-x")) return false;
            //return LocalLookup.TryGetValue(name, out var lookupTag) && TextureTags.Is(lookupTag, tag);

            if (tag != null) {
                if (!LocalLookup.TryGetValue(tag, out var pattern)) return false;

                return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
            }

            foreach (var pattern in LocalLookup.Values) {
                if (!Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase)) continue;
                
                return true;
            }

            return false;
        }

        public static bool IsGlobalFileTag(string localFile, string name, string tag)
        {
            var fileName = Path.GetFileNameWithoutExtension(localFile);

            var splitChars = new[] {'.'};
            var fileTags = fileName.Split(splitChars);
            string lookupTag;

            if (fileTags.Length > 1) {
                return fileTags.Any(t => GlobalLookup.TryGetValue(t, out lookupTag) && TextureTags.Is(lookupTag, tag));
            }

            if (!fileName.StartsWith(name)) return false;
            var fileTag = fileName[name.Length..].TrimStart('_');

            return GlobalLookup.TryGetValue(fileTag, out lookupTag) && TextureTags.Is(lookupTag, tag);
        }

        private static string BuildName(string name, string ext)
        {
            var result = name;
            if (ext != null) result += $".{ext}";
            return result;
        }

        public static string Get(string tag, string textureName, string extension, bool global)
        {
            if (global) {
                if (GlobalMap.TryGetValue(tag, out var func)) {
                    var name = func(textureName);
                    return BuildName(name, extension);
                }
            }
            else {
                if (LocalMap.TryGetValue(tag, out var name))
                    return BuildName(name, extension);
            }

            throw new ApplicationException($"Unknown texture tag '{tag}'!");
        }

        public static string GetInputTextureName(MaterialProperties material, string tag)
        {
            return Get(tag, material.Name, "*", material.UseGlobalMatching);
        }

        public static string GetInputMetaName(MaterialProperties material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            var path = GetPath(material, material.UseGlobalMatching);
            var name = material.UseGlobalMatching ? $"{material.Name}.mcmeta" : "mat.mcmeta";
            var filename = PathEx.Join(path, name);
            return PathEx.Localize(filename);
        }

        public static string GetInputMetaName(MaterialProperties material, string tag)
        {
            var path = GetPath(material, material.UseGlobalMatching);
            var name = Get(tag, material.Name, "mcmeta", material.UseGlobalMatching);
            var filename = PathEx.Join(path, name);
            return PathEx.Localize(filename);
        }

        public static string GetInputPropertiesName(MaterialProperties material)
        {
            var path = GetPath(material, material.UseGlobalMatching);
            var name = material.UseGlobalMatching ? $"{material.Name}.properties" : "mat.properties";
            var filename = PathEx.Join(path, name);
            return PathEx.Localize(filename);
        }

        public static string GetOutputPropertiesName(MaterialProperties material, bool global)
        {
            var isLocalCtm = material.CTM?.Method != null && !material.UseGlobalMatching;
            var path = GetPath(material, global && !isLocalCtm);
            var filename = PathEx.Join(path, $"{material.Name}.properties");
            return PathEx.Localize(filename);
        }

        //public static string GetOutputMetaName(MaterialProperties material, bool global)
        //{
        //    var path = GetPath(material, global && material.CTM?.Type == null);
        //    var name = global ? $"{material.Name}.mcmeta" : "mat.mcmeta";
        //    var filename = PathEx.Join(path, name);
        //    return PathEx.Localize(filename);
        //}

        public static string GetOutputMetaName(ResourcePackProfileProperties pack, MaterialProperties material, string tag, bool global)
        {
            return GetOutputMetaName(pack, material, material.Name, tag, global);
        }

        public static string GetOutputMetaName(ResourcePackProfileProperties pack, MaterialProperties material, string mat_name, string tag, bool global)
        {
            var path = GetPath(material, global && material.CTM?.Method == null);
            var ext = GetExtension(pack);
            var name = Get(tag, mat_name, $"{ext}.mcmeta", global);
            var filename = PathEx.Join(path, name);
            return PathEx.Localize(filename);
        }

        public static string GetPath(MaterialProperties material, bool global)
        {
            return global ? material.LocalPath : PathEx.Join(material.LocalPath, material.Name);
        }

        public static string GetExtension(ResourcePackProfileProperties pack)
        {
            var encoding = pack.Encoding.Image ?? ImageExtensions.Default;

            if (!ImageExtensions.Supports(encoding))
                throw new ApplicationException($"Unsupported image encoding '{encoding}'!");

            return encoding;
        }
    }
}
