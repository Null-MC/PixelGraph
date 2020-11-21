using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Encoding
{
    public class TextureFormatBase
    {
        protected readonly Dictionary<string, TextureOutputEncoding> Map;


        protected TextureFormatBase()
        {
            Map = new Dictionary<string, TextureOutputEncoding>(StringComparer.InvariantCultureIgnoreCase);
        }

        //protected void Map(string tag, ColorChannel color, string encoding)
        //{
        //    map.GetOrCreate(tag, NewKey).Set(color, encoding);
        //}

        public TextureOutputEncoding Get(string tag)
        {
            return Map.TryGetValue(tag, out var data) ? data : null;
        }

        //private static TextureOutputEncoding NewKey() => new TextureOutputEncoding();
    }
}
