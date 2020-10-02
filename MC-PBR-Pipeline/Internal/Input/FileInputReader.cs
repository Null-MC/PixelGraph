using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
            var fullPath = localPath == "." ? root
                : Path.Combine(root, localPath);

            foreach (var directory in Directory.EnumerateDirectories(fullPath, pattern, SearchOption.TopDirectoryOnly)) {
                var directoryName = Path.GetFileName(directory);

                yield return localPath == "." ? directoryName
                    : Path.Combine(localPath, directoryName);
            }
        }

        public IEnumerable<string> EnumerateFiles(string localPath, string pattern = "*")
        {
            var fullPath = localPath == "." ? root
                : Path.Combine(root, localPath);

            foreach (var file in Directory.EnumerateFiles(fullPath, pattern, SearchOption.TopDirectoryOnly)) {
                var fileName = Path.GetFileName(file);

                yield return localPath == "." ? fileName
                    : Path.Combine(localPath, fileName);
            }
        }

        public Stream Open(string localFile)
        {
            var fullFile = Path.Combine(root, localFile);
            return File.Open(fullFile, FileMode.Open, FileAccess.Read);
        }

        public Task<T> ReadJsonAsync<T>(string localFile, CancellationToken token = default) where T : new()
        {
            var fullFile = Path.Combine(root, localFile);
            return JsonFile.ReadAsync<T>(fullFile, token);
        }

        public bool FileExists(string localFile)
        {
            var fullFile = Path.Combine(root, localFile);
            return File.Exists(fullFile);
        }
    }
}
