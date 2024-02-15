using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;

namespace PixelGraph.Common.IO.Texture;

public interface ITextureWriter
{
    string? TryGet(string tag, string textureName, string? extension, bool global);
    string? GetInputTextureName(MaterialProperties material, string tag);
    string? GetInputMetaName(MaterialProperties material, string tag);
    string? GetOutputMetaName(PublishProfileProperties pack, MaterialProperties material, string tag, bool global);
    string? GetOutputMetaName(PublishProfileProperties pack, string path, string name, string tag, bool global);
}

internal class TextureWriterBase : ITextureWriter
{
    protected Dictionary<string, string> LocalMap {get; set;}
    protected Dictionary<string, Func<string, string>> GlobalMap {get; set;}


    protected TextureWriterBase()
    {
        LocalMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        GlobalMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase);
    }

    public string? TryGet(string tag, string textureName, string? extension, bool global)
    {
        if (global) {
            if (GlobalMap.TryGetValue(tag, out var func)) {
                var name = func(textureName);
                return BuildName(name, extension);
            }
        }
        else {
            if (LocalMap.TryGetValue(tag, out var name))
                return BuildName(name, extension);
        }

        return null;
    }

    public string? GetInputTextureName(MaterialProperties material, string tag)
    {
        return TryGet(tag, material.Name, "*", material.UseGlobalMatching);
    }

    public string? GetInputMetaName(MaterialProperties material, string tag)
    {
        var path = NamingStructure.GetPath(material, material.UseGlobalMatching);
        var name = TryGet(tag, material.Name, "mcmeta", material.UseGlobalMatching);
        if (name == null) return null;

        var filename = PathEx.Join(path, name);
        return PathEx.Localize(filename);
    }

    public string? GetOutputMetaName(PublishProfileProperties pack, MaterialProperties material, string tag, bool global)
    {
        var path = NamingStructure.GetPath(material, global && material.CTM?.Method == null);
        var ext = NamingStructure.GetExtension(pack);
        var name = TryGet(tag, material.Name, $"{ext}.mcmeta", global);
        if (name == null) return null;

        var filename = PathEx.Join(path, name);
        return PathEx.Localize(filename);
    }

    public string? GetOutputMetaName(PublishProfileProperties pack, string path, string name, string tag, bool global)
    {
        var ext = NamingStructure.GetExtension(pack);
        var fileName = TryGet(tag, name, $"{ext}.mcmeta", global);
        if (fileName == null) return null;

        var filename = PathEx.Join(path, fileName);
        return PathEx.Localize(filename);
    }

    private static string BuildName(string name, string? ext)
    {
        var result = name;
        if (ext != null) result += $".{ext}";
        return result;
    }
}