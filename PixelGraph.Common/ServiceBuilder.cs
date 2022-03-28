using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Effects;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using Serilog;

namespace PixelGraph.Common
{
    public interface IServiceBuilder
    {
        ServiceCollection Services {get;}
        //ContentTypes InputContentType {get; set;}
        //ContentTypes OutputContentType {get; set;}

        void AddContentReader(ContentTypes contentType);
        void AddContentWriter(ContentTypes contentType);
        void AddTextureReader(GameEditions gameEdition);
        void AddTextureWriter(GameEditions gameEdition);
        void AddImporter(GameEditions gameEdition);

        ServiceProvider Build();
    }

    public enum ContentTypes
    {
        //None,
        File,
        Archive,
    }

    public enum GameEditions
    {
        None,
        Java,
        Bedrock,
    }

    public class ServiceBuilder : IServiceBuilder
    {
        public ServiceCollection Services {get;}


        public ServiceBuilder()
        {
            Services = new ServiceCollection();
            Services.AddLogging(builder => builder.AddSerilog(LocalLogFile.FileLogger));

            Services.AddSingleton<IResourcePackReader, ResourcePackReader>();
            Services.AddSingleton<IResourcePackWriter, ResourcePackWriter>();
            Services.AddSingleton<IMaterialReader, MaterialReader>();
            Services.AddSingleton<IMaterialWriter, MaterialWriter>();
            Services.AddSingleton<IJavaPublisher, JavaPublisher>();
            Services.AddSingleton<IBedrockPublisher, BedrockPublisher>();
            Services.AddSingleton<IPublishReader, PublishReader>();
            Services.AddSingleton<IDefaultPublishMapping, DefaultPublishMapping>();
            Services.AddSingleton<IJavaToBedrockPublishMapping, JavaToBedrockPublishMapping>();
            Services.AddSingleton<IMinecraftResourceLocator, MinecraftResourceLocator>();
            Services.AddSingleton<ICtmPublisher, CtmPublisher>();

            Services.AddScoped<ITextureGraphContext, TextureGraphContext>();
            Services.AddScoped<ITextureGraph, TextureGraph>();
            Services.AddScoped<ITextureSourceGraph, TextureSourceGraph>();
            Services.AddScoped<ITextureHeightGraph, TextureHeightGraph>();
            Services.AddScoped<ITextureNormalGraph, TextureNormalGraph>();
            Services.AddScoped<ITextureOcclusionGraph, TextureOcclusionGraph>();
            Services.AddScoped<IImportGraphBuilder, ImportGraphBuilder>();
            Services.AddScoped<IPublishGraphBuilder, PublishGraphBuilder>();
            Services.AddScoped<IEdgeFadeImageEffect, EdgeFadeImageEffect>();
            Services.AddScoped<IImageWriter, ImageWriter>();

            //Services.AddScoped<IMaterialInputReader, RawMaterialInputReader>();

            Services.AddTransient<IResourcePackImporter, ResourcePackImporter>();
            Services.AddTransient<IItemTextureGenerator, ItemTextureGenerator>();
            Services.AddTransient<ITextureBuilder, TextureBuilder>();
            Services.AddTransient<ITextureRegionEnumerator, TextureRegionEnumerator>();

            //Services.AddTransient<IMaterialImporter, MaterialImporterBase>();

            Services.AddTransient<BedrockRtxGrassFixer>();
        }

        public void AddContentReader(ContentTypes contentType)
        {
            switch (contentType) {
                case ContentTypes.File:
                    Services.AddSingleton<IInputReader, FileInputReader>();
                    break;
                case ContentTypes.Archive:
                    Services.AddSingleton<IInputReader, ArchiveInputReader>();
                    break;
                //case ContentTypes.None:
                //default:
                //    throw new ApplicationException("Content input type is undefined!");
            }
        }

        public void AddContentWriter(ContentTypes contentType)
        {
            switch (contentType) {
                case ContentTypes.File:
                    Services.AddSingleton<IOutputWriter, FileOutputWriter>();
                    break;
                case ContentTypes.Archive:
                    Services.AddSingleton<IOutputWriter, ArchiveOutputWriter>();
                    break;
                //case ContentTypes.None:
                //default:
                //    throw new ApplicationException("Content output type is undefined!");
            }
        }

        public void AddTextureReader(GameEditions gameEdition)
        {
            switch (gameEdition) {
                case GameEditions.Java:
                    Services.AddTransient<ITextureReader, JavaTextureReader>();
                    break;
                case GameEditions.Bedrock:
                    Services.AddTransient<ITextureReader, BedrockTextureReader>();
                    break;
                case GameEditions.None:
                    Services.AddTransient<ITextureReader, RawTextureReader>();
                    break;
            }
        }

        public void AddTextureWriter(GameEditions gameEdition)
        {
            switch (gameEdition) {
                case GameEditions.Java:
                    Services.AddTransient<ITextureWriter, JavaTextureWriter>();
                    break;
                case GameEditions.Bedrock:
                    Services.AddTransient<ITextureWriter, BedrockTextureWriter>();
                    break;
                case GameEditions.None:
                    Services.AddTransient<ITextureWriter, RawTextureWriter>();
                    break;
            }
        }

        public void AddImporter(GameEditions gameEdition)
        {
            switch (gameEdition) {
                case GameEditions.Java:
                    Services.AddTransient<IMaterialImporter, JavaMaterialImporter>();
                    break;
                case GameEditions.Bedrock:
                    Services.AddTransient<IMaterialImporter, BedrockMaterialImporter>();
                    break;
                //case GameEditions.None:
                //default:
                //    throw new ApplicationException("Game edition is undefined!");
            }
        }

        public ServiceProvider Build() => Services.BuildServiceProvider();
    }
}
