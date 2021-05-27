using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IJavaPublisher : IPublisher {}

    internal class JavaPublisher : PublisherBase<IDefaultPublishMapping>, IJavaPublisher
    {
        public JavaPublisher(
            ILogger<JavaPublisher> logger,
            IServiceProvider provider,
            IPublishReader loader,
            IInputReader reader,
            IOutputWriter writer,
            IDefaultPublishMapping mapping) : base(logger, provider, loader, reader, writer, mapping) {}

        protected override Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token)
        {
            var packMeta = new JavaPackMetadata {
                PackFormat = pack.Format ?? ResourcePackProfileProperties.DefaultJavaFormat,
                Description = pack.Description ?? string.Empty,
            };

            if (pack.Tags != null) {
                packMeta.Description += $"\n{string.Join(' ', pack.Tags)}";
            }

            var data = new {pack = packMeta};
            using var stream = Writer.Open("pack.mcmeta");
            return WriteJsonAsync(stream, data, Formatting.Indented, token);
        }

        protected override async Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token)
        {
            var graphContext = scopeProvider.GetRequiredService<ITextureGraphContext>();
            var graphBuilder = scopeProvider.GetRequiredService<ITextureGraphBuilder>();

            var ext = NamingStructure.GetExtension(graphContext.Profile);
            await graphBuilder.PublishInventoryAsync($"_inventory.{ext}", token);
        }
    }
}
