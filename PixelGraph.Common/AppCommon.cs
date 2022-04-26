using System.Reflection;

namespace PixelGraph.Common
{
    internal static class AppCommon
    {
        public static string Version {get;}


        static AppCommon()
        {
            Version = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
        }
    }
}
