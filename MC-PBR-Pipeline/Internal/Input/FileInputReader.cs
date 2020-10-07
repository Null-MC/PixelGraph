using McPbrPipeline.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace McPbrPipeline.Internal.Input
{
    internal class FileInputReader : IInputReader
    {
        private readonly string root;


        public FileInputReader(string root)
        {
            this.root = root;
        }

        public IEnumerable<string> EnumerateDirectories(string localPath, string pattern = "*")
        {
            var fullPath = PathEx.Join(root, localPath);

            foreach (var directory in Directory.EnumerateDirectories(fullPath, pattern, SearchOption.TopDirectoryOnly)) {
                var directoryName = Path.GetFileName(directory);
                yield return PathEx.Join(localPath, directoryName);
            }
        }

        public IEnumerable<string> EnumerateFiles(string localPath, string pattern = "*")
        {
            var fullPath = PathEx.Join(root, localPath);

            foreach (var file in Directory.EnumerateFiles(fullPath, pattern, SearchOption.TopDirectoryOnly)) {
                var fileName = Path.GetFileName(file);
                yield return PathEx.Join(localPath, fileName);
            }
        }

        public Stream Open(string localFile)
        {
            var fullFile = PathEx.Join(root, localFile);
            return File.Open(fullFile, FileMode.Open, FileAccess.Read);
        }

        //public Task<T> ReadJsonAsync<T>(string localFile, CancellationToken token = default) where T : new()
        //{
        //    var fullFile = Path.Combine(root, localFile);
        //    return JsonFile.ReadAsync<T>(fullFile, token);
        //}

        public bool FileExists(string localFile)
        {
            var fullFile = PathEx.Join(root, localFile);
            return File.Exists(fullFile);
        }

        public DateTime? GetWriteTime(string localFile)
        {
            var fullFile = PathEx.Join(root, localFile);
            if (!File.Exists(fullFile)) return null;
            return File.GetLastWriteTime(fullFile);
        }
    }
}
