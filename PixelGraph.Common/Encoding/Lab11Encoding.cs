using PixelGraph.Common.Textures;

namespace PixelGraph.Common.Encoding
{
    internal class Lab11Encoding : TextureFormatBase
    {
        public Lab11Encoding()
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
                Alpha = EncodingChannel.Height,
                Include = true,
            };

            Map[TextureTags.Specular] = new TextureOutputEncoding {
                Red = EncodingChannel.PerceptualSmooth,
                Green = EncodingChannel.Metal,
                Blue = EncodingChannel.Emissive,
                Alpha = EncodingChannel.White,
                Include = true,
            };
        }
    }
}
