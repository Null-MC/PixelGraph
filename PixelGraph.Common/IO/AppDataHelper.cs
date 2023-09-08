using System;
using System.IO;

namespace PixelGraph.Common.IO;

public static class AppDataHelper
{
    private const string AppName = "PixelGraph";

    public static string AppDataPath {get;}


    static AppDataHelper()
    {
        var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        AppDataPath = Path.Combine(rootPath, AppName);
    }
}