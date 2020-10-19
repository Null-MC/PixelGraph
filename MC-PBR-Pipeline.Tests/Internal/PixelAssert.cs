using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace McPbrPipeline.Tests.Internal
{
    internal static class PixelAssert
    {
        public static void RedEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].R);

        public static void GreenEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].G);

        public static void BlueEquals(byte expectedValue, Image<Rgba32> image) =>
            Assert.Equal(expectedValue, image[0, 0].B);
    }
}
