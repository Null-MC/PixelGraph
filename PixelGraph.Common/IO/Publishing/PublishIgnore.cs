namespace PixelGraph.Common.IO.Publishing;

internal static class PublishIgnore
{
    public static bool AllowFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return !fileIgnoreList.Contains(fileName);
    }

    public static bool AllowDirectory(string path)
    {
        if (path.EndsWith(".ignore", StringComparison.InvariantCultureIgnoreCase)) return false;

        var file = Path.GetFileName(path);
        return !folderIgnoreList.Contains(file);
    }

    private static readonly HashSet<string> fileIgnoreList = new(StringComparer.InvariantCultureIgnoreCase) {
        "project.yml",
        "input.yml",
        "source.txt",
        "readme.txt",
        "readme.md",
        "desktop.ini",
        "thumbs.db",
        ".gitignore",
        ".gitmodules",
    };

    private static readonly HashSet<string> folderIgnoreList = new(StringComparer.InvariantCultureIgnoreCase) {
        ".git",
        ".github",
    };
}
