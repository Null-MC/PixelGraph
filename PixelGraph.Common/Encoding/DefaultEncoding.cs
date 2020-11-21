using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class DefaultEncoding : TextureFormatBase
    {
        public DefaultEncoding()
        {
            Map[TextureTags.Albedo] = new TextureOutputEncoding {
                Red = EncodingChannel.Red,
                Green = EncodingChannel.Green,
                Blue = EncodingChannel.Blue,
                Alpha = EncodingChannel.Alpha,
                Include = true,
            };
        }
    }
}
