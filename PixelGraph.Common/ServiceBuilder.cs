using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.Publishing;
using PixelGraph.Common.Textures;
using Serilog;

namespace PixelGraph.Common
{
    public interface IServiceBuilder
    {
        ServiceCollection Services {get;}

        void AddFileInput();
        void AddFileOutput();
        void AddArchiveOutput();
        ServiceProvider Build();
    }

    public class ServiceBuilder : IServiceBuilder
    {
        public ServiceCollection Services {get;}


        public ServiceBuilder()
        {
            Services = new ServiceCollection();
            Services.AddLogging(builder => builder.AddSerilog());
            Services.AddSingleton<INamingStructure, JavaNamingStructure>();
            Services.AddSingleton<ITextureGraphBuilder, TextureGraphBuilder>();
            Services.AddSingleton<IPackReader, PackReader>();
            Services.AddSingleton<IPbrReader, PbrReader>();
            Services.AddSingleton<IPropertyWriter, PropertySerializer>();
            Services.AddSingleton<IPublisher, Publisher>();

            Services.AddTransient<IFileLoader, FileLoader>();
        }

        public void AddFileInput()
        {
            Services.AddSingleton<IInputReader, FileInputReader>();
        }

        public void AddFileOutput()
        {
            Services.AddSingleton<IOutputWriter, FileOutputWriter>();
        }

        public void AddArchiveOutput()
        {
            Services.AddSingleton<IOutputWriter, ArchiveOutputWriter>();
        }

        public ServiceProvider Build() => Services.BuildServiceProvider();
    }
}
