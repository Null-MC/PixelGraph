using System;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack;

public class PackInputEncoding : PackEncoding, ICloneable
{
    public const bool AutoMaterialDefault = false;

    /// <summary>
    /// Gets or sets the named texture encoding format for reading image data.
    /// </summary>
    /// <remarks>
    /// See <see cref="PixelGraph.Common.TextureFormats.TextureFormat"/>.
    /// </remarks>
    public string Format {get; set;}

    public bool? AutoMaterial {get; set;}

    /// <summary>
    /// Gets or sets the edition of Minecraft the RP will target.
    /// </summary>
    /// <remarks>
    /// See <see cref="PixelGraph.Common.IO.GameEdition"/>.
    /// </remarks>
    [YamlMember(Order = -99)]
    public string Edition {get; set;}


    public override bool HasAnyData()
    {
        if (AutoMaterial.HasValue) return true;
        if (!string.IsNullOrWhiteSpace(Format)) return true;
        if (!string.IsNullOrWhiteSpace(Edition)) return true;
        return base.HasAnyData();
    }
}