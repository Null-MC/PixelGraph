﻿using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Material;

public class MaterialHeightProperties : IHaveData
{
    public const decimal DefaultEdgeFadeStrength = 1.0m;
    //public const bool DefaultAutoLevel = false;

    public ResourcePackHeightChannelProperties? Input {get; set;}
    public string? Texture {get; set;}
    public decimal? Shift {get; set;}
    public decimal? Scale {get; set;}
    public decimal? EdgeFadeX {get; set;}
    public decimal? EdgeFadeY {get; set;}
    public decimal? EdgeFadeTop {get; set;}
    public decimal? EdgeFadeBottom {get; set;}
    public decimal? EdgeFadeLeft {get; set;}
    public decimal? EdgeFadeRight {get; set;}
    public decimal? EdgeFadeStrength {get; set;}
    public bool? AutoLevel {get; set;}


    public bool HasAnyData()
    {
        if (Input != null && Input.HasAnyData()) return true;
        if (Texture != null) return true;
        if (Shift.HasValue) return true;
        if (Scale.HasValue) return true;
        if (EdgeFadeX.HasValue) return true;
        if (EdgeFadeY.HasValue) return true;
        if (EdgeFadeTop.HasValue) return true;
        if (EdgeFadeBottom.HasValue) return true;
        if (EdgeFadeLeft.HasValue) return true;
        if (EdgeFadeRight.HasValue) return true;
        if (EdgeFadeStrength.HasValue) return true;
        if (AutoLevel.HasValue) return true;
        return false;
    }

    #region Deprecated

    [Obsolete] public decimal? Value {
        get => null;
        set {}
    }

    #endregion
}