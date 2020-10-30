using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Output
{
    internal interface INamingStructure
    {
        string GetInputTextureName(string tag, string textureName, bool global);
        string GetOutputTextureName(string tag, string textureName, bool global);
        string GetInputMetaName(string tag, PbrProperties texture);
        string GetOutputMetaName(string tag, PbrProperties texture, bool global);
        string Get(string tag, string textureName, string extension, bool global);
    }

    internal abstract class NamingStructureBase : INamingStructure
    {
        protected static readonly Dictionary<string, Func<string, string>> localMap;


        static NamingStructureBase()
        {
            localMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
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

        public string GetInputTextureName(string tag, string textureName, bool global)
        {
            return Get(tag, textureName, "*", global);
        }

        public string GetOutputTextureName(string tag, string textureName, bool global)
        {
            return Get(tag, textureName, "png", global);
        }

        public string GetInputMetaName(string tag, PbrProperties texture)
        {
            var path = GetPath(texture, texture.UseGlobalMatching);
            var file = Get(tag, texture.Name, "mcmeta", texture.UseGlobalMatching);
            return PathEx.Join(path, file);
        }

        public string GetOutputMetaName(string tag, PbrProperties texture, bool global)
        {
            var path = GetPath(texture, global);
            var file = Get(tag, texture.Name, "png.mcmeta", global);
            return PathEx.Join(path, file);
        }

        private static string GetPath(PbrProperties texture, bool global)
        {
            return global ? texture.Path : PathEx.Join(texture.Path, texture.Name);
        }
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
                : localMap[tag](extension);
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
                : localMap[tag](extension);
        }
    }
}
