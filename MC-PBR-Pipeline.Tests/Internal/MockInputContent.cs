using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace McPbrPipeline.Tests.Internal
{
    internal class MockInputContent
    {
        public List<string> Files {get; set;}


        public MockInputContent()
        {
            Files = new List<string>();
        }

        public IEnumerable<string> EnumerateDirectories(string path, string pattern)
        {
            var subdirectories = Files.Select(Path.GetDirectoryName)
                .Where(d => d.StartsWith(path, StringComparison.InvariantCultureIgnoreCase)).Distinct();

            foreach (var subdirectory in subdirectories) {
                var subPath = subdirectory[path.Length..].TrimStart(Path.DirectorySeparatorChar);
                if (string.IsNullOrEmpty(subPath)) continue;

                var i = subPath.IndexOf(Path.DirectorySeparatorChar);
                yield return i < 0 ? subPath : subPath[..i];
            }
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern)
        {
            return Files.Where(f => string.Equals(Path.GetDirectoryName(f), path, StringComparison.InvariantCultureIgnoreCase))
                .Where(d => Match(Path.GetFileName(d), pattern));
        }

        public bool FileExists(string filename)
        {
            return Files.Contains(filename, StringComparer.InvariantCultureIgnoreCase);
        }

        private static bool Match(string name, string pattern)
        {
            if (pattern == null || pattern == "*") return true;

            var regexPattern = Regex.Escape(pattern)
                .Replace("\\?", ".")
                .Replace("\\*", ".+");

            return Regex.IsMatch(name, regexPattern);
        }
    }
}
