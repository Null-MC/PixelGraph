using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Output
{
    internal class ArchiveOutputWriter : IOutputWriter
    {
        private readonly Stream fileStream;
        private readonly ZipArchive archive;


        public ArchiveOutputWriter(string destinationFile)
        {
            fileStream = File.Open(destinationFile, FileMode.Create, FileAccess.Write);
            archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        }

        public Stream WriteFile(string localFilename)
        {
            var entry = archive.CreateEntry(localFilename);
            return entry.Open();
        }

        public async ValueTask DisposeAsync()
        {
            archive?.Dispose();

            if (fileStream != null)
                await fileStream.DisposeAsync();
        }
    }
}
