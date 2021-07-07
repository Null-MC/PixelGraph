using PixelGraph.Common.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    internal class FileOutputWriter : IOutputWriter
    {
        private string destinationPath;


        public void SetRoot(string absolutePath)
        {
            destinationPath = absolutePath;
        }

        public void Prepare()
        {
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);
        }

        public Stream Open(string localFilename)
        {
            var filename = PathEx.Join(destinationPath, localFilename);
            CreateMissingDirectory(filename);

            return File.Open(filename, FileMode.Create, FileAccess.Write);
        }

        public Stream OpenReadWrite(string localFilename)
        {
            var filename = PathEx.Join(destinationPath, localFilename);
            CreateMissingDirectory(filename);

            return File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
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
