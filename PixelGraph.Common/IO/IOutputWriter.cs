using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public interface IOutputWriter : IAsyncDisposable, IDisposable
    {
        bool AllowConcurrency {get;}

        void SetRoot(string absolutePath);
        void Prepare();
        Task OpenAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default);
        Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> readWriteFunc, CancellationToken token = default);
        //bool FileExists(string localFile);
        DateTime? GetWriteTime(string localFile);
        void Delete(string localFile);
        void Clean();
    }
}
