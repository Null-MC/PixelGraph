using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO
{
    internal class JavaNamingStructure : NamingStructureBase
    {
        private static readonly Dictionary<string, Func<string, string, string>> globalMap;


        static JavaNamingStructure()
        {
            globalMap = new Dictionary<string, Func<string, string, string>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Alpha] = (name, ext) => $"{name}_a.{ext}",
                [TextureTags.Albedo] = (name, ext) => $"{name}.{ext}",
                [TextureTags.Diffuse] = (name, ext) => $"{name}_d.{ext}",
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
            if (global) {
                if (globalMap.TryGetValue(tag, out var func))
                    return func(textureName, extension);
            }
            else {
                if (LocalMap.TryGetValue(tag, out var func2))
                    return func2(extension);
            }

            throw new ApplicationException($"Unknown texture tag '{tag}'!");
        }
    }
}
