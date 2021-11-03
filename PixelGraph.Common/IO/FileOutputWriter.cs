using PixelGraph.Common.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    internal class FileOutputWriter : IOutputWriter
    {
        private string destinationPath;


        public bool AllowConcurrency => true;

        public void SetRoot(string absolutePath)
        {
            destinationPath = absolutePath;
        }

        public void Prepare()
        {
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);
        }

        public async Task OpenAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
        {
            var filename = PathEx.Join(destinationPath, localFilename);
            CreateMissingDirectory(filename);

            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            await writeFunc(stream);
        }

        public async Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default)
        {
            var filename = PathEx.Join(destinationPath, localFilename);
            CreateMissingDirectory(filename);

            await using var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await writeFunc(stream);
        }

        public bool FileExists(string localFile)
        {
            var fullFile = PathEx.Join(destinationPath, localFile);
            return File.Exists(fullFile);
        }

        public void Delete(string localFile)
        {
            var fullFile = PathEx.Join(destinationPath, localFile);
            File.Delete(fullFile);
        }

        public void Clean()
        {
            Directory.Delete(destinationPath, true);
        }

        public DateTime? GetWriteTime(string localFile)
        {
            var fullFile = PathEx.Join(destinationPath, localFile);
            if (!File.Exists(fullFile)) return null;
            return File.GetLastWriteTime(fullFile);
        }

        public void Dispose() {}
        public ValueTask DisposeAsync() => default;

        private static void CreateMissingDirectory(string filename)
        {
            var path = Path.GetDirectoryName(filename);
            if (path == null) return;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
