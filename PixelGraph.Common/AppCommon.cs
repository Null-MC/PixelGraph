using System.Reflection;

namespace PixelGraph.Common;

public static class AppCommon
{
    public static string? Version {get;}


    static AppCommon()
    {
        Version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
    }
}
