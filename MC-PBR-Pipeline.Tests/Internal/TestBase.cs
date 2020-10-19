using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.Internal
{
    public abstract class TestBase
    {
        protected ServiceCollection Services {get;}
        protected MockFileContent Content {get;}


        protected TestBase(ITestOutputHelper output)
        {
            Services = new ServiceCollection();
            Services.AddSingleton(output);
            Services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
            Services.AddSingleton<ILogger, TestLogger>();

            Content = new MockFileContent();
        }
    }
}
