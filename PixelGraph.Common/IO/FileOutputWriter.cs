using Microsoft.Extensions.Options;
using PixelGraph.Common.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO;

internal class FileOutputWriter : IOutputWriter
{
    private readonly IOptions<OutputOptions> options;


    public FileOutputWriter(IOptions<OutputOptions> options)
    {
        this.options = options;
    }

    public void Prepare()
    {
        if (!Directory.Exists(options.Value.Root))
            Directory.CreateDirectory(options.Value.Root);
    }

    public async Task<T> OpenReadAsync<T>(string localFilename, Func<Stream, Task<T>> readFunc, CancellationToken token = default)
    {
        var filename = PathEx.Join(options.Value.Root, localFilename);
        filename = PathEx.Localize(filename);

        await using var stream = File.Open(filename, FileMode.Open, FileAccess.Read);
        return await readFunc(stream);
    }

    public async Task<long> OpenWriteAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
    {
        var filename = PathEx.Join(options.Value.Root, localFilename);
        filename = PathEx.Localize(filename);
        CreateMissingDirectory(filename);

        var file = new FileInfo(filename);
        await using (var stream = file.Open(FileMode.Create, FileAccess.Write)) {
            await writeFunc(stream);
        }

        return file.Length;
    }

    public async Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
    {
        var filename = PathEx.Join(options.Value.Root, localFilename);
        filename = PathEx.Localize(filename);
        CreateMissingDirectory(filename);

        await using var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        await writeFunc(stream);
    }

    public bool FileExists(string localFile)
    {
        var fullFile = PathEx.Join(options.Value.Root, localFile);
        return File.Exists(fullFile);
    }

    public void Delete(string localFile)
    {
        var fullFile = PathEx.Join(options.Value.Root, localFile);
        File.Delete(fullFile);
    }

    public void Clean()
    {
        Directory.Delete(options.Value.Root, true);
    }

    public DateTime? GetWriteTime(string localFile)
    {
        var fullFile = PathEx.Join(options.Value.Root, localFile);
        if (!File.Exists(fullFile)) return null;
        return File.GetLastWriteTime(fullFile);
    }

    public void Dispose() {}
    public ValueTask DisposeAsync() => default;

    private static void CreateMissingDirectory(string filename)
    {
        var path = Path.GetDirectoryName(filename);
        if (string.IsNullOrEmpty(path)) return;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}