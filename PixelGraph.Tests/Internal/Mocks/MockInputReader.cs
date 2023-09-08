using Microsoft.Extensions.Options;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Tests.Internal.Mocks;

internal class MockInputReader : BaseInputReader
{
    private readonly IOptions<InputOptions> options;
    private readonly MockFileContent _content;
    //public string Root {get; set;} = ".";


    public MockInputReader(
        IOptions<InputOptions> options,
        MockFileContent content)
    {
        this.options = options;
        _content = content;
    }

    public override IEnumerable<string> EnumerateDirectories(string localPath, string pattern = default)
    {
        var fullPath = GetFullPath(localPath);

        foreach (var directory in _content.EnumerateDirectories(fullPath, pattern)) {
            var directoryName = Path.GetFileName(directory);
            yield return PathEx.Join(localPath, directoryName);
        }
    }

    public override IEnumerable<string> EnumerateFiles(string localPath, string pattern = default)
    {
        var fullPath = GetFullPath(localPath);

        foreach (var file in _content.EnumerateFiles(fullPath, pattern)) {
            var fileName = Path.GetFileName(file.Filename) ?? string.Empty;
            yield return PathEx.Join(localPath, fileName);
        }
    }

    public override bool FileExists(string localFile)
    {
        var fullFile = GetFullPath(localFile);
        return _content.FileExists(fullFile);
    }

    public override Stream Open(string localFile)
    {
        var fullFile = GetFullPath(localFile);
        return _content.OpenRead(fullFile);
    }

    public override DateTime? GetWriteTime(string localFile) => null;

    public override string GetFullPath(string localPath)
    {
        return PathEx.Join(options.Value.Root, localPath);
    }

    public override string GetRelativePath(string fullPath)
    {
        return PathEx.TryGetRelative(options.Value.Root, fullPath, out var localPath) ? localPath : fullPath;
    }
}