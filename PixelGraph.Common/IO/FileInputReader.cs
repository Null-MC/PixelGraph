using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO
{
    internal class FileInputReader : BaseInputReader
    {
        private string root;


        //public FileInputReader() : base() {}

        public override void SetRoot(string absolutePath)
        {
            root = absolutePath;
        }

        public override IEnumerable<string> EnumerateDirectories(string localPath, string pattern)
        {
            var fullPath = GetFullPath(localPath);

            foreach (var directory in Directory.EnumerateDirectories(fullPath, pattern, SearchOption.TopDirectoryOnly)) {
                var directoryName = Path.GetFileName(directory);
                yield return PathEx.Join(localPath, directoryName);
            }
        }

        public override IEnumerable<string> EnumerateFiles(string localPath, string pattern)
        {
            var fullPath = GetFullPath(localPath);

            foreach (var file in Directory.EnumerateFiles(fullPath, pattern, SearchOption.TopDirectoryOnly)) {
                var fileName = Path.GetFileName(file);
                yield return PathEx.Join(localPath, fileName);
            }
        }

        public override Stream Open(string localFile)
        {
            var fullFile = GetFullPath(localFile);
            return File.Open(fullFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public override bool FileExists(string localFile)
        {
            var fullFile = GetFullPath(localFile);
            return File.Exists(fullFile);
        }

        public override string GetFullPath(string localFile)
        {
            var fullFile = PathEx.Join(root, localFile);
            return Path.GetFullPath(fullFile);
        }

        public override DateTime? GetWriteTime(string localFile)
        {
            var fullFile = PathEx.Join(root, localFile);
            if (!File.Exists(fullFile)) return null;
            return File.GetLastWriteTime(fullFile);
        }
    }
}
