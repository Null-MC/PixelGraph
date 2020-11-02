using System.IO;
using System.Linq;

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
    }
}
