using PixelGraph.Common.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PixelGraph.Tests.Internal
{
    public class MockFileContent : IDisposable, IAsyncDisposable
    {
        public List<MockFile> Files {get; set;}


        public MockFileContent()
        {
            Files = new List<MockFile>();
        }

        public void Add(string filename, MockStream content = null)
        {
            var f = new MockFile {
                Filename = PathEx.Normalize(filename),
                Content = content?.BaseStream,
            };

            Files.Add(f);
        }

        public IEnumerable<string> EnumerateDirectories(string localPath, string pattern)
        {
            var subdirectories = Files.Select(f => Path.GetDirectoryName(f.Filename) ?? string.Empty)
                .Where(d => d.StartsWith(localPath, StringComparison.InvariantCultureIgnoreCase)).Distinct();

            foreach (var subdirectory in subdirectories) {
                var subPath = subdirectory[localPath.Length..].TrimStart(Path.DirectorySeparatorChar);
                if (string.IsNullOrEmpty(subPath)) continue;

                var i = subPath.IndexOf(Path.DirectorySeparatorChar);
                yield return i < 0 ? subPath : subPath[..i];
            }
        }

        public IEnumerable<MockFile> EnumerateFiles(string localPath, string pattern)
        {
            return Files.Where(f => string.Equals(Path.GetDirectoryName(f.Filename), localPath, StringComparison.InvariantCultureIgnoreCase))
                .Where(d => PathEx.MatchPattern(Path.GetFileName(d.Filename), pattern));
        }

        public bool FileExists(string filename)
        {
            return Files.Select(f => f.Filename).Contains(filename, StringComparer.InvariantCultureIgnoreCase);
        }

        public Stream OpenRead(string filename)
        {
            var file = PathEx.Normalize(filename);
            bool Match(MockFile f) => string.Equals(f.Filename, file, StringComparison.InvariantCultureIgnoreCase);
            var content = Files.FirstOrDefault(Match)?.Content;
            if (content == null) return null;

            var resultStream = new MemoryStream();

            try {
                content.Seek(0, SeekOrigin.Begin);
                content.CopyToAsync(resultStream);

                resultStream.Seek(0, SeekOrigin.Begin);
                return resultStream;
            }
            catch {
                resultStream.Dispose();
                throw;
            }
        }

        public async Task<Image<Rgba32>> OpenImageAsync(string filename)
        {
            await using var stream = OpenRead(filename);
            if (stream == null) throw new FileNotFoundException("Image not found!", filename);

            return await Image.LoadAsync<Rgba32>(Configuration.Default, stream);
        }

        public void Dispose()
        {
            foreach (var file in Files)
                file.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var file in Files)
                await file.DisposeAsync();
        }

        public async Task AddAsync(string filename, string text = "")
        {
            await using var stream = new MockStream();
            Add(filename, stream);

            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(text);
        }

        public async Task AddAsync(string filename, Image image)
        {
            await using var stream = new MockStream();
            Add(filename, stream);

            await image.SaveAsPngAsync(stream);
        }
    }

    public class MockFile : IDisposable, IAsyncDisposable
    {
        public string Filename {get; set;}
        public Stream Content {get; set;}


        public void Dispose()
        {
            Content?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Content != null) await Content.DisposeAsync();
        }
    }
}
