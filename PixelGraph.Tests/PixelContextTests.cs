using PixelGraph.Common.PixelOperations;
using Xunit;

namespace PixelGraph.Tests
{
    public class PixelContextTests
    {
        private readonly PixelContext context;


        public PixelContextTests()
        {
            context = new PixelContext(128, 128, 0);
        }

        [InlineData(0, 0, 0, 0)]
        [InlineData(24, 12, 24, 12)]
        [InlineData(128, 0, 127, 0)]
        [InlineData(0, 128, 0, 127)]
        [InlineData(256, 1024, 127, 127)]
        [Theory] public void ClampCoordinates(int actualX, int actualY, int expectedX, int expectedY)
        {
            context.Clamp(ref actualX, ref actualY);
            Assert.Equal(expectedX, actualX);
            Assert.Equal(expectedY, actualY);
        }

        [InlineData(0, 0, 0, 0)]
        [InlineData(24, 12, 24, 12)]
        [InlineData(128, 0, 0, 0)]
        [InlineData(0, 128, 0, 0)]
        [InlineData(132, 255, 4, 127)]
        [Theory] public void WrapCoordinates(int actualX, int actualY, int expectedX, int expectedY)
        {
            context.Wrap(ref actualX, ref actualY);
            Assert.Equal(expectedX, actualX);
            Assert.Equal(expectedY, actualY);
        }
    }
}
