using Microsoft.Extensions.Options;
using PixelGraph.Common.Extensions;

namespace PixelGraph.Common.IO;

internal class FileInputReader : BaseInputReader
{
    private readonly IOptions<InputOptions> options;


    public FileInputReader(IOptions<InputOptions> options)
    {
        this.options = options;
    }

    public override IEnumerable<string> EnumerateDirectories(string? localPath, string? pattern = null)
    {
        var fullPath = GetFullPath(localPath);
        if (!Directory.Exists(fullPath)) yield break;

        foreach (var directory in Directory.EnumerateDirectories(fullPath, pattern ?? "*", SearchOption.TopDirectoryOnly)) {
            var directoryName = Path.GetFileName(directory);
            yield return PathEx.Join(localPath, directoryName);
        }
    }

    public override IEnumerable<string> EnumerateFiles(string? localPath, string? pattern = null)
    {
        var fullPath = GetFullPath(localPath);
        if (!Directory.Exists(fullPath)) yield break;

        foreach (var file in Directory.EnumerateFiles(fullPath, pattern ?? "*.*", SearchOption.TopDirectoryOnly)) {
            var fileName = Path.GetFileName(file);
            yield return PathEx.Join(localPath, fileName);
        }
    }

    public override Stream Open(string localFile)
    {
        var fullFile = GetFullPath(localFile);
        return File.Open(fullFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public override bool FileExists(string localFile)
    {
        var fullFile = GetFullPath(localFile);
        return File.Exists(fullFile);
    }

    public override string GetFullPath(string? localFile)
    {
        var fullFile = PathEx.Join(options.Value.Root, localFile);
        fullFile = PathEx.Localize(fullFile);
        return Path.GetFullPath(fullFile);
    }

    public override string GetRelativePath(string fullPath)
    {
        if (options.Value.Root == null) throw new ApplicationException("Root path is undefined!");

        return PathEx.TryGetRelative(options.Value.Root, fullPath, out var localPath) ? localPath : fullPath;
    }

    public override DateTime? GetWriteTime(string localFile)
    {
        var fullFile = PathEx.Join(options.Value.Root, localFile);
        if (!File.Exists(fullFile)) return null;
        return File.GetLastWriteTime(fullFile);
    }
}
