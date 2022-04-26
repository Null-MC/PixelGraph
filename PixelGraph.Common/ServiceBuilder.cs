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

        void Initialize();
        void ConfigureReader(ContentTypes contentType, GameEditions gameEdition, string rootPath);
        void ConfigureWriter(ContentTypes contentType, GameEditions gameEdition, string rootPath);
        void AddImporter(GameEditions gameEdition);
        void AddPublisher(GameEditions gameEdition);

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
            Services.AddOptions();

            Services.AddSingleton<CtmPublisher>();
            Services.AddSingleton<DefaultPublishMapping>();
            Services.AddSingleton<JavaToBedrockPublishMapping>();
            Services.AddSingleton<IPublishSummary, PublishSummary>();

            Services.AddScoped<GenericTexturePublisher>();

            Services.AddTransient<MinecraftResourceLocator>();
            Services.AddTransient<TextureRegionEnumerator>();
            Services.AddTransient<BedrockRtxGrassFixer>();
        }

        public virtual void Initialize()
        {
            Services.AddLogging(builder => builder.AddSerilog(LocalLogFile.FileLogger));

            Services.AddSingleton<IPublishReader, PublishReader>();

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

            Services.AddTransient<IMaterialReader, MaterialReader>();
            Services.AddTransient<IMaterialWriter, MaterialWriter>();
            Services.AddTransient<IResourcePackImporter, ResourcePackImporter>();
            Services.AddTransient<IItemTextureGenerator, ItemTextureGenerator>();
            Services.AddTransient<ITextureBuilder, TextureBuilder>();
            //Services.AddTransient<ProjectSerializer>();
        }

        public void ConfigureReader(ContentTypes contentType, GameEditions gameEdition, string rootPath)
        {
            AddContentReader(contentType);
            AddTextureReader(gameEdition);
            Services.Configure<InputOptions>(options => options.Root = rootPath);
        }

        public void ConfigureWriter(ContentTypes contentType, GameEditions gameEdition, string rootPath)
        {
            AddContentWriter(contentType);
            AddTextureWriter(gameEdition);
            Services.Configure<OutputOptions>(options => options.Root = rootPath);
        }

        protected virtual void AddContentReader(ContentTypes contentType)
        {
            switch (contentType) {
                case ContentTypes.File:
                    Services.AddSingleton<IInputReader, FileInputReader>();
                    break;
                case ContentTypes.Archive:
                    Services.AddSingleton<IInputReader, ArchiveInputReader>();
                    break;
            }
        }

        protected virtual void AddContentWriter(ContentTypes contentType)
        {
            switch (contentType) {
                case ContentTypes.File:
                    Services.AddSingleton<IOutputWriter, FileOutputWriter>();
                    break;
                case ContentTypes.Archive:
                    Services.AddSingleton<IOutputWriter, ArchiveOutputWriter>();
                    break;
            }
        }

        protected virtual void AddTextureReader(GameEditions gameEdition)
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

        protected virtual void AddTextureWriter(GameEditions gameEdition)
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

        public virtual void AddImporter(GameEditions gameEdition)
        {
            switch (gameEdition) {
                case GameEditions.Java:
                    Services.AddTransient<IMaterialImporter, JavaMaterialImporter>();
                    break;
                case GameEditions.Bedrock:
                    Services.AddTransient<IMaterialImporter, BedrockMaterialImporter>();
                    break;
            }
        }

        public virtual void AddPublisher(GameEditions gameEdition)
        {
            switch (gameEdition) {
                case GameEditions.Java:
                    Services.AddTransient<IPublisher, JavaPublisher>();
                    break;
                case GameEditions.Bedrock:
                    Services.AddTransient<IPublisher, BedrockPublisher>();
                    break;
            }
        }

        public ServiceProvider Build() => Services.BuildServiceProvider();
    }
}
