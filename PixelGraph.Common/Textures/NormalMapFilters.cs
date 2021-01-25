using System.Runtime.Serialization;

namespace PixelGraph.Common.Textures
{
    public enum NormalMapFilters
    {
        [EnumMember(Value = "sobel3")]
        Sobel3,

        [EnumMember(Value = "sobel-high")]
        SobelHigh,

        [EnumMember(Value = "sobel-low")]
        SobelLow,

        [EnumMember(Value = "variance")]
        Variance,
    }
}
