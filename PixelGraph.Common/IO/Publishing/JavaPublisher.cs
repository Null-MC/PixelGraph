using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using System;
using System.Collections.Generic;
using System.IO;
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
            var graphBuilder = scopeProvider.GetRequiredService<IPublishGraphBuilder>();

            var ext = NamingStructure.GetExtension(graphContext.Profile);
            await graphBuilder.PublishInventoryAsync($"_inventory.{ext}", token);

            await BuildCtmPropertiesAsync(graphContext.Material, token);
        }

        private async Task BuildCtmPropertiesAsync(MaterialProperties material, CancellationToken token)
        {
            var propsFileIn = NamingStructure.GetInputPropertiesName(material);
            var properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var propertySerializer = new PropertyFileSerializer();
            var hasProperties = false;

            if (Reader.FileExists(propsFileIn)) {
                await using var sourceStream = Reader.Open(propsFileIn);
                using var reader = new StreamReader(sourceStream);

                await foreach (var (propertyName, propertyValue) in propertySerializer.ReadAsync(reader, token))
                    properties[propertyName] = propertyValue;

                hasProperties = true;
            }

            if (material.CTM?.Method != null) {
                properties["method"] = material.CTM.Method;

                if (material.CTM.Width.HasValue || !properties.ContainsKey("width"))
                    properties["width"] = (material.CTM.Width ?? 1).ToString("N0");

                if (material.CTM.Height.HasValue || !properties.ContainsKey("height"))
                    properties["height"] = (material.CTM.Height ?? 1).ToString("N0");

                if (material.CTM.MatchBlocks != null)
                    properties["matchBlocks"] = material.CTM.MatchBlocks;

                if (material.CTM.MatchTiles != null)
                    properties["matchTiles"] = material.CTM.MatchTiles;

                if (!properties.ContainsKey("tiles")) {
                    var tileCount = CtmTypes.GetBounds(material.CTM)?.Total ?? 1;
                    properties["tiles"] = $"0-{tileCount-1:N0}";
                }

                hasProperties = true;
            }

            if (!hasProperties) return;

            var propsFileOut = NamingStructure.GetOutputPropertiesName(material, true);
            await using var destStream = Writer.Open(propsFileOut);
            await using var writer = new StreamWriter(destStream);
            await propertySerializer.WriteAsync(writer, properties, token);
        }
    }
}
