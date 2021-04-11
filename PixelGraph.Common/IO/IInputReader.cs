using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO
{
    public interface IInputReader
    {
        void SetRoot(string absolutePath);
        IEnumerable<string> EnumerateDirectories(string localPath, string pattern);
        IEnumerable<string> EnumerateFiles(string localPath, string pattern);
        IEnumerable<string> EnumerateInputTextures(MaterialProperties material, string tag);
        IEnumerable<string> EnumerateOutputTextures(ResourcePackProfileProperties pack, MaterialProperties material, string tag, bool global);
        IEnumerable<string> EnumerateAllTextures(MaterialProperties material);
        bool FileExists(string localFile);
        string GetFullPath(string localFile);
        Stream Open(string localFile);
        DateTime? GetWriteTime(string localFile);
    }
}
