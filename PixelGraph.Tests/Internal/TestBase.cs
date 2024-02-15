using PixelGraph.Common;
using PixelGraph.Tests.Internal.Mocks;
using System.Reflection;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal;

public abstract class TestBase
{
    private static readonly Lazy<string?> assemblyPathFunc;

    protected IServiceBuilder Builder {get;}
    protected ITestOutputHelper Output {get;}

    protected static string? AssemblyPath => assemblyPathFunc.Value;


    static TestBase()
    {
        assemblyPathFunc = new Lazy<string?>(GetAssemblyPath);
    }

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;

        Builder = new MockServiceBuilder(output);
        Builder.Initialize();
    }

    private static string? GetAssemblyPath()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return Path.GetDirectoryName(assembly.Location);
    }
}