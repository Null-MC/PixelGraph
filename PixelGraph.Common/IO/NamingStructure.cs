using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO
{
    public interface INamingStructure
    {
        string GetInputTextureName(MaterialProperties texture, string tag);
        string GetInputMetaName(MaterialProperties texture, string tag);
        string GetOutputTextureName(ResourcePackProfileProperties pack, string name, string tag, bool global);
        string GetOutputMetaName(ResourcePackProfileProperties pack, MaterialProperties texture, string tag, bool global);
        string Get(string tag, string textureName, string extension, bool global);
    }

    internal abstract class NamingStructureBase : INamingStructure
    {
        protected static readonly Dictionary<string, Func<string, string>> LocalMap;


        static NamingStructureBase()
        {
            LocalMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Alpha] = ext => $"alpha.{ext}",
                [TextureTags.Albedo] = ext => $"albedo.{ext}",
                [TextureTags.Diffuse] = ext => $"diffuse.{ext}",
                [TextureTags.Height] = ext => $"height.{ext}",
                [TextureTags.Normal] = ext => $"normal.{ext}",
                [TextureTags.Occlusion] = ext => $"occlusion.{ext}",
                [TextureTags.Specular] = ext => $"specular.{ext}",
                [TextureTags.Smooth] = ext => $"smooth.{ext}",
                [TextureTags.Rough] = ext => $"rough.{ext}",
                [TextureTags.Metal] = ext => $"metal.{ext}",
                [TextureTags.F0] = ext => $"f0.{ext}",
                [TextureTags.Porosity] = ext => $"porosity.{ext}",
                [TextureTags.SubSurfaceScattering] = ext => $"sss.{ext}",
                [TextureTags.Emissive] = ext => $"emissive.{ext}",
                
                // Internal
                [TextureTags.Inventory] = ext => $"inventory.{ext}",
            };
        }

        public abstract string Get(string tag, string textureName, string extension, bool global);

        public string GetInputTextureName(MaterialProperties material, string tag)
        {
            return Get(tag, material.Name, "*", material.UseGlobalMatching);
        }

        public string GetInputMetaName(MaterialProperties material, string tag)
        {
            var path = GetPath(material, material.UseGlobalMatching);
            var file = Get(tag, material.Name, "mcmeta", material.UseGlobalMatching);
            return PathEx.Join(path, file);
        }

        public string GetOutputTextureName(ResourcePackProfileProperties pack, string name, string tag, bool global)
        {
            var ext = GetExtension(pack);
            return Get(tag, name, ext, global);
        }

        public string GetOutputMetaName(ResourcePackProfileProperties pack, MaterialProperties material, string tag, bool global)
        {
            var path = GetPath(material, global);
            var ext = GetExtension(pack);
            var file = Get(tag, material.Name, $"{ext}.mcmeta", global);
            return PathEx.Join(path, file);
        }

        //public TextureTypes GetTextureType(string filename)
        //{
        //    var path = System.IO.Path.GetDirectoryName(filename);
        //    if (PathEx.ContainsSegment(path, "textures", "block"))
        //        return TextureTypes.Block;

        //    return TextureTypes.Unknown;
        //}

        private static string GetPath(MaterialProperties material, bool global)
        {
            return global ? material.LocalPath : PathEx.Join(material.LocalPath, material.Name);
        }

        private static string GetExtension(ResourcePackProfileProperties pack)
        {
            var encoding = pack.Encoding.Image ?? ImageExtensions.Default;

            if (!ImageExtensions.Supports(encoding))
                throw new ApplicationException($"Unsupported image encoding '{encoding}'!");

            return encoding;
        }
    }
}
