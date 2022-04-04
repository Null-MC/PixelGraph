using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Effects;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal.Mocks
{
    internal class MockServiceBuilder : ServiceBuilder
    {
        private readonly ITestOutputHelper output;


        public MockServiceBuilder(ITestOutputHelper output)
        {
            this.output = output;
        }

        public override void Initialize()
        {
            Services.AddSingleton(output);
            Services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
            Services.AddSingleton<ILogger, TestLogger>();
            Services.AddSingleton<MockFileContent>();

            Services.AddTransient<IServiceBuilder, MockServiceBuilder>();

            Services.AddSingleton<IResourcePackReader, ResourcePackReader>();
            Services.AddSingleton<IResourcePackWriter, ResourcePackWriter>();
            Services.AddSingleton<IMaterialReader, MaterialReader>();
            Services.AddSingleton<IMaterialWriter, MaterialWriter>();
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

            Services.AddTransient<IResourcePackImporter, ResourcePackImporter>();
            Services.AddTransient<IItemTextureGenerator, ItemTextureGenerator>();
            Services.AddTransient<ITextureBuilder, TextureBuilder>();
        }

        protected override void AddContentReader(ContentTypes contentType)
        {
            Services.AddSingleton<IInputReader, MockInputReader>();
        }

        protected override void AddContentWriter(ContentTypes contentType)
        {
            Services.AddSingleton<IOutputWriter, MockOutputWriter>();
        }
    }
}
