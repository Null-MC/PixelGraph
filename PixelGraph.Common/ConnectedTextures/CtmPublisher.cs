using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures.Graphing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.ConnectedTextures
{
    public class CtmPublisher
    {
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;


        public CtmPublisher(
            IInputReader reader,
            IOutputWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }

        public async Task TryBuildPropertiesAsync(ITextureGraphContext context, CancellationToken token = default)
        {
            var method = context.Material.CTM?.Method;
            if (string.IsNullOrWhiteSpace(method)) return;

            var propsFileIn = NamingStructure.GetInputPropertiesName(context.Material);
            var properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var propertySerializer = new PropertyFileSerializer();

            if (reader.FileExists(propsFileIn)) {
                await using var sourceStream = reader.Open(propsFileIn);
                using var streamReader = new StreamReader(sourceStream);

                await foreach (var (propertyName, propertyValue) in propertySerializer.ReadAsync(streamReader, token))
                    properties[propertyName] = propertyValue;
            }
            
            if (!optifineWriteMap.TryGetValue(method, out var ctmWriteMethod))
                throw new ApplicationException($"Unable to map CTM type '{context.Material.CTM.Method}'!");

            properties["method"] = ctmWriteMethod;

            if (context.Material.CTM.MatchBlocks != null)
                properties["matchBlocks"] = context.Material.CTM.MatchBlocks;

            if (context.Material.CTM.MatchTiles != null)
                properties["matchTiles"] = context.Material.CTM.MatchTiles;

            if (!properties.ContainsKey("matchBlocks") && !properties.ContainsKey("matchTiles"))
                properties["matchTiles"] = context.Material.Name;

            if (CtmTypes.IsRepeatType(method)) {
                if (context.Material.CTM.Width.HasValue || !properties.ContainsKey("width"))
                    properties["width"] = (context.Material.CTM.Width ?? 1).ToString("N0");

                if (context.Material.CTM.Height.HasValue || !properties.ContainsKey("height"))
                    properties["height"] = (context.Material.CTM.Height ?? 1).ToString("N0");
            }

            if (!properties.ContainsKey("tiles")) {
                var minTile = context.Material?.CTM?.TileStartIndex ??
                              context.Profile?.TileStartIndex ?? 1;

                var hasPlaceholder = context.Material.CTM?.Placeholder ?? false;
                var tileCount = CtmTypes.GetBounds(context.Material.CTM)?.Total ?? 1;
                var maxTile = minTile + tileCount - 1;
                if (hasPlaceholder) minTile++;

                var result = $"{minTile:N0}";
                if (maxTile > minTile) result += $"-{maxTile:N0}";
                if (hasPlaceholder) result = $"textures/block/{context.Material.Name} {result}";
                properties["tiles"] = result;
            }

            var propsFileOut = NamingStructure.GetOutputPropertiesName(context.Material, true);
            await writer.OpenWriteAsync(propsFileOut, async stream => {
                await using var streamWriter = new StreamWriter(stream, leaveOpen: true);
                await propertySerializer.WriteAsync(streamWriter, properties, token);
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
