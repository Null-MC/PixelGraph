using System.Collections.Generic;
using System.IO;

namespace McPbrPipeline.Internal.Input
{
    internal interface IInputReader
    {
        IEnumerable<string> EnumerateDirectories(string localPath, string pattern);
        IEnumerable<string> EnumerateFiles(string localPath, string pattern);
        //Task<T> ReadJsonAsync<T>(string localFile, CancellationToken token = default) where T : new();
        bool FileExists(string localFile);
        Stream Open(string localFile);
        //DateTime? GetWriteTime(string localFile);
    }
}
