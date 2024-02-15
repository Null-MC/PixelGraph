using PixelGraph.Common.Extensions;
using System.Text.RegularExpressions;

namespace PixelGraph.UI.Internal.IO.Models;

internal static class MinecraftPath
{
    private static readonly Regex entityPathExp = new(@"assets\/[\w-_.]+\/textures\/(?:entity|models)(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex blockPathExp = new(@"assets\/[\w-_.]+\/textures\/block(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex itemPathExp = new(@"assets\/[\w-_.]+\/textures\/item(?:$|\/)", RegexOptions.IgnoreCase | RegexOptions.Compiled);


    public static bool IsEntityPath(string materialPath)
    {
        ArgumentNullException.ThrowIfNull(materialPath);

        return entityPathExp.IsMatch(PathEx.Normalize(materialPath));
    }

    public static bool IsBlockPath(string materialPath)
    {
        ArgumentNullException.ThrowIfNull(materialPath);

        return blockPathExp.IsMatch(PathEx.Normalize(materialPath));
    }

    public static bool IsItemPath(string materialPath)
    {
        ArgumentNullException.ThrowIfNull(materialPath);

        return itemPathExp.IsMatch(PathEx.Normalize(materialPath));
    }
}
