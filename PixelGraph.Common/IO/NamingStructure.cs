using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using System;

namespace PixelGraph.Common.IO
{
    public static class NamingStructure
    {
        public static string GetInputMetaName(MaterialProperties material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            var path = GetPath(material, material.UseGlobalMatching);
            var name = material.UseGlobalMatching ? $"{material.Name}.mcmeta" : "mat.mcmeta";
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
