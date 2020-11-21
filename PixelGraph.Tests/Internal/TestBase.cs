using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal
{
    public abstract class TestBase
    {
        protected IServiceBuilder Builder {get;}
        //protected MockFileContent Content {get;}


        protected TestBase(ITestOutputHelper output)
        {
            //Content = new MockFileContent();
            Builder = new ServiceBuilder();

            Builder.Services.AddSingleton(output);
            //Builder.Services.AddSingleton(Content);
            Builder.Services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
            Builder.Services.AddSingleton<ILogger, TestLogger>();
            Builder.Services.AddSingleton<IInputReader, MockInputReader>();
            Builder.Services.AddSingleton<IOutputWriter, MockOutputWriter>();
            Builder.Services.AddSingleton<MockFileContent>();
        }

        protected Image CreateImageR(byte value)
        {
            var color = new Rgba32(value, 0, 0, 0);
            return new Image<Rgba32>(Configuration.Default, 1, 1, color);
        }

        protected Image CreateImage(byte r, byte g, byte b)
        {
            var color = new Rgba32(r, g, b, 255);
            return new Image<Rgba32>(Configuration.Default, 1, 1, color);
        }
    }
}
