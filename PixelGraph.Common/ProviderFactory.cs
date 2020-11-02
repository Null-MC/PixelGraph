using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.Publishing;
using PixelGraph.Common.Textures;

namespace PixelGraph.Common
{
    public abstract class ProviderFactory
    {


        public virtual ServiceProvider Build<TInput, TOutput>()
            where TInput : class, IInputReader
            where TOutput : class, IOutputWriter
        {
            var services = new ServiceCollection();

            services.AddSingleton<INamingStructure, JavaNamingStructure>();
            services.AddSingleton<ITextureGraphBuilder, TextureGraphBuilder>();
            services.AddSingleton<IPackReader, PackReader>();
            services.AddSingleton<IPbrReader, PbrReader>();
            services.AddSingleton<IPublisher, Publisher>();
            services.AddSingleton<IFileLoader, FileLoader>();

            services.AddSingleton<IInputReader, TInput>();
            services.AddSingleton<IOutputWriter, TOutput>();

            return services.BuildServiceProvider();
        }
    }
}
