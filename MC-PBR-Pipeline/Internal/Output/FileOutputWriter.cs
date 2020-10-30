using McPbrPipeline.Internal.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Output
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

        public Stream WriteFile(string localFilename)
        {
            var filename = PathEx.Join(destinationPath, localFilename);

            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return File.Open(filename, FileMode.Create, FileAccess.Write);
        }

        public bool FileExists(string localFile)
        {
            var fullFile = PathEx.Join(destinationPath, localFile);
            return File.Exists(fullFile);
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

        public ValueTask DisposeAsync() => default;
    }
}
