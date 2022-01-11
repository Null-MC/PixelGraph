using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.IO.Serialization;
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
            await Writer.OpenAsync("pack.mcmeta", async stream => {
                await WriteJsonAsync(stream, data, Formatting.Indented, token);
            }, token);
        }

        protected override async Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token)
        {
            var graphContext = scopeProvider.GetRequiredService<ITextureGraphContext>();
            await BuildCtmPropertiesAsync(graphContext, token);

            if (graphContext.Material.PublishItem ?? false) {
                var graphBuilder = scopeProvider.GetRequiredService<IPublishGraphBuilder>();
                await graphBuilder.PublishInventoryAsync(token);
            }
        }

        private async Task BuildCtmPropertiesAsync(ITextureGraphContext context, CancellationToken token)
        {
            var propsFileIn = NamingStructure.GetInputPropertiesName(context.Material);
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

            if (context.Material.CTM?.Method != null) {
                if (!optifineWriteMap.TryGetValue(context.Material.CTM.Method, out var ctmWriteMethod))
                    throw new ApplicationException($"Unable to map CTM type '{context.Material.CTM.Method}'!");

                properties["method"] = ctmWriteMethod;

                if (context.Material.CTM.Width.HasValue || !properties.ContainsKey("width"))
                    properties["width"] = (context.Material.CTM.Width ?? 1).ToString("N0");

                if (context.Material.CTM.Height.HasValue || !properties.ContainsKey("height"))
                    properties["height"] = (context.Material.CTM.Height ?? 1).ToString("N0");

                if (context.Material.CTM.MatchBlocks != null)
                    properties["matchBlocks"] = context.Material.CTM.MatchBlocks;

                if (context.Material.CTM.MatchTiles != null)
                    properties["matchTiles"] = context.Material.CTM.MatchTiles;

                if (!properties.ContainsKey("tiles")) {
                    var minTile = context.Profile?.TileStartIndex ??
                                  context.Material?.CTM?.TileStartIndex ?? 1;

                    var hasPlaceholder = context.Material.CTM?.Placeholder ?? false;
                    var tileCount = CtmTypes.GetBounds(context.Material.CTM)?.Total ?? 1;
                    var maxTile = minTile + tileCount - 1;
                    if (hasPlaceholder) minTile++;

                    var result = $"{minTile:N0}{maxTile:N0}";
                    if (hasPlaceholder) result = $"<default> {result}";
                    properties["tiles"] = result;
                }

                hasProperties = true;
            }

            if (!hasProperties) return;

            var propsFileOut = NamingStructure.GetOutputPropertiesName(context.Material, true);
            await Writer.OpenAsync(propsFileOut, async stream => {
                await using var writer = new StreamWriter(stream);
                await propertySerializer.WriteAsync(writer, properties, token);
            }, token);
        }

        private static readonly Dictionary<string, string> optifineWriteMap = new(StringComparer.InvariantCultureIgnoreCase) {
            [CtmTypes.Optifine_Full] = "ctm",
            [CtmTypes.Optifine_Compact] = "ctm_compact",
            [CtmTypes.Optifine_Horizontal] = "horizontal",
            [CtmTypes.Optifine_Vertical] = "vertical",
            [CtmTypes.Optifine_HorizontalVertical] = "horizontal+vertical",
            [CtmTypes.Optifine_VerticalHorizontal] = "vertical+horizontal",
            [CtmTypes.Optifine_Top] = "top",
            [CtmTypes.Optifine_Random] = "random",
            [CtmTypes.Optifine_Repeat] = "repeat",
            [CtmTypes.Optifine_Fixed] = "fixed",
            [CtmTypes.Optifine_Overlay] = "overlay",
            [CtmTypes.Optifine_OverlayFull] = "overlay_ctm",
            [CtmTypes.Optifine_OverlayRandom] = "overlay_random",
            [CtmTypes.Optifine_OverlayRepeat] = "overlay_repeat",
            [CtmTypes.Optifine_OverlayFixed] = "overlay_fixed",
        };
    }
}
