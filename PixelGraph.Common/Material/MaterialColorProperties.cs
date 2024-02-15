using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using System.Globalization;

namespace PixelGraph.Common.Material;

public class MaterialColorProperties : IHaveData
{
    public const bool DefaultBakeOcclusion = false;

    public string? Texture {get; set;}
    public string? Value {get; set;}
    public bool? BakeOcclusion {get; set;}


    public ResourcePackColorRedChannelProperties? InputRed {get; set;}
    public decimal? ScaleRed {get; set;}

    public ResourcePackColorGreenChannelProperties? InputGreen {get; set;}
    public decimal? ScaleGreen {get; set;}

    public ResourcePackColorBlueChannelProperties? InputBlue {get; set;}
    public decimal? ScaleBlue {get; set;}


    public decimal? GetValueRed() => ParseHexRange(Value, 0);
    public decimal? GetValueGreen() => ParseHexRange(Value, 2);
    public decimal? GetValueBlue() => ParseHexRange(Value, 4);

    public bool HasAnyData()
    {
        if (Texture != null) return true;
        if (Value != null) return true;
        if (BakeOcclusion.HasValue) return true;

        if (InputRed != null && InputRed.HasAnyData()) return true;
        if (InputGreen != null && InputGreen.HasAnyData()) return true;
        if (InputBlue != null && InputBlue.HasAnyData()) return true;

        if (ScaleRed.HasValue) return true;
        if (ScaleGreen.HasValue) return true;
        if (ScaleBlue.HasValue) return true;
            
        return false;
    }

    private static decimal? ParseHexRange(string? hex, int start)
    {
        var trimValue = hex?.Trim('#', ' ');
        if (string.IsNullOrEmpty(trimValue) || trimValue.Length != 6) return null;
        var byteValue = byte.Parse(trimValue.Substring(start, 2), NumberStyles.HexNumber);
        return decimal.Round(new decimal(byteValue / 255f), 3);
    }

    #region Deprecated

    internal decimal? __ValueRed;
    [Obsolete("Replace usages of ValueRed with Value")]
    public decimal? ValueRed {
        get => null;
        set => __ValueRed = value;
    }

    internal decimal? __ValueGreen;
    [Obsolete("Replace usages of ValueGreen with Value")]
    public decimal? ValueGreen {
        get => null;
        set => __ValueGreen = value;
    }

    internal decimal? __ValueBlue;
    [Obsolete("Replace usages of ValueBlue with Value")]
    public decimal? ValueBlue {
        get => null;
        set => __ValueBlue = value;
    }

    internal string? __PreviewTint;
    [Obsolete("Replace usages of mat.Color.PreviewTint with mat.ColorTint")]
    public string? PreviewTint {
        get => null;
        set => __PreviewTint = value;
    }

    #endregion
}