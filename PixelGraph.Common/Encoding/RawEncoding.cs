using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class RawEncoding : TextureFormatBase
    {
        public RawEncoding()
        {
            Map[TextureTags.Albedo] = new TextureOutputEncoding {
                Red = EncodingChannel.Red,
                Green = EncodingChannel.Green,
                Blue = EncodingChannel.Blue,
                Alpha = EncodingChannel.Alpha,
                Include = true,
            };

            Map[TextureTags.Height] = new TextureOutputEncoding {
                Red = EncodingChannel.Height,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Normal] = new TextureOutputEncoding {
                Red = EncodingChannel.NormalX,
                Green = EncodingChannel.NormalY,
                Blue = EncodingChannel.NormalZ,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Occlusion] = new TextureOutputEncoding {
                Red = EncodingChannel.Occlusion,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Specular] = new TextureOutputEncoding {
                Red = EncodingChannel.Specular,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Smooth] = new TextureOutputEncoding {
                Red = EncodingChannel.Smooth,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Rough] = new TextureOutputEncoding {
                Red = EncodingChannel.Rough,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Metal] = new TextureOutputEncoding {
                Red = EncodingChannel.Metal,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Porosity] = new TextureOutputEncoding {
                Red = EncodingChannel.Porosity,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.SubSurfaceScattering] = new TextureOutputEncoding {
                Red = EncodingChannel.SubSurfaceScattering,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Emissive] = new TextureOutputEncoding {
                Red = EncodingChannel.Emissive,
                Alpha = EncodingChannel.White,
                Include = true,
            };
        }
    }
}
