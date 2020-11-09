using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO
{
    public interface IInputReader
    {
        void SetRoot(string absolutePath);
        string GetFullPath(string localPath);
        IEnumerable<string> EnumerateDirectories(string localPath, string pattern);
        IEnumerable<string> EnumerateFiles(string localPath, string pattern);
        IEnumerable<string> EnumerateTextures(PbrProperties texture, string tag);
        bool FileExists(string localFile);
        Stream Open(string localFile);
        DateTime? GetWriteTime(string localFile);
    }
}
