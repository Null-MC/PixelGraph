using Microsoft.Extensions.DependencyInjection;
using Nito.Disposables.Internals;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;

namespace PixelGraph.Common.IO.Texture;

public interface ITextureReader
{
    bool TryGetByTag(string tag, out string? localFile);
    bool TryGetByName(in string name, out string? localFile);

    //IEnumerable<string> EnumerateInputTextures(string localPath, string filename);
    IEnumerable<string> EnumerateInputTextures(MaterialProperties material, string tag);
    //IEnumerable<string> EnumerateOutputTextures(string destName, string destPath, string tag, bool global);
    IEnumerable<string> EnumerateAllTextures(MaterialProperties material);

    bool IsLocalFile(string localFile, string tag);
    bool IsGlobalFile(string localFile, string name, string tag);
}

internal abstract class TextureReaderBase(IServiceProvider provider) : ITextureReader
{
    protected ITextureGraphContext Context {get;} = provider.GetRequiredService<ITextureGraphContext>();
    protected IInputReader Reader {get;} = provider.GetRequiredService<IInputReader>();


    public virtual bool TryGetByTag(string tag, out string? localFile)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(Context.Material);

        var textureList = Context.IsImport
            ? EnumerateOutputTextures(Context.Material.Name, Context.Material.LocalPath, tag, true)
            : EnumerateInputTextures(Context.Material, tag);

        // TODO: All enum files should exist, why are we checking?
        localFile = textureList.FirstOrDefault(f => Reader.FileExists(f));
        //localFile = textureList.FirstOrDefault();
        return localFile != null;
    }

    public virtual bool TryGetByName(in string localName, out string? localFile)
    {
        var localPath = Path.GetDirectoryName(localName);
        var name = Path.GetFileName(localName);

        var textureList = Context.IsImport
            ? EnumerateOutputTextures(localPath, name)
            : EnumerateInputTextures(localPath, name);

        // TODO: All enum files should exist, why are we checking?
        //filename = textureList.FirstOrDefault(f => Reader.FileExists(f));
        localFile = textureList.FirstOrDefault();
        return localFile != null;
    }

    public virtual IEnumerable<string> EnumerateInputTextures(string? localPath, string name)
    {
        if (localPath == null) throw new ArgumentNullException(nameof(localPath));
        if (name == null) throw new ArgumentNullException(nameof(name));

        foreach (var file in Reader.EnumerateFiles(localPath)) {
            var ext = Path.GetExtension(file);
            if (!ImageExtensions.Supports(ext)) continue;

            var _name = Path.GetFileNameWithoutExtension(file);
            if (!string.Equals(_name, name, StringComparison.InvariantCultureIgnoreCase)) continue;

            yield return file;
        }
    }

    public virtual IEnumerable<string> EnumerateInputTextures(MaterialProperties material, string tag)
    {
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(tag);

        var srcPath = material.UseGlobalMatching
            ? material.LocalPath : PathEx.Join(material.LocalPath, material.Name);

        var localName = TextureTags.Get(material, tag);

        while (localName != null) {
            var linkedFilename = TextureTags.Get(material, localName);
            if (string.IsNullOrEmpty(linkedFilename)) break;

            tag = localName;
            localName = linkedFilename;
        }

        if (!string.IsNullOrEmpty(localName)) {
            localName = PathEx.Localize(localName);
            yield return PathEx.Join(srcPath, localName);
        }

        foreach (var file in Reader.EnumerateFiles(srcPath)) {
            var ext = Path.GetExtension(file);
            if (!ImageExtensions.Supports(ext)) continue;

            var isMatch = material.UseGlobalMatching
                ? IsGlobalFile(file, material.Name, tag)
                : IsLocalFile(file, tag);

            if (isMatch) yield return file;
        }
    }

    public virtual IEnumerable<string> EnumerateOutputTextures(string? localPath, string name)
    {
        if (localPath == null) throw new ArgumentNullException(nameof(localPath));
        if (name == null) throw new ArgumentNullException(nameof(name));

        foreach (var file in Reader.EnumerateFiles(localPath)) {
            var ext = Path.GetExtension(file);
            if (!ImageExtensions.Supports(ext)) continue;

            var _name = Path.GetFileNameWithoutExtension(file);
            if (!string.Equals(_name, name, StringComparison.InvariantCultureIgnoreCase)) continue;

            yield return file;
        }
    }

    public virtual IEnumerable<string> EnumerateOutputTextures(string destName, string destPath, string tag, bool global)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var srcPath = global
            ? destPath : PathEx.Join(destPath, destName);

        foreach (var file in Reader.EnumerateFiles(srcPath)) {
            var ext = Path.GetExtension(file);
            if (!ImageExtensions.Supports(ext)) continue;

            var isMatch = global
                ? IsGlobalFile(file, destName, tag)
                : IsLocalFile(file, tag);

            if (isMatch) yield return file;
        }
    }

    public abstract bool IsLocalFile(string localFile, string tag);
    //{
    //    return NamingStructure.IsLocalFileTag(localFile, tag);
    //}

    public abstract bool IsGlobalFile(string localFile, string name, string tag);
    //{
    //    return NamingStructure.IsGlobalFileTag(localFile, name, tag);
    //}

    public virtual IEnumerable<string> EnumerateAllTextures(MaterialProperties material)
    {
        return TextureTags.All
            .SelectMany(tag => EnumerateInputTextures(material, tag))
            .WhereNotNull().Distinct();
    }
}