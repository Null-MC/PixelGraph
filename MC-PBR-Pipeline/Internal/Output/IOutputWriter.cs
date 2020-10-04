using System;
using System.IO;

namespace McPbrPipeline.Internal.Output
{
    internal interface IOutputWriter : IAsyncDisposable
    {
        void Prepare();
        Stream WriteFile(string localFilename);
        DateTime? GetWriteTime(string localFile);
        void Clean();
    }
}
