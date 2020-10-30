using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal interface IPublisher
    {
        Task PublishAsync(PackProperties pack, string destination, bool clean);
    }

    internal class Publisher : IPublisher
    {
        private readonly IAppLifetime lifetime;
        private readonly IInputReader reader;
        private readonly IOutputWriter writer;
        private readonly ILogger logger;
        private readonly ITextureGraphBuilder graphBuilder;
        private readonly IFileLoader loader;


        public Publisher(
            ITextureGraphBuilder graphBuilder,
            IFileLoader loader,
            IAppLifetime lifetime,
            IInputReader reader,
            IOutputWriter writer,
            ILogger<Publisher> logger)
        {
            this.graphBuilder = graphBuilder;
            this.loader = loader;
            this.lifetime = lifetime;
            this.reader = reader;
            this.writer = writer;
            this.logger = logger;
        }

        public async Task PublishAsync(PackProperties pack, string destination, bool clean)
        {
            if (pack == null) throw new ArgumentNullException(nameof(pack));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

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

            await PublishPackMetaAsync(pack);

            await PublishContentAsync(pack);
        }

        public async Task PublishContentAsync(PackProperties pack)
        {
            var genericPublisher = new GenericTexturePublisher(pack, reader, writer);

            await foreach (var fileObj in loader.LoadAsync(lifetime.Token)) {
                lifetime.Token.ThrowIfCancellationRequested();
                DateTime? sourceTime, destinationTime;

                switch (fileObj) {
                    case PbrProperties texture:
                        sourceTime = reader.GetWriteTime(texture.FileName);
                        foreach (var texFile in texture.GetAllTextures(reader)) {
                            var z = reader.GetWriteTime(texFile);
                            if (!z.HasValue) continue;
                            if (!sourceTime.HasValue || z.Value > sourceTime.Value) sourceTime = z.Value;
                        }

                        var albedoOutputName = PathEx.Join(texture.Path, $"{texture.Name}.png");
                        destinationTime = writer.FileExists(albedoOutputName)
                            ? writer.GetWriteTime(albedoOutputName) : null;

                        if (IsUpToDate(pack.WriteTime, sourceTime, destinationTime)) {
                            logger.LogDebug("Skipping up-to-date texture {DisplayName}.", texture.DisplayName);
                            continue;
                        }

                        logger.LogDebug("Publishing texture {DisplayName}.", texture.DisplayName);

                        await graphBuilder.BuildAsync(pack, texture, lifetime.Token);

                        // TODO: Publish mcmeta files

                        break;
                    case string localName:
                        sourceTime = reader.GetWriteTime(localName);
                        destinationTime = writer.GetWriteTime(localName);

                        if (IsUpToDate(pack.WriteTime, sourceTime, destinationTime)) {
                            logger.LogDebug("Skipping up-to-date untracked file {localName}.", localName);
                            continue;
                        }

                        var extension = Path.GetExtension(localName);
                        var filterImage = ImageExtensions.Supported.Contains(extension, StringComparer.InvariantCultureIgnoreCase)
                            && !pathIgnoreList.Any(x => localName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));

                        if (filterImage) {
                            await genericPublisher.PublishAsync(localName, lifetime.Token);
                        }
                        else {
                            await using var srcStream = reader.Open(localName);
                            await using var destStream = writer.WriteFile(localName);
                            await srcStream.CopyToAsync(destStream, lifetime.Token);
                        }

                        logger.LogInformation("Published untracked file {localName}.", localName);
                        break;
                }
            }
        }

        private Task PublishPackMetaAsync(PackProperties pack)
        {
            var packMeta = new PackMetadata {
                PackFormat = pack.PackFormat,
                Description = pack.PackDescription ?? string.Empty,
            };

            if (pack.PackTags != null) {
                packMeta.Description += $"\n{string.Join(' ', pack.PackTags)}";
            }

            var data = new {pack = packMeta};
            using var stream = writer.WriteFile("pack.mcmeta");
            return WriteAsync(stream, data, Formatting.Indented, lifetime.Token);
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

        private static readonly string[] pathIgnoreList = {
            Path.Combine("assets", "minecraft", "textures", "font"),
            Path.Combine("assets", "minecraft", "textures", "gui"),
            Path.Combine("assets", "minecraft", "textures", "colormap"),
            Path.Combine("assets", "minecraft", "textures", "misc"),
            Path.Combine("assets", "minecraft", "optifine", "colormap"),
        };
    }
}
