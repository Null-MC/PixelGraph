using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IPublisher
    {
        Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default);
    }

    internal class Publisher : IPublisher
    {
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly INamingStructure naming;
        private readonly ILogger logger;
        private readonly ITextureGraphBuilder graphBuilder;
        private readonly IFileLoader loader;


        public Publisher(
            ITextureGraphBuilder graphBuilder,
            IFileLoader loader,
            IInputReader reader,
            IOutputWriter writer,
            INamingStructure naming,
            ILogger<Publisher> logger)
        {
            this.graphBuilder = graphBuilder;
            this.loader = loader;
            this.reader = reader;
            this.writer = writer;
            this.naming = naming;
            this.logger = logger;
        }

        public async Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (clean) {
                logger.LogDebug("Cleaning destination...");

                try {
                    writer.Clean();

                    logger.LogInformation("Destination directory clean.");
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to clean destination!");
                    throw new ApplicationException("Failed to clean destination!", error);
                }
            }

            await PublishPackMetaAsync(context.Profile, token);

            await PublishContentAsync(context, token);
        }

        private async Task PublishContentAsync(ResourcePackContext context, CancellationToken token = default)
        {
            var genericPublisher = new GenericTexturePublisher(context.Profile, reader, writer);

            var packWriteTime = reader.GetWriteTime(context.Profile.LocalFile) ?? DateTime.Now;

            await foreach (var fileObj in loader.LoadAsync(token)) {
                token.ThrowIfCancellationRequested();
                DateTime? sourceTime, destinationTime = null;

                switch (fileObj) {
                    case MaterialProperties material:
                        sourceTime = reader.GetWriteTime(material.LocalFilename);

                        foreach (var texFile in reader.EnumerateAllTextures(material)) {
                            var z = reader.GetWriteTime(texFile);
                            if (!z.HasValue) continue;

                            if (!sourceTime.HasValue || z.Value > sourceTime.Value)
                                sourceTime = z.Value;
                        }

                        if (material.IsMultiPart) {
                            foreach (var part in material.Parts) {
                                var albedoOutputName = naming.GetOutputTextureName(context.Profile, part.Name, TextureTags.Albedo, true);
                                var albedoFile = PathEx.Join(material.LocalPath, albedoOutputName);
                                var writeTime = writer.GetWriteTime(albedoFile);
                                if (!writeTime.HasValue) continue;

                                if (!destinationTime.HasValue || writeTime.Value > destinationTime.Value)
                                    destinationTime = writeTime;
                            }
                        }
                        else {
                            var albedoOutputName = naming.GetOutputTextureName(context.Profile, material.Name, TextureTags.Albedo, true);
                            var albedoFile = PathEx.Join(material.LocalPath, albedoOutputName);
                            destinationTime = writer.GetWriteTime(albedoFile);
                        }

                        if (IsUpToDate(packWriteTime, sourceTime, destinationTime)) {
                            logger.LogDebug($"Skipping up-to-date texture {material.LocalPath}:{{DisplayName}}.", material.DisplayName);
                            continue;
                        }

                        logger.LogDebug($"Publishing texture {material.LocalPath}:{{DisplayName}}.", material.DisplayName);

                        var materialContext = new MaterialContext {
                            Input = context.Input,
                            Profile = context.Profile,
                            Material = material,
                        };

                        await graphBuilder.ProcessInputGraphAsync(materialContext, token);

                        // TODO: Publish mcmeta files

                        break;
                    case string localFile:
                        sourceTime = reader.GetWriteTime(localFile);
                        destinationTime = writer.GetWriteTime(localFile);

                        var file = Path.GetFileName(localFile);
                        if (fileIgnoreList.Contains(file)) {
                            logger.LogDebug("Skipping ignored file {localFile}.", localFile);
                            continue;
                        }

                        if (IsUpToDate(packWriteTime, sourceTime, destinationTime)) {
                            logger.LogDebug("Skipping up-to-date untracked file {localFile}.", localFile);
                            continue;
                        }

                        if (IsGenericResizable(localFile)) {
                            await genericPublisher.PublishAsync(localFile, token);
                        }
                        else {
                            await using var srcStream = reader.Open(localFile);
                            await using var destStream = writer.Open(localFile);
                            await srcStream.CopyToAsync(destStream, token);
                        }

                        logger.LogInformation("Published untracked file {localFile}.", localFile);
                        break;
                }
            }
        }

        private Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token)
        {
            var packMeta = new PackMetadata {
                PackFormat = pack.Format ?? ResourcePackProfileProperties.DefaultFormat,
                Description = pack.Description ?? string.Empty,
            };

            if (pack.Tags != null) {
                packMeta.Description += $"\n{string.Join(' ', pack.Tags)}";
            }

            var data = new {pack = packMeta};
            using var stream = writer.Open("pack.mcmeta");
            return WriteAsync(stream, data, Formatting.Indented, token);
        }

        private static async Task WriteAsync(Stream stream, object content, Formatting formatting, CancellationToken token)
        {
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer) {Formatting = formatting};

            await JToken.FromObject(content).WriteToAsync(jsonWriter, token);
        }

        private static bool IsUpToDate(DateTime profileWriteTime, DateTime? sourceWriteTime, DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }

        private static bool IsGenericResizable(string localFile)
        {
            var extension = Path.GetExtension(localFile);
            if (!ImageExtensions.Supports(extension)) return false;

            // Do not resize pack icon
            var path = Path.GetDirectoryName(localFile);
            var name = Path.GetFileNameWithoutExtension(localFile);
            if (string.IsNullOrEmpty(path) && string.Equals("pack", name, StringComparison.InvariantCultureIgnoreCase)) return false;

            return !resizeIgnoreList.Any(x => localFile.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }

        private static readonly string[] resizeIgnoreList = {
            Path.Combine("assets", "minecraft", "textures", "font"),
            Path.Combine("assets", "minecraft", "textures", "gui"),
            Path.Combine("assets", "minecraft", "textures", "colormap"),
            Path.Combine("assets", "minecraft", "textures", "misc"),
            Path.Combine("assets", "minecraft", "optifine", "colormap"),
            Path.Combine("pack", "minecraft", "optifine", "colormap"),
        };

        private static readonly HashSet<string> fileIgnoreList = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
            "input.yml",
            "source.txt",
            "readme.txt",
            "readme.md",
        };
    }
}
