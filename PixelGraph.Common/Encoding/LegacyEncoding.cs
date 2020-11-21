using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class LegacyEncoding : TextureFormatBase
    {
        public LegacyEncoding()
        {
            Map[TextureTags.Albedo] = new TextureOutputEncoding {
                Red = EncodingChannel.Red,
                Green = EncodingChannel.Green,
                Blue = EncodingChannel.Blue,
                Alpha = EncodingChannel.Alpha,
                Include = true,
            };

            Map[TextureTags.Normal] = new TextureOutputEncoding {
                Red = EncodingChannel.NormalX,
                Green = EncodingChannel.NormalY,
                Blue = EncodingChannel.NormalZ,
                Alpha = EncodingChannel.White,
                Include = true,
            };

            Map[TextureTags.Specular] = new TextureOutputEncoding {
                Red = EncodingChannel.Specular,
                Green = EncodingChannel.Specular,
                Blue = EncodingChannel.Specular,
                Alpha = EncodingChannel.White,
                Include = true,
            };
        }
    }
}
