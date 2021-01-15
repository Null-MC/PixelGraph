using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.Extensions
{
    public static class PathEx
    {
        public static string Join(params string[] pathParts)
        {
            var parts = pathParts.Where(p => p != null && p != ".").ToArray();
            return Path.Join(parts);
        }

        public static string Normalize(string filename)
        {
            if (Path.DirectorySeparatorChar == '/') return filename;
            return filename.Replace('/', Path.DirectorySeparatorChar);
        }

        public static bool MatchPattern(string name, string pattern)
        {
            if (pattern == null || pattern == "*") return true;

            var regexPattern = Regex.Escape(pattern)
                .Replace("\\?", ".")
                .Replace("\\*", ".+");

            return Regex.IsMatch(name, $"^{regexPattern}$");
        }
    }
}
