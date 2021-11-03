using Nito.AsyncEx;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    internal class ArchiveOutputWriter : IOutputWriter
    {
        private readonly AsyncLock writeLock;
        private Stream fileStream;
        private ZipArchive archive;

        public bool AllowConcurrency => false;


        public ArchiveOutputWriter()
        {
            writeLock = new AsyncLock();
        }

        public void SetRoot(string absolutePath)
        {
            archive?.Dispose();
            fileStream?.Dispose();

            fileStream = File.Open(absolutePath, FileMode.Create, FileAccess.Write);
            archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        }

        public void Prepare() {}

        public async Task OpenAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await writeLock.LockAsync(token);
            var entry = archive.CreateEntry(localFilename);

            await using var stream = entry.Open();
            await writeFunc(stream);
        }

        public async Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> readWriteFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await writeLock.LockAsync(token);
            var entry = archive.GetEntry(localFilename)
                     ?? archive.CreateEntry(localFilename);

            await using var stream = entry.Open();
            await readWriteFunc(stream);
        }

        public bool FileExists(string localFile) => false;
        public DateTime? GetWriteTime(string localFile) => null;

        public void Delete(string localFile)
        {
            archive.GetEntry(localFile)?.Delete();
        }

        public void Clean() {}

        public void Dispose()
        {
            archive?.Dispose();
            fileStream?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            archive?.Dispose();

            if (fileStream != null)
                await fileStream.DisposeAsync();
        }
    }

    //public class StreamWrapper : Stream, IDisposable
    //{
    //    private readonly IDisposable @lock;
    //    private readonly Stream stream;

    //    public override bool CanRead => stream.CanRead;
    //    public override bool CanSeek => stream.CanSeek;
    //    public override bool CanWrite => stream.CanWrite;
    //    public override long Length => stream.Length;

    //    public override long Position {
    //        get => stream.Position;
    //        set => stream.Position = value;
    //    }


    //    public StreamWrapper(Stream stream, IDisposable @lock)
    //    {
    //        this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
    //        this.@lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
    //    }

    //    public new void Dispose()
    //    {
    //        stream.Dispose();
    //        @lock.Dispose();
    //    }

    //    public override void Flush() => stream.Flush();
    //    public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
    //    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
    //    public override void SetLength(long value) => stream.SetLength(value);
    //    public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
    //}
}
