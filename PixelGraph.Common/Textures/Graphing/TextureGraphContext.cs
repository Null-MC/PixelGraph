using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.Textures.Graphing;

public interface ITextureGraphContext
{
    MaterialProperties? Material {get; set;}
    IProjectDescription? Project {get; set;}
    PublishProfileProperties? Profile {get; set;}
    List<PackEncodingChannel> InputEncoding {get; set;}
    List<PackEncodingChannel> OutputEncoding {get; set;}
    IPublisherMapping? Mapping {get; set;}

    DateTime PackWriteTime {get; set;}
    bool IsAnimated {get; set;}
    int MaxFrameCount {get; set;}
    bool PublishAsGlobal {get; set;}
    bool ApplyPostProcessing {get; set;}
    bool IsImport {get; set;}
    bool MaterialWrapX {get;}
    bool MaterialWrapY {get;}
    bool IsMaterialMultiPart {get;}
    bool IsMaterialCtm {get;}
    float? TextureScale {get;}
    string DefaultSampler {get;}
    bool AutoGenerateOcclusion {get;}
    bool BakeOcclusionToColor {get;}
    bool EnablePalette {get;}
    int PaletteColors {get;}

    void ApplyInputEncoding();
    void ApplyOutputEncoding();
    ISampler<T> CreateSampler<T>(Image<T> image, string name) where T : unmanaged, IPixel<T>;
    float? GetExpectedAspect();
    Size? GetMaterialSize(float? defaultAspect = null);
    Size? GetTextureSize(float? defaultAspect);
    Size? GetBufferSize(float aspect);
    string GetSamplerName(string channelName);
    MaterialType GetFinalMaterialType();
}

internal class TextureGraphContext : ITextureGraphContext
{
    private static readonly Regex blockTextureExp = new(@"(?:^|\/)textures\/block(?:\/|$)", RegexOptions.Compiled);
    private static readonly Regex itemTextureExp = new(@"(?:^|\/)textures\/item(?:\/|$)", RegexOptions.Compiled);
    private static readonly Regex ctmTextureExp = new(@"(?:^|\/)optifine\/ctm(?:\/|$)", RegexOptions.Compiled);
    private static readonly Regex citTextureExp = new(@"(?:^|\/)optifine\/cit(?:\/|$)", RegexOptions.Compiled);
    private static readonly Regex entityTextureExp = new(@"(?:^|\/)textures\/entity(?:\/|$)", RegexOptions.Compiled);

    public MaterialProperties? Material {get; set;}
    public IProjectDescription? Project {get; set;}
    public PublishProfileProperties? Profile {get; set;}
    public List<PackEncodingChannel> InputEncoding {get; set;} = [];
    public List<PackEncodingChannel> OutputEncoding {get; set;} = [];
    public IPublisherMapping? Mapping {get; set;}

    public DateTime PackWriteTime {get; set;}
    public bool IsAnimated {get; set;}
    public int MaxFrameCount {get; set;} = 1;
    public bool PublishAsGlobal {get; set;} = true;
    public bool ApplyPostProcessing {get; set;} = true;
    public bool IsImport {get; set;}

    public bool MaterialWrapX => Material?.WrapX ?? MaterialProperties.DefaultWrap;
    public bool MaterialWrapY => Material?.WrapY ?? MaterialProperties.DefaultWrap;
    public bool IsMaterialMultiPart => Material?.Parts?.Any() ?? false;
    public bool IsMaterialCtm => !string.IsNullOrWhiteSpace(Material?.CTM?.Method);

    public float? TextureScale => (float?)Profile?.TextureScale;
    public string DefaultSampler => Profile?.Encoding?.Sampler ?? Samplers.Samplers.Nearest;

    public bool AutoGenerateOcclusion => Profile?.AutoGenerateOcclusion ?? PublishProfileProperties.AutoGenerateOcclusionDefault;

    public bool BakeOcclusionToColor {
        get {
            var matVal = Material?.Color.BakeOcclusion;
            var profileVal = Profile?.BakeOcclusionToColor;

            if (!matVal.HasValue && !profileVal.HasValue)
                return PublishProfileProperties.BakeOcclusionToColorDefault;

            return (matVal ?? false) || (profileVal ?? false);
        }
    }

    public bool EnablePalette => Profile?.Encoding?.EnablePalette ?? false;
    public int PaletteColors => Profile?.Encoding?.PaletteColors ?? PackOutputEncoding.DefaultPaletteColors;


    public void ApplyInputEncoding()
    {
        var inputEncoding = BuildEncoding(Project?.Input?.Format);
        if (Project?.Input != null) inputEncoding.Merge(Project.Input);
        if (Material != null) inputEncoding.Merge(Material);

        var outputEncoding = BuildEncoding(Profile?.Encoding?.Format);
        if (Profile?.Encoding != null) outputEncoding.Merge(Profile.Encoding);

        InputEncoding = inputEncoding.GetMapped().ToList();
        OutputEncoding = outputEncoding.GetMapped().ToList();
    }

    public void ApplyOutputEncoding()
    {
        var inputEncoding = BuildEncoding(Profile?.Encoding?.Format);
        if (Profile?.Encoding != null) inputEncoding.Merge(Profile.Encoding);

        var outputEncoding = BuildEncoding(Project?.Input?.Format);
        if (Project?.Input != null) outputEncoding.Merge(Project.Input);
        // TODO: layer material properties on top of pack encoding?

        InputEncoding = inputEncoding.GetMapped().ToList();
        if (InputEncoding.Count == 0) throw new ApplicationException("Input encoding is empty!");

        OutputEncoding = outputEncoding.GetMapped().ToList();
        if (OutputEncoding.Count == 0) throw new ApplicationException("Output encoding is empty!");
    }

    public string GetSamplerName(string channelName)
    {
        return Profile?.Encoding?.GetSampler(channelName)
            ?? Material?.GetInput(channelName)?.Sampler
            ?? DefaultSampler;
    }

    public ISampler<T> CreateSampler<T>(Image<T> image, string name) where T : unmanaged, IPixel<T>
    {
        var sampler = Sampler<T>.Create(name);
        sampler.Image = image;
        sampler.WrapX = MaterialWrapX;
        sampler.WrapY = MaterialWrapY;

        return sampler;
    }

    public MaterialType GetFinalMaterialType()
    {
        ArgumentNullException.ThrowIfNull(Material);

        var type = Material.GetMaterialType();

        if (type != MaterialType.Automatic || string.IsNullOrWhiteSpace(Material.LocalPath))
            return type;

        var path = PathEx.Normalize(Material.LocalPath);
        //if (path == null) return type;

        if (blockTextureExp.IsMatch(path)) return MaterialType.Block;
        if (itemTextureExp.IsMatch(path)) return MaterialType.Item;
        if (ctmTextureExp.IsMatch(path)) return MaterialType.Block;
        if (citTextureExp.IsMatch(path)) return MaterialType.Item;
        if (entityTextureExp.IsMatch(path)) return MaterialType.Entity;

        return type;
    }

    public float? GetExpectedAspect()
    {
        if (Material == null || !IsMaterialMultiPart) return null;

        var (width, height) = Material.GetMultiPartBounds();
        return width / (float)height;
    }

    public Size? GetMaterialSize(float? defaultAspect = null)
    {
        if (Material == null) return null;

        if (Material.TextureWidth.HasValue) {
            if (Material.TextureHeight.HasValue) {
                var width = Material.TextureWidth.Value;
                var height = Material.TextureHeight.Value;

                if (Profile != null && Profile.TextureScale.HasValue) {
                    var scale = (float)Profile.TextureScale.Value;
                    width = (int)MathF.Ceiling(width * scale);
                    height = (int)MathF.Ceiling(height * scale);
                }

                return new Size(width, height);
            }

            if (defaultAspect.HasValue) {
                // TODO: return width, width*aspect
                var width = Material.TextureWidth.Value;
                var height = (int)(width * defaultAspect.Value);

                if (Profile != null && Profile.TextureScale.HasValue) {
                    var scale = (float)Profile.TextureScale.Value;
                    width = (int)MathF.Ceiling(width * scale);
                    height = (int)MathF.Ceiling(height * scale);
                }

                return new Size(width, height);
            }
        }
        else if (Material.TextureHeight.HasValue) {
            if (defaultAspect.HasValue) {
                // TODO: return height/aspect, height
            }
        }

        if (Material.TextureSize.HasValue) {
            var aspect = defaultAspect ?? 1f;
            var width = Material.TextureSize.Value;
            var height = (int)MathF.Ceiling(width * aspect);

            if (Profile?.TextureScale.HasValue ?? false) {
                var scale = (float)Profile.TextureScale.Value;
                width = (int)MathF.Ceiling(width * scale);
                height = (int)MathF.Ceiling(height * scale);
            }

            return new Size(width, height);
        }

        return null;
    }

    public Size? GetTextureSize(float? defaultAspect)
    {
        //if (Material.TextureWidth.HasValue) {
        //    if (Material.TextureHeight.HasValue) {
        //        var width = Material.TextureWidth.Value;
        //        var height = Material.TextureHeight.Value;

        //        if (Profile != null && Profile.TextureScale.HasValue) {
        //            var scale = (float)Profile.TextureScale.Value;
        //            width = (int)MathF.Ceiling(width * scale);
        //            height = (int)MathF.Ceiling(height * scale);
        //        }

        //        return new Size(width, height);
        //    }

        //    if (defaultAspect.HasValue) {
        //        // TODO: return width, width*aspect
        //        var width = Material.TextureWidth.Value;
        //        var height = (int)(width * defaultAspect);

        //        if (Profile != null && Profile.TextureScale.HasValue) {
        //            var scale = (float)Profile.TextureScale.Value;
        //            width = (int)MathF.Ceiling(width * scale);
        //            height = (int)MathF.Ceiling(height * scale);
        //        }

        //        return new Size(width, height);
        //    }
        //}
        //else if (Material.TextureHeight.HasValue) {
        //    if (defaultAspect.HasValue) {
        //        // TODO: return height/aspect, height
        //    }
        //}

        //if (Material.TextureSize.HasValue) {
        //    var aspect = defaultAspect ?? 1f;
        //    var width = Material.TextureSize.Value;
        //    var height = (int)MathF.Ceiling(width * aspect);

        //    if (Profile?.TextureScale.HasValue ?? false) {
        //        var scale = (float)Profile.TextureScale.Value;
        //        width = (int)MathF.Ceiling(width * scale);
        //        height = (int)MathF.Ceiling(height * scale);
        //    }

        //    return new Size(width, height);
        //}

        var size = GetMaterialSize(defaultAspect);
        if (size.HasValue) return size;

        var type = GetFinalMaterialType();
        if (Profile != null)
            return TextureSizeUtility.GetSizeByType(Profile, type, defaultAspect);

        return null;
    }

    public Size? GetBufferSize(float aspect)
    {
        if (Profile == null) return null;

        var blockSize = Profile?.BlockTextureSize;
        var scale = (float?)Profile?.TextureScale;

        if (Material != null && Material.TryGetSourceBounds(in blockSize, scale, out var bounds)) return bounds;

        return GetTextureSize(aspect);
    }

    private static PackEncoding BuildEncoding(string? format)
    {
        PackEncoding? encoding = null;
        if (!string.IsNullOrWhiteSpace(format)) {
            var formatFactory = TextureFormat.GetFactory(format);
            encoding = formatFactory?.Create();
        }

        return encoding ?? new PackEncoding();
    }
}
