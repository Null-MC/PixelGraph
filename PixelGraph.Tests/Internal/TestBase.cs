using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using System;
using System.IO;
using System.Reflection;
using PixelGraph.Tests.Internal.Mocks;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal
{
    public abstract class TestBase
    {
        private static readonly Lazy<string> assemblyPathFunc;

        protected IServiceBuilder Builder {get;}
        protected string AssemblyPath => assemblyPathFunc.Value;


        static TestBase()
        {
            assemblyPathFunc = new Lazy<string>(GetAssemblyPath);
        }

        protected TestBase(ITestOutputHelper output)
        {
            Builder = new ServiceBuilder();

            Builder.Services.AddSingleton(output);
            Builder.Services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
            Builder.Services.AddSingleton<ILogger, TestLogger>();
            Builder.Services.AddSingleton<IInputReader, MockInputReader>();
            Builder.Services.AddSingleton<IOutputWriter, MockOutputWriter>();
            Builder.Services.AddSingleton<MockFileContent>();
        }

        private static string GetAssemblyPath()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(assembly.Location);
        }
    }
}
