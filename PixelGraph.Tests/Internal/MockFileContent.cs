using PixelGraph.Common.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;

namespace PixelGraph.Tests.Internal
{
    public class MockFileContent : IDisposable, IAsyncDisposable
    {
        private readonly IServiceProvider provider;

        public List<MockFile> Files {get; set;}


        public MockFileContent(IServiceProvider provider)
        {
            this.provider = provider;

            Files = new List<MockFile>();
        }

        public void Add(string filename, Stream content = null)
        {
            Files.Add(new MockFile {
                Filename = PathEx.Normalize(filename),
                Content = content,
            });
        }

        public IEnumerable<string> EnumerateDirectories(string path, string pattern)
        {
            var subdirectories = Files.Select(f => Path.GetDirectoryName(f.Filename) ?? string.Empty)
                .Where(d => d.StartsWith(path, StringComparison.InvariantCultureIgnoreCase)).Distinct();

            foreach (var subdirectory in subdirectories) {
                var subPath = subdirectory[path.Length..].TrimStart(Path.DirectorySeparatorChar);
                if (string.IsNullOrEmpty(subPath)) continue;

                var i = subPath.IndexOf(Path.DirectorySeparatorChar);
                yield return i < 0 ? subPath : subPath[..i];
            }
        }

        public IEnumerable<MockFile> EnumerateFiles(string path, string pattern)
        {
            return Files.Where(f => string.Equals(Path.GetDirectoryName(f.Filename), path, StringComparison.InvariantCultureIgnoreCase))
                .Where(d => MatchPattern(Path.GetFileName(d.Filename), pattern));
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
            content?.Seek(0, SeekOrigin.Begin);
            return content;
        }

        public async Task<Image<Rgba32>> OpenImageAsync(string filename)
        {
            await using var stream = OpenRead(filename);
            return await Image.LoadAsync<Rgba32>(Configuration.Default, stream);
        }

        private static bool MatchPattern(string name, string pattern)
        {
            if (pattern == null || pattern == "*") return true;

            var regexPattern = Regex.Escape(pattern)
                .Replace("\\?", ".")
                .Replace("\\*", ".+");

            return Regex.IsMatch(name, regexPattern);
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
            var stream = new MemoryStream();

            try {
                var writer = new StreamWriter(stream, leaveOpen: true);
                await writer.WriteAsync(text);
                Add(filename, stream);
            }
            catch {
                await stream.DisposeAsync();
                throw;
            }
        }

        public async Task AddAsync(string filename, MaterialProperties material)
        {
            var stream = new MemoryStream();

            try {
                var writer = provider.GetRequiredService<IMaterialWriter>();
                await writer.WriteAsync(material, filename);
                Add(filename, stream);
            }
            catch {
                await stream.DisposeAsync();
                throw;
            }
        }

        public async Task AddAsync(string filename, Image image)
        {
            var stream = new MemoryStream();

            try {
                await image.SaveAsPngAsync(stream);
                await stream.FlushAsync();
                Add(filename, stream);
            }
            catch {
                await stream.DisposeAsync();
                throw;
            }
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
