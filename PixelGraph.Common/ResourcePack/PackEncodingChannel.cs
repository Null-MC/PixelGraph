using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack;

public abstract class PackEncodingChannel : IHaveData
{
    [YamlIgnore]
    public string ID {get;}
    
    public string? Texture {get; set;}
    public ColorChannel? Color {get; set;}
    public string? Sampler {get; set;}
    public decimal? MinValue {get; set;}
    public decimal? MaxValue {get; set;}
    public byte? RangeMin {get; set;}
    public byte? RangeMax {get; set;}
    public int? Shift {get; set;}
    public decimal? Power {get; set;}
    public bool? Invert {get; set;}
    //public bool? Perceptual {get; set;}
    public decimal? DefaultValue {get; set;}
    public decimal? ClipValue {get; set;}
    public int? Priority {get; set;}

    public bool? EnableClipping {get; set;}

    [YamlIgnore]
    internal string? __Filename {get; set;}

    [YamlIgnore]
    public bool HasTexture => Texture != null && !TextureTags.Is(Texture, TextureTags.None);

    [YamlIgnore]
    public bool HasColor => Color.HasValue && Color.Value != ColorChannel.None;

    [YamlIgnore]
    public bool HasMapping => HasTexture && HasColor;


    protected PackEncodingChannel(string id)
    {
        ID = id;
    }

    protected PackEncodingChannel(string id, string texture, ColorChannel color) : this(id)
    {
        Texture = texture;
        Color = color;
    }

    public void Merge(PackEncodingChannel channel)
    {
        if (channel == null) throw new ArgumentNullException(nameof(channel));

        if (channel.Texture != null) Texture = channel.Texture;
        if (channel.Sampler != null) Sampler = channel.Sampler;
        if (channel.Color.HasValue) Color = channel.Color.Value;
        if (channel.MinValue.HasValue) MinValue = channel.MinValue.Value;
        if (channel.MaxValue.HasValue) MaxValue = channel.MaxValue.Value;
        if (channel.RangeMin.HasValue) RangeMin = channel.RangeMin.Value;
        if (channel.RangeMax.HasValue) RangeMax = channel.RangeMax.Value;
        if (channel.Shift.HasValue) Shift = channel.Shift.Value;
        if (channel.Power.HasValue) Power = channel.Power.Value;
        if (channel.Invert.HasValue) Invert = channel.Invert.Value;
        //if (channel.Perceptual.HasValue) Perceptual = channel.Perceptual.Value;
        //if (channel.ApplyOcclusion.HasValue) ApplyOcclusion = channel.Perceptual.Value;
        if (channel.DefaultValue.HasValue) DefaultValue = channel.DefaultValue.Value;
        if (channel.ClipValue.HasValue) ClipValue = channel.ClipValue.Value;
        if (channel.Priority.HasValue) Priority = channel.Priority.Value;
        if (channel.EnableClipping.HasValue) EnableClipping = channel.EnableClipping.Value;
    }

    public void Reset()
    {
        Texture = null;
        Sampler = null;
        Color = null;
        MinValue = null;
        MaxValue = null;
        RangeMin = null;
        RangeMax = null;
        Shift = null;
        Power = null;
        Invert = null;
        //Perceptual = null;
        //ApplyOcclusion = null;
        DefaultValue = null;
        ClipValue = null;
        Priority = null;
        EnableClipping = null;
    }

    public virtual bool HasAnyData()
    {
        if (Texture != null) return true;
        if (Sampler != null) return true;
        if (Color.HasValue) return true;
        if (MinValue.HasValue) return true;
        if (MaxValue.HasValue) return true;
        if (RangeMin.HasValue) return true;
        if (RangeMax.HasValue) return true;
        if (Shift.HasValue) return true;
        if (Power.HasValue) return true;
        if (Invert.HasValue) return true;
        //if (Perceptual.HasValue) return true;
        //if (ApplyOcclusion.HasValue) return true;
        if (DefaultValue.HasValue) return true;
        if (ClipValue.HasValue) return true;
        if (Priority.HasValue) return true;
        if (EnableClipping.HasValue) return true;
        return false;
    }

    public virtual object Clone()
    {
        return MemberwiseClone();
    }

    public override string ToString()
    {
        // TODO: use StringBuilder and show all options
        return $"{{{ID} {Texture}:{Color}}}";
    }
}