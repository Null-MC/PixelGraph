using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO
{
    public interface IInputReader
    {
        void SetRoot(string absolutePath);
        IEnumerable<string> EnumerateDirectories(string localPath, string pattern = null);
        IEnumerable<string> EnumerateFiles(string localPath, string pattern = null);
        bool FileExists(string localFile);
        string GetFullPath(string localFile);
        string GetRelativePath(string fullPath);
        Stream Open(string localFile);
        DateTime? GetWriteTime(string localFile);
    }
}
