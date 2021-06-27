using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO
{
    public static class NamingStructure
    {
        private static readonly Dictionary<string, Func<string>> LocalMap;
        private static readonly Dictionary<string, Func<string, string>> GlobalMap;


        static NamingStructure()
        {
            LocalMap = new Dictionary<string, Func<string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Alpha] = () => "alpha",
                [TextureTags.Albedo] = () => "albedo",
                [TextureTags.Diffuse] = () => "diffuse",
                [TextureTags.Height] = () => "height",
                [TextureTags.Bump] = () => "bump",
                [TextureTags.Normal] = () => "normal",
                [TextureTags.Occlusion] = () => "occlusion",
                [TextureTags.Specular] = () => "specular",
                [TextureTags.Smooth] = () => "smooth",
                [TextureTags.Rough] = () => "rough",
                [TextureTags.Metal] = () => "metal",
                [TextureTags.F0] = () => "f0",
                [TextureTags.Porosity] = () => "porosity",
                [TextureTags.SubSurfaceScattering] = () => "sss",
                [TextureTags.Emissive] = () => "emissive",
                [TextureTags.MER] = () => "mer",
                
                // Internal
                [TextureTags.Inventory] = () => "inventory",
            };

            GlobalMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = name => name,
                [TextureTags.Alpha] = name => $"{name}_a",
                [TextureTags.Diffuse] = name => $"{name}_d",
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
                [TextureTags.Inventory] = name => $"{name}_inventory",
            };
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
                if (LocalMap.TryGetValue(tag, out var func)) {
                    var name = func();
                    return BuildName(name, extension);
                }
            }

            throw new ApplicationException($"Unknown texture tag '{tag}'!");
        }

        public static string GetInputTextureName(MaterialProperties material, string tag)
        {
            return Get(tag, material.Name, "*", material.UseGlobalMatching);
        }

        public static string GetInputMetaName(MaterialProperties material)
        {
            var path = GetPath(material, material.UseGlobalMatching);
            var name = material.UseGlobalMatching ? $"{material.Name}.mcmeta" : "mat.mcmeta";
            return PathEx.Join(path, name);
        }

        public static string GetInputMetaName(MaterialProperties material, string tag)
        {
            var path = GetPath(material, material.UseGlobalMatching);
            var file = Get(tag, material.Name, "mcmeta", material.UseGlobalMatching);
            return PathEx.Join(path, file);
        }

        public static string GetInputPropertiesName(MaterialProperties material)
        {
            var path = GetPath(material, material.UseGlobalMatching);
            var name = material.UseGlobalMatching ? $"{material.Name}.properties" : "mat.properties";
            return PathEx.Join(path, name);
        }

        public static string GetOutputPropertiesName(MaterialProperties material, bool global)
        {
            var isLocalCtm = material.CTM?.Type != null && !material.UseGlobalMatching;
            var path = GetPath(material, global && !isLocalCtm);
            return PathEx.Join(path, $"{material.Name}.properties");
        }

        public static string GetOutputMetaName(ResourcePackProfileProperties pack, MaterialProperties material, string tag, bool global)
        {
            var path = GetPath(material, global && material.CTM?.Type == null);
            var ext = GetExtension(pack);
            var file = Get(tag, material.Name, $"{ext}.mcmeta", global);
            return PathEx.Join(path, file);
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
