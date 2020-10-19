using McPbrPipeline.Internal.Input;
using System.Collections.Generic;
using System.IO;
using McPbrPipeline.Internal.Extensions;

namespace McPbrPipeline.Tests.Internal
{
    internal class MockInputReader : IInputReader
    {
        public MockFileContent Content {get;}
        public string Root {get; set;} = ".";


        public MockInputReader(MockFileContent content)
        {
            Content = content;
        }

        public IEnumerable<string> EnumerateDirectories(string localPath, string pattern = default)
        {
            var fullPath = PathEx.Join(Root, localPath);

            foreach (var directory in Content.EnumerateDirectories(fullPath, pattern)) {
                var directoryName = Path.GetFileName(directory);
                yield return PathEx.Join(localPath, directoryName);
            }
        }

        public IEnumerable<string> EnumerateFiles(string localPath, string pattern = default)
        {
            var fullPath = PathEx.Join(Root, localPath);

            foreach (var file in Content.EnumerateFiles(fullPath, pattern)) {
                var fileName = Path.GetFileName(file.Filename) ?? string.Empty;
                yield return PathEx.Join(localPath, fileName);
            }
        }

        public bool FileExists(string localFile)
        {
            var fullFile = PathEx.Join(Root, localFile);
            return Content.FileExists(fullFile);
        }

        public Stream Open(string localFile)
        {
            var fullFile = PathEx.Join(Root, localFile);
            return Content.OpenRead(fullFile);
        }
    }
}
