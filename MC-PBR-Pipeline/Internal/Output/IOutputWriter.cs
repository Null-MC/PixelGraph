using System;
using System.IO;

namespace McPbrPipeline.Internal.Output
{
    internal interface IOutputWriter : IAsyncDisposable
    {
        Stream WriteFile(string localFilename);
        DateTime? GetWriteTime(string localFile);
        void Clean();
    }
}
