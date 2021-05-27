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

        public static string Localize(string filename)
        {
            if (Path.DirectorySeparatorChar == '/') return filename;
            return filename.Replace('/', Path.DirectorySeparatorChar);
        }

        public static string Normalize(string filename)
        {
            return filename.Replace('\\', '/');
        }

        public static bool MatchPattern(string name, string pattern)
        {
            if (pattern == null || pattern == "*") return true;

            var regexPattern = Regex.Escape(pattern)
                .Replace("\\?", ".")
                .Replace("\\*", ".+");

            return Regex.IsMatch(name, $"^{regexPattern}$");
        }

        public static string[] Split(string path)
        {
            return path.Split('/', '\\');
        }

        //public static bool ContainsSegment(string path, params string[] findParts)
        //{
        //    if (findParts == null) throw new ArgumentNullException(nameof(findParts));

        //    var pathParts = path?.Split('/', '\\');
        //    if (pathParts == null) return false;

        //    for (var i = 0; i <= pathParts.Length; i++) {
        //        if (MatchesAt(pathParts, findParts, i))
        //            return true;
        //    }

        //    return false;
        //}

        //private static bool MatchesAt(IReadOnlyList<string> srcParts, IReadOnlyList<string> matchParts, int index)
        //{
        //    for (var i = 0; i < matchParts.Count; i++) {
        //        if (index + i >= srcParts.Count) return false;
        //        if (srcParts[index + i] != matchParts[i]) return false;
        //    }

        //    return true;
        //}
    }
}
