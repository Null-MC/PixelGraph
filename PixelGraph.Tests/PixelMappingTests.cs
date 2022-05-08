namespace PixelGraph.Tests
{
    public class PixelMappingTests
    {
        //[InlineData(  0, 0.0f)]
        //[InlineData(127, 0.49803925f)]
        //[InlineData(255, 1.0f)]
        //[Theory] public void UnmapLinearTest(byte pixelValue, float expectedValue)
        //{
        //    var mapping = new TextureChannelMapping {
        //        InputRangeMin = 0,
        //        InputRangeMax = 255,
        //        InputMinValue = 0f,
        //        InputMaxValue = 1f,
        //        InputChannelPower = 1f,
        //    };

        //    var pixelMap = new PixelMapping(mapping);
        //    if (!pixelMap.TryUnmap(in pixelValue, out var channelValue))
        //        throw new ApplicationException("Failed to unmap value!");

        //    Assert.Equal(expectedValue, channelValue);
        //}

        //[InlineData(  0, 0.0f)]
        //[InlineData(127, 0.24804309f)]
        //[InlineData(255, 1.0f)]
        //[Theory] public void UnmapSqrtTest(byte pixelValue, float expectedValue)
        //{
        //    var mapping = new TextureChannelMapping {
        //        InputRangeMin = 0,
        //        InputRangeMax = 255,
        //        InputMinValue = 0f,
        //        InputMaxValue = 1f,
        //        InputChannelPower = 0.5f,
        //    };

        //    var pixelMap = new PixelMapping(mapping);
        //    if (!pixelMap.TryUnmap(in pixelValue, out var channelValue))
        //        throw new ApplicationException("Failed to unmap value!");

        //    Assert.Equal(expectedValue, channelValue);
        //}
    }
}
