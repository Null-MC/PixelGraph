using McPbrPipeline.Internal.Input;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Tests.Internal
{
    internal class MockInputReader : IInputReader
    {
        public MockInputContent Content {get;}
        public string Root {get; set;}


        public MockInputReader()
        {
            Content = new MockInputContent();
        }

        public IEnumerable<string> EnumerateDirectories(string localPath, string pattern = default)
        {
            var fullPath = localPath == "." ? Root : Path.Combine(Root, localPath);

            foreach (var directory in Content.EnumerateDirectories(fullPath, pattern)) {
                var directoryName = Path.GetFileName(directory);
                yield return localPath == "." ? directoryName : Path.Combine(localPath, directoryName);
            }
        }

        public IEnumerable<string> EnumerateFiles(string localPath, string pattern = default)
        {
            var fullPath = Path.Combine(Root, localPath);

            foreach (var file in Content.EnumerateFiles(fullPath, pattern)) {
                var fileName = Path.GetFileName(file);
                yield return Path.Combine(localPath, fileName);
            }
        }

        public Task<T> ReadJsonAsync<T>(string localFile, CancellationToken token = default) where T : new()
        {
            if (!FileExists(localFile)) throw new FileNotFoundException();

            return Task.FromResult(new T());
        }

        public bool FileExists(string localFile)
        {
            var fullFile = Path.Combine(Root, localFile);
            return Content.FileExists(fullFile);
        }
    }
}
