using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackInputProperties
    {
        public string Format {get; set;}

        public TextureEncoding Albedo {get; set;}

        public TextureEncoding Height {get; set;}

        public TextureEncoding Normal {get; set;}

        public TextureEncoding Occlusion {get; set;}

        public TextureEncoding Specular {get; set;}

        public TextureEncoding Smooth {get; set;}

        public TextureEncoding Rough {get; set;}

        public TextureEncoding Metal {get; set;}

        public TextureEncoding Porosity {get; set;}

        public TextureEncoding SSS {get; set;}

        public TextureEncoding Emissive {get; set;}


        public ResourcePackInputProperties()
        {
            Albedo = new TextureEncoding();
            Height = new TextureEncoding();
            Normal = new TextureEncoding();
            Occlusion = new TextureEncoding();
            Specular = new TextureEncoding();
            Smooth = new TextureEncoding();
            Rough = new TextureEncoding();
            Metal = new TextureEncoding();
            Porosity = new TextureEncoding();
            SSS = new TextureEncoding();
            Emissive = new TextureEncoding();
        }

        public TextureEncoding GetRawEncoding(string tag)
        {
            return textureMap.TryGetValue(tag, out var encoding) ? encoding(this) : null;
        }

        public TextureEncoding GetFormatEncoding(string tag)
        {
            var defaultEncoding = TextureEncoding.GetDefault(Format, tag);

            if (!textureMap.TryGetValue(tag, out var encoding)) return defaultEncoding;

            var rawEncoding = encoding(this);
            if (rawEncoding == null) return defaultEncoding;
            if (defaultEncoding == null) return rawEncoding;

            return new TextureEncoding {
                Red = rawEncoding.Red ?? defaultEncoding.Red,
                Green = rawEncoding.Green ?? defaultEncoding.Green,
                Blue = rawEncoding.Blue ?? defaultEncoding.Blue,
                Alpha = rawEncoding.Alpha ?? defaultEncoding.Alpha,
            };
        }

        private static readonly Dictionary<string, Func<ResourcePackInputProperties, TextureEncoding>> textureMap
            = new Dictionary<string, Func<ResourcePackInputProperties, TextureEncoding>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Albedo] = o => o.Albedo,
                [TextureTags.Height] = o => o.Height,
                [TextureTags.Normal] = o => o.Normal,
                [TextureTags.Occlusion] = o => o.Occlusion,
                [TextureTags.Specular] = o => o.Specular,
                [TextureTags.Smooth] = o => o.Smooth,
                [TextureTags.Rough] = o => o.Rough,
                [TextureTags.Metal] = o => o.Metal,
                [TextureTags.Porosity] = o => o.Porosity,
                [TextureTags.SubSurfaceScattering] = o => o.SSS,
                [TextureTags.Emissive] = o => o.Emissive,
            };
    }
}
