using System.IO;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Output
{
    internal class FileOutputWriter : IOutputWriter
    {
        private readonly string destinationPath;


        public FileOutputWriter(string destinationPath)
        {
            this.destinationPath = destinationPath;
        }

        public Stream WriteFile(string localFilename)
        {
            var filename = Path.Combine(destinationPath, localFilename);

            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return File.Open(filename, FileMode.Create, FileAccess.Write);
        }

        public ValueTask DisposeAsync() => default;
    }
}
