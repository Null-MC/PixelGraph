using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelGraph.Common.Textures;

public enum NormalMapMethods
{
    [EnumMember(Value = "sobel3")]
    Sobel3,

    [EnumMember(Value = "sobel5")]
    Sobel5,

    [EnumMember(Value = "sobel9")]
    Sobel9,

    [EnumMember(Value = "sobel-high")]
    SobelHigh,

    [EnumMember(Value = "sobel-low")]
    SobelLow,

    [EnumMember(Value = "variance")]
    Variance,
}

public static class NormalMapMethod
{
    public const string Sobel3 = "sobel3";
    public const string Sobel5 = "sobel5";
    public const string Sobel9 = "sobel9";
    public const string SobelHigh = "sobel-high";
    public const string SobelLow = "sobel-low";
    public const string Variance = "variance";

    private static readonly Dictionary<string, NormalMapMethods> parseMap = new(StringComparer.InvariantCultureIgnoreCase) {
        [Sobel3] = NormalMapMethods.Sobel3,
        [Sobel5] = NormalMapMethods.Sobel5,
        [Sobel9] = NormalMapMethods.Sobel9,
        [SobelHigh] = NormalMapMethods.SobelHigh,
        [SobelLow] = NormalMapMethods.SobelLow,
        [Variance] = NormalMapMethods.Variance,
    };

    private static readonly Dictionary<NormalMapMethods, string> map = new() {
        [NormalMapMethods.Sobel3] = Sobel3,
        [NormalMapMethods.Sobel5] = Sobel5,
        [NormalMapMethods.Sobel9] = Sobel9,
        [NormalMapMethods.SobelHigh] = SobelHigh,
        [NormalMapMethods.SobelLow] = SobelLow,
        [NormalMapMethods.Variance] = Variance,
    };


    //public static bool TryGetValue(NormalMapMethods method, out string value) =>
    //    map.TryGetValue(method, out value);

    public static bool TryParse(string value, out NormalMapMethods method)
    {
        if (value == null) {
            method = NormalMapMethods.Sobel3;
            return false;
        }

        return parseMap.TryGetValue(value, out method);
    }
}