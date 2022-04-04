using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public class OutputOptions
    {
        //public string RootPath {get; set;}
        //public string RootFile {get; set;}
        public string Root {get; set;}
    }

    public interface IOutputWriter : IAsyncDisposable, IDisposable
    {
        void Prepare();
        Task OpenReadAsync(string localFilename, Func<Stream, Task> readFunc, CancellationToken token = default);
        Task OpenWriteAsync(string localFilename, Func<Stream, Task> writeFunc, CancellationToken token = default);
        Task OpenReadWriteAsync(string localFilename, Func<Stream, Task> readWriteFunc, CancellationToken token = default);
        bool FileExists(string localFile);
        DateTime? GetWriteTime(string localFile);
        void Delete(string localFile);
        void Clean();
    }
}
