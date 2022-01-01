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
            await BuildCtmPropertiesAsync(graphContext.Material, token);

            if (graphContext.Material.PublishItem ?? false) {
                var graphBuilder = scopeProvider.GetRequiredService<IPublishGraphBuilder>();
                await graphBuilder.PublishInventoryAsync(token);
            }
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
                if (!optifineWriteMap.TryGetValue(material.CTM.Method, out var ctmWriteMethod))
                    throw new ApplicationException($"Unable to map CTM type '{material.CTM.Method}'!");

                properties["method"] = ctmWriteMethod;

                if (material.CTM.Width.HasValue || !properties.ContainsKey("width"))
                    properties["width"] = (material.CTM.Width ?? 1).ToString("N0");

                if (material.CTM.Height.HasValue || !properties.ContainsKey("height"))
                    properties["height"] = (material.CTM.Height ?? 1).ToString("N0");

                if (material.CTM.MatchBlocks != null)
                    properties["matchBlocks"] = material.CTM.MatchBlocks;

                if (material.CTM.MatchTiles != null)
                    properties["matchTiles"] = material.CTM.MatchTiles;

                if (!properties.ContainsKey("tiles")) {
                    var hasPlaceholder = material.CTM?.Placeholder ?? false;
                    var minTile = hasPlaceholder ? "2" : "1";
                    var tileCount = CtmTypes.GetBounds(material.CTM)?.Total ?? 1;
                    var maxTile = tileCount > 1 ? $"-{tileCount:N0}" : "";

                    if (hasPlaceholder) minTile = $"<default> {minTile}";

                    properties["tiles"] = $"{minTile}{maxTile}";
                }

                hasProperties = true;
            }

            if (!hasProperties) return;

            var propsFileOut = NamingStructure.GetOutputPropertiesName(material, true);
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
