using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Projects;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;

namespace PixelGraph.Common.IO.Publishing;

public class JavaPublisher : PublisherBase
{
    private readonly CtmPublisher ctmPublish;


    public JavaPublisher(
        ILogger<JavaPublisher> logger,
        IServiceProvider provider,
        CtmPublisher ctmPublish) : base(logger, provider)
    {
        this.ctmPublish = ctmPublish;
    }

    protected override async Task PublishPackMetaAsync(PublishProfileProperties pack, CancellationToken token)
    {
        var packMeta = new JavaPackMetadata {
            PackFormat = pack.Format ?? PublishProfileProperties.DefaultJavaFormat,
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

        if (graphContext.Material?.PublishItem ?? false) {
            var graphBuilder = scopeProvider.GetRequiredService<IPublishGraphBuilder>();
            await graphBuilder.PublishInventoryAsync(token);
        }
    }
}