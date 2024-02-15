using System.Text.RegularExpressions;

namespace PixelGraph.Common.Extensions;

public static class PathEx
{
    public static string Join(params string?[] pathParts)
    {
        var parts = pathParts.Where(p => p != null && p != ".").ToArray();
        return Path.Join(parts);
    }

    //public static string JoinNormalized(params string[] pathParts)
    //{
    //    var parts = pathParts.Where(p => p != null && p != ".").ToArray();
    //    return string.Join('/', parts);
    //}

    public static string Localize(string filename)
    {
        if (Path.DirectorySeparatorChar == '/') return filename;
        return filename.Replace('/', Path.DirectorySeparatorChar);
    }

    public static string? LocalizeNullable(string? filename)
    {
        if (Path.DirectorySeparatorChar == '/') return filename;
        return filename?.Replace('/', Path.DirectorySeparatorChar);
    }

    public static string Normalize(string filename)
    {
        return filename.Replace('\\', '/');
    }

    public static string? NormalizeNullable(string? filename)
    {
        return filename?.Replace('\\', '/');
    }

    public static bool MatchPattern(string name, string? pattern = null)
    {
        if (pattern is null or "*") return true;

        var regexPattern = Regex.Escape(pattern)
            .Replace("\\?", ".")
            .Replace("\\*", ".+");

        return Regex.IsMatch(name, $"^{regexPattern}$");
    }

    public static bool TryGetRelative(string rootPath, string fullPath, out string localPath)
    {
        if (!fullPath.StartsWith(rootPath, StringComparison.InvariantCultureIgnoreCase)) {
            localPath = fullPath;
            return false;
        }

        localPath = fullPath[rootPath.Length..].TrimStart('\\', '/');
        return true;
    }

    //public static string[] Split(string path)
    //{
    //    return path.Split('/', '\\');
    //}

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