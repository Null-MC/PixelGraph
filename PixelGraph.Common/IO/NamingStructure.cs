using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.IO
{
    public interface INamingStructure
    {
        string GetInputTextureName(PbrProperties texture, string tag);
        string GetInputMetaName(PbrProperties texture, string tag);
        string GetOutputTextureName(PackProperties pack, PbrProperties texture, string tag, bool global);
        string GetOutputMetaName(PackProperties pack, PbrProperties texture, string tag, bool global);
        string Get(string tag, string textureName, string extension, bool global);
    }

    internal abstract class NamingStructureBase : INamingStructure
    {
        protected static readonly Dictionary<string, Func<string, string>> LocalMap;


        static NamingStructureBase()
        {
            LocalMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = ext => $"albedo.{ext}",
                [TextureTags.Height] = ext => $"height.{ext}",
                [TextureTags.Normal] = ext => $"normal.{ext}",
                [TextureTags.Occlusion] = ext => $"occlusion.{ext}",
                [TextureTags.Specular] = ext => $"specular.{ext}",
                [TextureTags.Smooth] = ext => $"smooth.{ext}",
                [TextureTags.Rough] = ext => $"rough.{ext}",
                [TextureTags.Metal] = ext => $"metal.{ext}",
                [TextureTags.Porosity] = ext => $"porosity.{ext}",
                [TextureTags.SubSurfaceScattering] = ext => $"sss.{ext}",
                [TextureTags.Emissive] = ext => $"emissive.{ext}",
            };
        }

        public abstract string Get(string tag, string textureName, string extension, bool global);

        public string GetInputTextureName(PbrProperties texture, string tag)
        {
            return Get(tag, texture.Name, "*", texture.UseGlobalMatching);
        }

        public string GetInputMetaName(PbrProperties texture, string tag)
        {
            var path = GetPath(texture, texture.UseGlobalMatching);
            var file = Get(tag, texture.Name, "mcmeta", texture.UseGlobalMatching);
            return PathEx.Join(path, file);
        }

        public string GetOutputTextureName(PackProperties pack, PbrProperties texture, string tag, bool global)
        {
            var ext = GetExtension(pack);
            return Get(tag, texture.Name, ext, global);
        }

        public string GetOutputMetaName(PackProperties pack, PbrProperties texture, string tag, bool global)
        {
            var path = GetPath(texture, global);
            var ext = GetExtension(pack);
            var file = Get(tag, texture.Name, $"{ext}.mcmeta", global);
            return PathEx.Join(path, file);
        }

        private static string GetPath(PbrProperties texture, bool global)
        {
            return global ? texture.Path : PathEx.Join(texture.Path, texture.Name);
        }

        private static string GetExtension(PackProperties pack)
        {
            if (!supportedExtensions.Contains(pack.OutputEncoding))
                throw new ApplicationException($"Unsupported image encoding '{pack.OutputEncoding}'!");

            return pack.OutputEncoding;
        }

        private static readonly string[] supportedExtensions = {"bmp", "jpg", "jpeg", "gif", "png", "tga"};
    }

    internal class JavaNamingStructure : NamingStructureBase
    {
        private static readonly Dictionary<string, Func<string, string, string>> globalMap;


        static JavaNamingStructure()
        {
            globalMap = new Dictionary<string, Func<string, string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = (name, ext) => $"{name}.{ext}",
                [TextureTags.Height] = (name, ext) => $"{name}_h.{ext}",
                [TextureTags.Normal] = (name, ext) => $"{name}_n.{ext}",
                [TextureTags.Occlusion] = (name, ext) => $"{name}_ao.{ext}",
                [TextureTags.Specular] = (name, ext) => $"{name}_s.{ext}",
                [TextureTags.Smooth] = (name, ext) => $"{name}_smooth.{ext}",
                [TextureTags.Rough] = (name, ext) => $"{name}_rough.{ext}",
                [TextureTags.Metal] = (name, ext) => $"{name}_metal.{ext}",
                [TextureTags.Porosity] = (name, ext) => $"{name}_p.{ext}",
                [TextureTags.SubSurfaceScattering] = (name, ext) => $"{name}_sss.{ext}",
                [TextureTags.Emissive] = (name, ext) => $"{name}_e.{ext}",
            };
        }

        public override string Get(string tag, string textureName, string extension, bool global)
        {
            return global
                ? globalMap[tag](textureName, extension)
                : LocalMap[tag](extension);
        }
    }

    internal class BedrockNamingStructure : NamingStructureBase
    {
        private static readonly Dictionary<string, Func<string, string, string>> globalMap;


        static BedrockNamingStructure()
        {
            globalMap = new Dictionary<string, Func<string, string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = (name, ext) => $"{name}.{ext}",
                [TextureTags.Height] = (name, ext) => $"{name}_h.{ext}",
                [TextureTags.Normal] = (name, ext) => $"{name}_n.{ext}",
                [TextureTags.Occlusion] = (name, ext) => $"{name}_ao.{ext}",
                [TextureTags.Specular] = (name, ext) => $"{name}_s.{ext}",
                [TextureTags.Smooth] = (name, ext) => $"{name}_smooth.{ext}",
                [TextureTags.Rough] = (name, ext) => $"{name}_rough.{ext}",
                [TextureTags.Metal] = (name, ext) => $"{name}_metal.{ext}",
                [TextureTags.Porosity] = (name, ext) => $"{name}_p.{ext}",
                [TextureTags.SubSurfaceScattering] = (name, ext) => $"{name}_sss.{ext}",
                [TextureTags.Emissive] = (name, ext) => $"{name}_e.{ext}",
            };
        }

        public override string Get(string tag, string textureName, string extension, bool global)
        {
            return global
                ? globalMap[tag](textureName, extension)
                : LocalMap[tag](extension);
        }
    }
}
