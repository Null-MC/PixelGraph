using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PixelGraph.Common.IO
{
    internal class ArchiveInputReader : BaseInputReader, IDisposable
    {
        private Stream fileStream;
        private ZipArchive archive;


        //public ArchiveInputReader(INamingStructure naming) : base(naming) {}

        public override void SetRoot(string absolutePath)
        {
            fileStream = File.Open(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        }

        public override IEnumerable<string> EnumerateDirectories(string localPath, string pattern = null)
        {
            var fullPath = localPath == "." ? string.Empty : localPath;
            var start = fullPath.Length;

            return GetPathEntries(fullPath).Select(e => {
                var i = e.FullName.IndexOf('/', start + 1);
                return i < 0 ? null : e.FullName[..i].TrimStart('/');
            }).Where(x => x != null).Distinct();
        }

        public override IEnumerable<string> EnumerateFiles(string localPath, string pattern = null)
        {
            var fullPath = localPath == "." ? string.Empty : localPath;

            foreach (var entry in GetPathEntries(fullPath)) {
                var localEntryPath = entry.FullName[fullPath.Length..].TrimStart('/');
                if (localEntryPath.Contains('/')) continue;

                var entryName = Path.GetFileName(localEntryPath);
                if (PathEx.MatchPattern(entryName, pattern))
                    yield return entry.FullName;
            }
        }

        public override string GetFullPath(string localFile)
        {
            return Path.GetFullPath(localFile);
        }

        public override Stream Open(string localFile)
        {
            return archive.GetEntry(localFile)?.Open();
        }

        public override bool FileExists(string localFile)
        {
            return archive.GetEntry(localFile) != null;
        }

        public override DateTime? GetWriteTime(string localFile)
        {
            return archive.GetEntry(localFile)?
                .LastWriteTime.LocalDateTime;
        }

        public void Dispose()
        {
            fileStream?.Dispose();
            archive?.Dispose();
        }

        private IEnumerable<ZipArchiveEntry> GetPathEntries(string localPath)
        {
            foreach (var entry in archive.Entries) {
                if (string.IsNullOrEmpty(entry.Name)) continue;

                var path = GetDirectoryName(entry.FullName) ?? string.Empty;

                if (path.StartsWith(localPath, StringComparison.InvariantCultureIgnoreCase))
                    yield return entry;
            }
        }

        private static string GetDirectoryName(string path)
        {
            var i = path.LastIndexOf('/');
            return i > 0 ? path[..i] : null;
        }
    }
}
