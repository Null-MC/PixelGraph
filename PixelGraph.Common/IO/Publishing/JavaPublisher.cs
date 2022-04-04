using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
    //public interface IJavaPublisher : IPublisher {}

    public class JavaPublisher : PublisherBase //, IJavaPublisher
    {
        private readonly CtmPublisher ctmPublish;


        public JavaPublisher(
            ILogger<JavaPublisher> logger,
            IServiceProvider provider,
            IPublishReader loader,
            IInputReader reader,
            IOutputWriter writer,
            CtmPublisher ctmPublish) : base(logger, provider, loader, reader, writer)
        {
            this.ctmPublish = ctmPublish;
        }

        protected override async Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token)
        {
            var packMeta = new JavaPackMetadata {
                PackFormat = pack.Format ?? ResourcePackProfileProperties.DefaultJavaFormat,
                Description = pack.Description ?? string.Empty,
            };

            if (pack.Tags != null) {
                packMeta.Description += $"\n{string.Join(' ', pack.Tags)}";
            }

            var data = new {pack = packMeta};
            await Writer.OpenWriteAsync("pack.mcmeta", async stream => {
                await WriteJsonAsync(stream, data, Formatting.Indented, token);
            }, token);
        }

        protected override async Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token)
        {
            var graphContext = scopeProvider.GetRequiredService<ITextureGraphContext>();
            await ctmPublish.TryBuildPropertiesAsync(graphContext, token);

            if (graphContext.Material.PublishItem ?? false) {
                var graphBuilder = scopeProvider.GetRequiredService<IPublishGraphBuilder>();
                await graphBuilder.PublishInventoryAsync(token);
            }
        }
    }
}
