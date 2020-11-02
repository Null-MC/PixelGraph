using System;
using System.IO;

namespace PixelGraph.Common.IO
{
    public interface IOutputWriter : IAsyncDisposable
    {
        void SetRoot(string absolutePath);
        void Prepare();
        Stream WriteFile(string localFilename);
        bool FileExists(string localFile);
        DateTime? GetWriteTime(string localFile);
        void Clean();
    }
}
