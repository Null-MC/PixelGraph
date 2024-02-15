using ImageExtensions = PixelGraph.Common.IO.ImageExtensions;

namespace PixelGraph.Common.ResourcePack;

public class PackOutputEncoding : PackEncoding
{
    public const string ImageDefault = ImageExtensions.Png;
    public const int DefaultPaletteColors = 256;

    public string? Image {get; set;}

    /// <summary>
    /// Gets or sets the named texture encoding format for compiling image data.
    /// </summary>
    /// <remarks>
    /// See <see cref="TextureFormats.TextureFormat"/>.
    /// </remarks>
    public string? Format {get; set;}

    public string? Sampler {get; set;}

    public bool? EnablePalette {get; set;}
    public int? PaletteColors {get; set;}


    public string? GetSampler(string encodingChannel)
    {
        if (channelMap.TryGetValue(encodingChannel, out var channelFunc)) {
            var samplerName = channelFunc(this).Sampler;
            if (samplerName != null) return samplerName;
        }

        return Sampler;
    }

    public override bool HasAnyData()
    {
        if (!string.IsNullOrWhiteSpace(Image)) return true;
        if (!string.IsNullOrWhiteSpace(Format)) return true;
        if (!string.IsNullOrWhiteSpace(Sampler)) return true;
        if (EnablePalette.HasValue) return true;
        if (PaletteColors.HasValue) return true;
        return base.HasAnyData();
    }
}