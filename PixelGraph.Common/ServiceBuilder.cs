using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Effects;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
using Serilog;

namespace PixelGraph.Common
{
    public interface IServiceBuilder
    {
        ServiceCollection Services {get;}

        void AddFileInput();
        void AddFileOutput();
        void AddArchiveInput();
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
            Services.AddSingleton<IResourcePackReader, ResourcePackReader>();
            Services.AddSingleton<IResourcePackWriter, ResourcePackWriter>();
            Services.AddSingleton<IMaterialReader, MaterialReader>();
            Services.AddSingleton<IMaterialWriter, MaterialWriter>();
            Services.AddSingleton<IPublisher, Publisher>();
            Services.AddSingleton<IImageWriter, ImageWriter>();
            Services.AddSingleton<IFileLoader, FileLoader>();

            Services.AddScoped<ITextureGraphContext, TextureGraphContext>();
            Services.AddScoped<ITextureGraph, TextureGraph>();
            Services.AddScoped<ITextureSourceGraph, TextureSourceGraph>();
            Services.AddScoped<ITextureNormalGraph, TextureNormalGraph>();
            Services.AddScoped<ITextureOcclusionGraph, TextureOcclusionGraph>();
            Services.AddScoped<ITextureGraphBuilder, TextureGraphBuilder>();
            Services.AddScoped<ITextureRegionEnumerator, TextureRegionEnumerator>();
            Services.AddScoped<IEdgeFadeImageEffect, EdgeFadeImageEffect>();

            Services.AddTransient<IResourcePackImporter, ResourcePackImporter>();
            Services.AddTransient<IMaterialImporter, MaterialImporter>();
            Services.AddTransient<IInventoryTextureGenerator, InventoryTextureGenerator>();
            Services.AddTransient<ITextureBuilder, TextureBuilder>();
        }

        public void AddFileInput()
        {
            Services.AddSingleton<IInputReader, FileInputReader>();
        }

        public void AddFileOutput()
        {
            Services.AddSingleton<IOutputWriter, FileOutputWriter>();
        }

        public void AddArchiveInput()
        {
            Services.AddSingleton<IInputReader, ArchiveInputReader>();
        }

        public void AddArchiveOutput()
        {
            Services.AddSingleton<IOutputWriter, ArchiveOutputWriter>();
        }

        //public void AddLogging<T, TT>() where T : class, ILogger
        //{
        //    Services.AddSingleton(typeof(ILogger<>), typeof(TT));
        //    Services.AddSingleton<ILogger, T>();
        //}

        public ServiceProvider Build() => Services.BuildServiceProvider();
    }
}
