using Microsoft.Extensions.Options;
using Nito.Disposables.Internals;
using PixelGraph.Common.Extensions;
using System.IO.Compression;

namespace PixelGraph.Common.IO;

internal class ArchiveInputReader : BaseInputReader, IDisposable
{
    private readonly Stream fileStream;
    private readonly ZipArchive archive;
    private readonly string filename;


    public ArchiveInputReader(IOptions<InputOptions> options)
    {
        filename = options.Value.Root ?? throw new ApplicationException("Input filename is undefined!");
        fileStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
    }

    public void Dispose()
    {
        fileStream.Dispose();
        archive.Dispose();
    }

    public override IEnumerable<string> EnumerateDirectories(string localPath, string? pattern = null)
    {
        var fullPath = localPath == "." ? string.Empty : localPath;
        var start = fullPath.Length;

        return GetPathEntries(fullPath).Select(e => {
            var i = e.FullName.IndexOf('/', start + 1);
            return i < 0 ? null : e.FullName[..i].TrimStart('/');
        }).WhereNotNull().Distinct();
    }

    public override IEnumerable<string> EnumerateFiles(string localPath, string? pattern = null)
    {
        var fullPath = localPath == "." ? string.Empty : localPath;

        foreach (var entry in GetPathEntries(fullPath)) {
            var localEntryPath = entry.FullName[fullPath.Length..].TrimStart('/');
            if (localEntryPath.Contains('/')) continue;

            var entryName = Path.GetFileName(localEntryPath);
            if (PathEx.MatchPattern(entryName, pattern))
                yield return entry.FullName;
        }
    }

    public override string GetFullPath(string localFile)
    {
        return $"{filename}|{localFile}";
    }

    public override string GetRelativePath(string fullPath)
    {
        throw new NotImplementedException();
    }

    public override Stream? Open(string localFile)
    {
        var file = PathEx.Normalize(localFile);
        if (file == null) throw new FileNotFoundException();

        return archive.GetEntry(file)?.Open();
    }

    public override bool FileExists(string localFile)
    {
        var file = PathEx.Normalize(localFile);
        return file != null && archive.GetEntry(file) != null;
    }

    public override DateTime? GetWriteTime(string localFile)
    {
        return archive.GetEntry(localFile)?
            .LastWriteTime.LocalDateTime;
    }

    private IEnumerable<ZipArchiveEntry> GetPathEntries(string localPath)
    {
        var path = PathEx.Normalize(localPath);
        if (path == null) throw new FileNotFoundException();

        foreach (var entry in archive.Entries) {
            if (string.IsNullOrEmpty(entry.Name)) continue;

            var entryPath = GetDirectoryName(entry.FullName) ?? string.Empty;

            if (entryPath.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                yield return entry;
        }
    }

    private static string? GetDirectoryName(string path)
    {
        var i = path.LastIndexOf('/');
        return i > 0 ? path[..i] : null;
    }
}