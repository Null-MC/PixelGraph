using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO
{
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
