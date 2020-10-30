using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Publishing;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace McPbrPipeline.Internal
{
    internal class ProviderFactory
    {
        private readonly IAppLifetime lifetime;


        public ProviderFactory(IAppLifetime lifetime)
        {
            this.lifetime = lifetime;
        }

        public ServiceProvider Build(bool zip)
        {
            var commandServices = new ServiceCollection();
            commandServices.AddLogging(builder => builder.AddSerilog());
            commandServices.AddSingleton(lifetime);

            commandServices.AddSingleton<IInputReader, FileInputReader>();
            commandServices.AddSingleton<INamingStructure, JavaNamingStructure>();
            commandServices.AddSingleton<IPublisher, Publisher>();
            commandServices.AddSingleton<ITextureGraphBuilder, TextureGraphBuilder>();
            commandServices.AddSingleton<IFileLoader, FileLoader>();

            if (zip) commandServices.AddSingleton<IOutputWriter, ArchiveOutputWriter>();
            else commandServices.AddSingleton<IOutputWriter, FileOutputWriter>();

            return commandServices.BuildServiceProvider();
        }
    }
}
