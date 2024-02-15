using MinecraftMappings.Minecraft.LegacyJavaToBedrockMappings;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;

namespace PixelGraph.Common.IO.Importing;

internal class BedrockToJavaImportMapping : PublisherMappingBase
{
    protected override IDictionary<string, string> OnBuildMappings()
    {
        var data = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (key, value) in LegacyBlockMappings.Instance) data[value] = key;
        foreach (var (key, value) in LegacyEntityMappings.Instance) data[value] = key;
        foreach (var (key, value) in LegacyItemMappings.Instance) data[value] = key;
        foreach (var (key, value) in LegacyOtherMappings.Instance) data[value] = key;
        return data;
    }

    public override bool Contains(string sourceFile)
    {
        return Mappings?.Keys.Any(sourceFile.EndsWith) ?? false;
    }

    public override bool TryMap(string sourcePath, string sourceName, out string? destPath, out string? destName)
    {
        if (Mappings != null) {
            var sourceFile = PathEx.Join(sourcePath, sourceName);
            sourceFile = PathEx.Normalize(sourceFile);

            if (sourceFile != null) {
                foreach (var mapping in Mappings) {
                    if (!sourceFile.EndsWith(mapping.Key)) continue;

                    destName = Path.GetFileName(mapping.Value);
                    destPath = Path.GetDirectoryName(mapping.Value);
                    return true;
                }
            }
        }

        destName = null;
        destPath = null;
        return false;
    }
}
