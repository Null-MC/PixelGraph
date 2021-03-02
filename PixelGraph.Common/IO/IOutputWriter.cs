using System;
using System.IO;

namespace PixelGraph.Common.IO
{
    public interface IOutputWriter : IAsyncDisposable, IDisposable
    {
        void SetRoot(string absolutePath);
        void Prepare();
        Stream Open(string localFilename);
        //bool FileExists(string localFile);
        DateTime? GetWriteTime(string localFile);
        void Delete(string localFile);
        void Clean();
    }
}
