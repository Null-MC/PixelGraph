using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
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

        protected Image CreateImageR(in byte value)
        {
            var color = new Rgba32(value, 0, 0, 0);
            return new Image<Rgba32>(Configuration.Default, 1, 1, color);
        }

        protected Image CreateImageR(float value)
        {
            MathEx.SaturateFloor(value, out var pixelValue);
            return CreateImageR(in pixelValue);
        }

        protected Image CreateImage(in byte r, in byte g, in byte b)
        {
            var color = new Rgba32(r, g, b, 255);
            return new Image<Rgba32>(Configuration.Default, 1, 1, color);
        }
    }
}
