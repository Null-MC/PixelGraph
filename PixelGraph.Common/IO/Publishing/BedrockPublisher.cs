using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.Textures.Graphing;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IBedrockPublisher : IPublisher {}

    internal class BedrockPublisher : PublisherBase<IJavaToBedrockPublishMapping>, IBedrockPublisher
    {
        public BedrockPublisher(
            ILogger<BedrockPublisher> logger,
            IServiceProvider provider,
            IPublishReader loader,
            IInputReader reader,
            IOutputWriter writer,
            IJavaToBedrockPublishMapping mapping) : base(logger, provider, loader, reader, writer, mapping) {}

        protected override async Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token)
        {
            var packMeta = new BedrockPackMetadata {
                FormatVersion = pack.Format ?? ResourcePackProfileProperties.DefaultBedrockFormat,
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

            await using var stream = Writer.Open("manifest.json");
            await WriteJsonAsync(stream, packMeta, Formatting.Indented, token);
        }

        protected override async Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token)
        {
            //var graphBuilder = scopeProvider.GetRequiredService<ITextureGraphBuilder>();
            //await graphBuilder.PublishInventoryAsync("_carried.png", token);

            var context = scopeProvider.GetRequiredService<ITextureGraphContext>();

            if (context.OutputEncoding == null || context.IsMaterialCtm) return;

            var hasNormalMer = context.OutputEncoding.Any(e => TextureTags.Is(e.Texture, TextureTags.Normal) || TextureTags.Is(e.Texture, TextureTags.MER));
            if (!hasNormalMer) return;

            var sourcePath = context.Material.LocalPath;

            if (context.IsMaterialMultiPart) {
                foreach (var part in context.Material.Parts) {
                    if (Mapping.TryMap(sourcePath, part.Name, out var destPath, out var destName))
                        await CreateMaterialMetadataAsync(destPath, destName, token);
                }
            }
            else {
                if (Mapping.TryMap(sourcePath, context.Material.Name, out var destPath, out var destName))
                    await CreateMaterialMetadataAsync(destPath, destName, token);
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

                // TODO: replace with contains
                return Mapping.TryMap(sourceFile, out _);
            }) ?? false) return true;

            if (material.CTM?.Method != null) {
                // TODO
                return false;
            }

            sourceFile = PathEx.Join(sourcePath, material.Name);
            sourceFile = PathEx.Normalize(sourceFile);

            return Mapping.TryMap(sourceFile, out _);
        }

        private async Task CreateMaterialMetadataAsync(string matPath, string matName, CancellationToken token)
        {
            var meta = new BedrockMaterialMetadata {
                FormatVersion = "1.16.100",
                TextureSet = {
                    Color = $"{matName}",
                    Normal = $"{matName}_n",
                    MER = $"{matName}_mer",
                },
            };

            var name = $"{matName}.texture_set.json";
            var localFile = PathEx.Join(matPath, name);
            await using var stream = Writer.Open(localFile);
            await WriteJsonAsync(stream, meta, Formatting.Indented, token);
        }

        private static readonly Dictionary<string, string> fileMap = new(StringComparer.InvariantCultureIgnoreCase) {
            ["pack.png"] = "pack_icon.png",
            //...
        };
    }
}
