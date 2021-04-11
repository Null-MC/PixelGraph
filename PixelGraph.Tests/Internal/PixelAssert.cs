using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace PixelGraph.Tests.Internal
{
    internal static class PixelAssert
    {
        public static void Equals(byte expectedGray, Image<Rgba32> image)
        {
            RedEquals(expectedGray, image);
            GreenEquals(expectedGray, image);
            BlueEquals(expectedGray, image);
        }

        public static void Equals(byte expectedRed, byte expectedGreen, byte expectedBlue, Image<Rgba32> image)
        {
            RedEquals(expectedRed, image);
            GreenEquals(expectedGreen, image);
            BlueEquals(expectedBlue, image);
        }

        public static void Equals(byte expectedRed, byte expectedGreen, byte expectedBlue, byte expectedAlpha, Image<Rgba32> image)
        {
            RedEquals(expectedRed, image);
            GreenEquals(expectedGreen, image);
            BlueEquals(expectedBlue, image);
            AlphaEquals(expectedAlpha, image);
        }

        public static void RedEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].R);

        public static void GreenEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].G);

        public static void BlueEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].B);

        public static void AlphaEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].A);
    }
}
