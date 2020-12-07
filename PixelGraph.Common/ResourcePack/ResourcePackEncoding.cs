using PixelGraph.Common.Encoding;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackEncoding
    {
        [YamlIgnore]
        public ResourcePackChannelProperties[] All {get;}

        public ResourcePackChannelProperties Alpha {get; set;}

        public ResourcePackChannelProperties DiffuseRed {get; set;}
        public ResourcePackChannelProperties DiffuseGreen {get; set;}
        public ResourcePackChannelProperties DiffuseBlue {get; set;}

        public ResourcePackChannelProperties AlbedoRed {get; set;}
        public ResourcePackChannelProperties AlbedoGreen {get; set;}
        public ResourcePackChannelProperties AlbedoBlue {get; set;}

        public ResourcePackChannelProperties Height {get; set;}
        public ResourcePackChannelProperties Occlusion {get; set;}

        public ResourcePackChannelProperties NormalX {get; set;}
        public ResourcePackChannelProperties NormalY {get; set;}
        public ResourcePackChannelProperties NormalZ {get; set;}

        public ResourcePackChannelProperties Specular {get; set;}

        public ResourcePackChannelProperties Smooth {get; set;}
        public ResourcePackChannelProperties Rough {get; set;}

        public ResourcePackChannelProperties Metal {get; set;}

        public ResourcePackChannelProperties Porosity {get; set;}

        [YamlMember(Alias = "sss")]
        public ResourcePackChannelProperties SSS {get; set;}

        public ResourcePackChannelProperties Emissive {get; set;}


        public ResourcePackEncoding()
        {
            All = new [] {
                Alpha = new ResourcePackChannelProperties(EncodingChannel.Alpha),

                DiffuseRed = new ResourcePackChannelProperties(EncodingChannel.DiffuseRed),
                DiffuseGreen = new ResourcePackChannelProperties(EncodingChannel.DiffuseGreen),
                DiffuseBlue = new ResourcePackChannelProperties(EncodingChannel.DiffuseBlue),

                AlbedoRed = new ResourcePackChannelProperties(EncodingChannel.AlbedoRed),
                AlbedoGreen = new ResourcePackChannelProperties(EncodingChannel.AlbedoGreen),
                AlbedoBlue = new ResourcePackChannelProperties(EncodingChannel.AlbedoBlue),

                Height = new ResourcePackChannelProperties(EncodingChannel.Height),

                Occlusion = new ResourcePackChannelProperties(EncodingChannel.Occlusion),

                NormalX = new ResourcePackChannelProperties(EncodingChannel.NormalX),
                NormalY = new ResourcePackChannelProperties(EncodingChannel.NormalY),
                NormalZ = new ResourcePackChannelProperties(EncodingChannel.NormalZ),

                Specular = new ResourcePackChannelProperties(EncodingChannel.Specular),

                Smooth = new ResourcePackChannelProperties(EncodingChannel.Smooth),
                Rough = new ResourcePackChannelProperties(EncodingChannel.Rough),

                Metal = new ResourcePackChannelProperties(EncodingChannel.Metal),

                Porosity = new ResourcePackChannelProperties(EncodingChannel.Porosity),

                SSS = new ResourcePackChannelProperties(EncodingChannel.SubSurfaceScattering),

                Emissive = new ResourcePackChannelProperties(EncodingChannel.Emissive),
            };
        }

        public IEnumerable<string> GetAllTags()
        {
            return All
                .Select(i => i.Texture)
                .Where(i => i != null).Distinct();
        }

        public IEnumerable<ResourcePackChannelProperties> GetByTag(string tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            return All.Where(i => TextureTags.Is(i.Texture, tag));

            //return textureMap.TryGetValue(tag, out var encoding) ? encoding(this) : null;
        }

        public bool HasAnyChannels(string tag)
        {
            return All.Any(i => TextureTags.Is(i.Texture, tag));
        }

        //public TextureEncoding GetFormatEncoding(string tag)
        //{
        //    var defaultEncoding = TextureEncoding.GetDefault(Format, tag);

        //    if (!textureMap.TryGetValue(tag, out var encoding)) return defaultEncoding;

        //    var rawEncoding = encoding(this);
        //    if (rawEncoding == null) return defaultEncoding;
        //    if (defaultEncoding == null) return rawEncoding;

        //    return new TextureEncoding {
        //        Red = rawEncoding.Red ?? defaultEncoding.Red,
        //        Green = rawEncoding.Green ?? defaultEncoding.Green,
        //        Blue = rawEncoding.Blue ?? defaultEncoding.Blue,
        //        Alpha = rawEncoding.Alpha ?? defaultEncoding.Alpha,
        //    };
        //}
    }
}
