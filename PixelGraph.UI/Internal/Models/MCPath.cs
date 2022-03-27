using PixelGraph.Common.Extensions;
using System;
using System.Text.RegularExpressions;

namespace PixelGraph.UI.Internal.Models
{
    internal static class MCPath
    {
        private static readonly Regex entityPathExp = new(@"assets\/[\w-_.]+\/textures\/(?:entity|models)(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex blockPathExp = new(@"assets\/[\w-_.]+\/textures\/block(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex itemPathExp = new(@"assets\/[\w-_.]+\/textures\/item(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public static bool IsEntityPath(string materialPath)
        {
            if (materialPath == null) throw new ArgumentNullException(nameof(materialPath));

            return entityPathExp.IsMatch(PathEx.Normalize(materialPath));
        }

        public static bool IsBlockPath(string materialPath)
        {
            if (materialPath == null) throw new ArgumentNullException(nameof(materialPath));

            return blockPathExp.IsMatch(PathEx.Normalize(materialPath));
        }

        public static bool IsItemPath(string materialPath)
        {
            if (materialPath == null) throw new ArgumentNullException(nameof(materialPath));

            return itemPathExp.IsMatch(PathEx.Normalize(materialPath));
        }
    }
}
