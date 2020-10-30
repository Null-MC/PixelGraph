using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.Internal
{
    public abstract class TestBase
    {
        protected ServiceCollection Services {get;}
        protected MockFileContent Content {get;}


        protected TestBase(ITestOutputHelper output)
        {
            Content = new MockFileContent();

            Services = new ServiceCollection();
            Services.AddSingleton(output);
            Services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
            Services.AddSingleton<ILogger, TestLogger>();
            Services.AddSingleton(Content);

            Services.AddSingleton<INamingStructure, JavaNamingStructure>();
            Services.AddSingleton<IInputReader, MockInputReader>();
            Services.AddSingleton<IOutputWriter, MockOutputWriter>();
            Services.AddSingleton<ITextureGraphBuilder, TextureGraphBuilder>();
            Services.AddSingleton<IFileLoader, FileLoader>();
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
