using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    internal class ArchiveOutputWriter : IOutputWriter
    {
        private readonly AsyncReaderWriterLock locker;
        private readonly Stream fileStream;
        private readonly ZipArchive archive;


        public ArchiveOutputWriter(IOptions<OutputOptions> options)
        {
            locker = new AsyncReaderWriterLock();

            fileStream = File.Open(options.Value.Root, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            archive = new ZipArchive(fileStream, ZipArchiveMode.Update);
        }

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

        public void Prepare() {}

        public async Task<T> OpenReadAsync<T>(string localFilename, Func<Stream, Task<T>> readFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var readLock = await locker.ReaderLockAsync();
            var entry = archive.GetEntry(localFilename);
            if (entry == null) throw new FileNotFoundException("File not found!", localFilename);

            await using var stream = entry.Open();
            return await readFunc(stream);
        }

        public async Task<long> OpenWriteAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await locker.WriterLockAsync(token);
            var entry = archive.CreateEntry(localFilename);

            await using var stream = new MemoryStream();
            await writeFunc(stream);
            await stream.FlushAsync(token);
            var size = stream.Length;

            stream.Seek(0, SeekOrigin.Begin);
            await using var entryStream = entry.Open();
            await stream.CopyToAsync(entryStream, token);

            // ERROR: Cannot get length of ArchiveEntry after writing!
            return size; //entry.Length;
        }

        public async Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> readWriteFunc, CancellationToken token = default)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFilename = localFilename.Replace(Path.DirectorySeparatorChar, '/');

            using var @lock = await locker.WriterLockAsync(token);
            var entry = archive.GetEntry(localFilename)
                     ?? archive.CreateEntry(localFilename);

            await using var stream = entry.Open();
            await readWriteFunc(stream);
        }

        public bool FileExists(string localFile)
        {
            if (Path.DirectorySeparatorChar != '/')
                localFile = localFile.Replace(Path.DirectorySeparatorChar, '/');

            return archive.Entries.Any(e => string.Equals(e.FullName, localFile));
            //return archive.GetEntry(localFile) != null;
        }

        public DateTime? GetWriteTime(string localFile) => null;

        public void Delete(string localFile)
        {
            using var @lock = locker.WriterLock();
            archive.GetEntry(localFile)?.Delete();
        }

        public void Clean()
        {
            var allEntries = archive.Entries.ToArray();
            foreach (var entry in allEntries) entry.Delete();
        }
    }
}
