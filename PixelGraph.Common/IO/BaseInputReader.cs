using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO;

public abstract class BaseInputReader : IInputReader
{
    public abstract IEnumerable<string> EnumerateDirectories(string localPath, string pattern = null);
    public abstract IEnumerable<string> EnumerateFiles(string localPath, string pattern = null);
    public abstract bool FileExists(string localFile);
    public abstract string GetFullPath(string localFile);
    public abstract string GetRelativePath(string fullPath);
    public abstract Stream Open(string localFile);
    public abstract DateTime? GetWriteTime(string localFile);
}