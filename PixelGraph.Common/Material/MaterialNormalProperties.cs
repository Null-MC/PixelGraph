﻿using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.Material;

public class MaterialNormalProperties : IHaveData
{
    public const decimal DefaultStrength = 1.0m;

    public string? Texture {get; set;}
    public decimal? Strength {get; set;}
    public string? Method {get; set;}

    public decimal? Noise { get; set; }
    public decimal? CurveX { get; set; }
    public decimal? CurveY { get; set; }
    public decimal? RadiusX { get; set; }
    public decimal? RadiusY { get; set; }
        
    public ResourcePackNormalXChannelProperties? InputX {get; set;}

    [YamlMember(Alias = "value-x", ApplyNamingConventions = false)]
    public decimal? ValueX {get; set;}

    public ResourcePackNormalYChannelProperties? InputY {get; set;}

    [YamlMember(Alias = "value-y", ApplyNamingConventions = false)]
    public decimal? ValueY {get; set;}

    public ResourcePackNormalZChannelProperties? InputZ {get; set;}

    [YamlMember(Alias = "value-z", ApplyNamingConventions = false)]
    public decimal? ValueZ {get; set;}


    public bool HasAnyData()
    {
        if (InputX?.HasAnyData() ?? false) return true;
        if (InputY?.HasAnyData() ?? false) return true;
        if (InputZ?.HasAnyData() ?? false) return true;

        if (Texture != null) return true;
        if (Strength.HasValue) return true;
        if (Method != null) return true;

        if (ValueX.HasValue) return true;
        if (ValueY.HasValue) return true;
        if (ValueZ.HasValue) return true;

        if (Noise.HasValue) return true;
        if (CurveX.HasValue) return true;
        if (CurveY.HasValue) return true;
        if (RadiusX.HasValue) return true;
        if (RadiusY.HasValue) return true;
            
        return false;
    }


    #region Deprecated

    [Obsolete("Rename usages of Filter to Method.")]
    public string? Filter {
        get => null;
        set => Method = value;
    }

    #endregion
}