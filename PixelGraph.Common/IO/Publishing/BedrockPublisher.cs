using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing;

public class BedrockPublisher : PublisherBase
{
    public BedrockPublisher(
        ILogger<BedrockPublisher> logger,
        IServiceProvider provider) : base(logger, provider)
    {
        Mapping = new JavaToBedrockPublishMapping();
    }

    protected override async Task PublishPackMetaAsync(PublishProfileProperties pack, CancellationToken token)
    {
        var packMeta = new BedrockPackMetadata {
            FormatVersion = pack.Format ?? PublishProfileProperties.DefaultBedrockFormat,
            Header = {
                Name = pack.Name,
                Description = pack.Description,
                UniqueId = pack.HeaderUuid,
                Version = new [] {1, 0, 0},
                MinEngineVersion = new [] {1, 16, 0},
            },
            Modules = {
                new BedrockPackModuleMetadata {
                    UniqueId = pack.ModuleUuid,
                    Description = pack.Description,
                    Type = "resources",
                    Version = new [] {1, 0, 0},
                },
            },
        };

        var isRtx = TextureFormat.Is(pack.Encoding.Format, TextureFormat.Format_Rtx);
        if (isRtx) packMeta.Capabilities.Add("raytraced");

        await Writer.OpenWriteAsync("manifest.json", async stream => {
            await WriteJsonAsync(stream, packMeta, Formatting.Indented, token);
        }, token);
    }

    protected override async Task OnPackPublished(ProjectPublishContext context, CancellationToken token)
    {
        try {
            var grassFixer = Provider.GetRequiredService<BedrockRtxGrassFixer>();
            await grassFixer.FixAsync(context, token);

            Logger.LogInformation("Successfully generated grass_side texture!");
        }
        catch (Exception error) {
            Logger.LogError(error, $"Failed to generate grass_side texture! {error.UnfoldMessageString()}");
        }
    }

    protected override async Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token)
    {
        var context = scopeProvider.GetRequiredService<ITextureGraphContext>();

        if (context.OutputEncoding == null) return;
        if (context.IsMaterialCtm && !(context.Material.CTM.Placeholder ?? false)) return;

        var hasNormalMer = context.OutputEncoding.Any(e => TextureTags.Is(e.Texture, TextureTags.Normal) || TextureTags.Is(e.Texture, TextureTags.MER));
        if (!hasNormalMer) return;

        var sourcePath = context.Material.LocalPath;

        // Create *.texture_set.json file
        if (context.IsMaterialMultiPart) {
            foreach (var part in context.Material.Parts) {
                if (Mapping.TryMap(sourcePath, part.Name, out var destPath, out var destName))
                    await CreateMaterialMetadataAsync(context, destPath, destName, token);
            }
        }
        else if (context.IsMaterialCtm) {
            var localBlockPath = PathEx.Localize("assets/minecraft/textures/block");
            if (Mapping.TryMap(localBlockPath, context.Material.Name, out var destPath, out var destName))
                await CreateMaterialMetadataAsync(context, destPath, destName, token);
        }
        else {
            if (Mapping.TryMap(sourcePath, context.Material.Name, out var destPath, out var destName))
                await CreateMaterialMetadataAsync(context, destPath, destName, token);
        }
    }

    protected override bool TryMapFile(in string sourceFile, out string destinationFile)
    {
        return fileMap.TryGetValue(sourceFile, out destinationFile);
    }

    protected override bool TryMapMaterial(in MaterialProperties material)
    {
        if (Mapping == null) return true;

        var sourcePath = material.LocalPath;
        string sourceFile;

        if (material.Parts?.Any(part => {
                sourceFile = PathEx.Join(sourcePath, part.Name);
                sourceFile = PathEx.Normalize(sourceFile);

                return Mapping.Contains(sourceFile);
            }) ?? false) return true;

        if (material.CTM?.Method != null) {
            if (!(material.CTM.Placeholder ?? false)) return false;

            sourceFile = PathEx.Join("assets/minecraft/textures/block", material.Name);
            sourceFile = PathEx.Normalize(sourceFile);

            return Mapping.Contains(sourceFile);
        }

        sourceFile = PathEx.Join(sourcePath, material.Name);
        sourceFile = PathEx.Normalize(sourceFile);

        return Mapping.Contains(sourceFile);
    }

    private async Task CreateMaterialMetadataAsync(ITextureGraphContext context, string matPath, string matName, CancellationToken token)
    {
        var meta = new BedrockMaterialMetadata {
            FormatVersion = "1.16.100",
            TextureSet = {
                Color = $"{matName}",
                MER = $"{matName}_mer",
            },
        };

        if (context.OutputEncoding.HasNormalChannels())
            meta.TextureSet.Normal = $"{matName}_normal";
        else if (context.OutputEncoding.HasChannel(EncodingChannel.Height))
            meta.TextureSet.Height = $"{matName}_heightmap";

        var name = $"{matName}.texture_set.json";
        var localFile = PathEx.Join(matPath, name);
        await Writer.OpenWriteAsync(localFile, async stream => {
            await WriteJsonAsync(stream, meta, Formatting.Indented, token);
        }, token);
    }

    private static readonly Dictionary<string, string> fileMap = new(StringComparer.InvariantCultureIgnoreCase) {
        ["pack.png"] = "pack_icon.png",
        //...
    };
}