using System.IO;
using System.Linq;

namespace McPbrPipeline.Internal.Extensions
{
    internal static class PathEx
    {
        public static string Join(params string[] pathParts)
        {
            var parts = pathParts.Where(p => p != null && p != ".").ToArray();
            return Path.Join(parts);
        }
    }
}
