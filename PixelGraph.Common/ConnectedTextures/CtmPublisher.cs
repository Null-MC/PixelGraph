using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures.Graphing;

namespace PixelGraph.Common.ConnectedTextures;

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
        ArgumentNullException.ThrowIfNull(context.Material);

        var ctmData = context.Material.CTM;
        if (ctmData == null || string.IsNullOrWhiteSpace(ctmData.Method)) return;

        var propsFileIn = NamingStructure.GetInputPropertiesName(context.Material);
        var properties = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase);
        var propertySerializer = new PropertyFileSerializer();

        if (reader.FileExists(propsFileIn)) {
            await using var sourceStream = reader.Open(propsFileIn)
                ?? throw new ApplicationException("Failed to open file stream!");

            using var streamReader = new StreamReader(sourceStream);

            await foreach (var (propertyName, propertyValue) in propertySerializer.ReadAsync(streamReader, token))
                properties[propertyName] = propertyValue;
        }
            
        if (!optifineWriteMap.TryGetValue(ctmData.Method, out var ctmWriteMethod))
            throw new ApplicationException($"Unable to map CTM type '{ctmData.Method}'!");

        properties["method"] = ctmWriteMethod;

        if (ctmData.MatchBlocks != null)
            properties["matchBlocks"] = ctmData.MatchBlocks;

        if (ctmData.MatchTiles != null)
            properties["matchTiles"] = ctmData.MatchTiles;

        if (!properties.ContainsKey("matchBlocks") && !properties.ContainsKey("matchTiles"))
            properties["matchTiles"] = context.Material.Name;

        if (CtmTypes.IsRepeatType(ctmData.Method)) {
            if (ctmData.Width.HasValue || !properties.ContainsKey("width"))
                properties["width"] = (ctmData.Width ?? 1).ToString("N0");

            if (ctmData.Height.HasValue || !properties.ContainsKey("height"))
                properties["height"] = (ctmData.Height ?? 1).ToString("N0");
        }

        if (!properties.ContainsKey("tiles")) {
            var minTile = ctmData.TileStartIndex ?? context.Profile?.TileStartIndex ?? 1;

            var hasPlaceholder = ctmData.Placeholder ?? false;
            var tileCount = CtmTypes.GetBounds(ctmData)?.Total ?? 1;
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