using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackOutputProperties
    {
        public string Format {get; set;}

        public TextureOutputEncoding Albedo {get; set;}

        public TextureOutputEncoding Height {get; set;}

        public TextureOutputEncoding Normal {get; set;}

        public TextureOutputEncoding Occlusion {get; set;}

        public TextureOutputEncoding Specular {get; set;}

        public TextureOutputEncoding Smooth {get; set;}

        public TextureOutputEncoding Rough {get; set;}

        public TextureOutputEncoding Metal {get; set;}

        public TextureOutputEncoding Porosity {get; set;}

        public TextureOutputEncoding SSS {get; set;}

        public TextureOutputEncoding Emissive {get; set;}


        //public ResourcePackOutputProperties()
        //{
        //    _albedo = new TextureOutputEncoding();
        //    _height = new TextureOutputEncoding();
        //    _normal = new TextureOutputEncoding();
        //    _occlusion = new TextureOutputEncoding();
        //    _specular = new TextureOutputEncoding();
        //    _smooth = new TextureOutputEncoding();
        //    _rough = new TextureOutputEncoding();
        //    _metal = new TextureOutputEncoding();
        //    _porosity = new TextureOutputEncoding();
        //    _sss = new TextureOutputEncoding();
        //    _emissive = new TextureOutputEncoding();
        //}

        public TextureOutputEncoding GetRawTextureEncoding(string tag)
        {
            return textureMap.TryGetValue(tag, out var encoding) ? encoding(this) : null;
        }

        public TextureOutputEncoding GetFinalTextureEncoding(string tag)
        {
            var format = Format ?? TextureEncoding.Format_Default;
            var defaultEncoding = TextureEncoding.GetDefault(format, tag);

            if (!textureMap.TryGetValue(tag, out var encoding)) return defaultEncoding;

            var rawEncoding = encoding(this);
            if (rawEncoding == null) return defaultEncoding;
            if (defaultEncoding == null) return rawEncoding;

            return new TextureOutputEncoding {
                Red = rawEncoding.Red ?? defaultEncoding.Red,
                Green = rawEncoding.Green ?? defaultEncoding.Green,
                Blue = rawEncoding.Blue ?? defaultEncoding.Blue,
                Alpha = rawEncoding.Alpha ?? defaultEncoding.Alpha,
                Include = rawEncoding.Include ?? defaultEncoding.Include,
                Sampler = rawEncoding.Sampler ?? defaultEncoding.Sampler,
            };
        }

        //public string GetEncodingChannel(string tag, ColorChannel color)
        //{
        //    return GetTextureEncoding(tag)?.GetEncodingChannel(color);
        //}

        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        private static readonly Dictionary<string, Func<ResourcePackOutputProperties, TextureOutputEncoding>> textureMap
            = new Dictionary<string, Func<ResourcePackOutputProperties, TextureOutputEncoding>>(StringComparer.InvariantCultureIgnoreCase) {
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
