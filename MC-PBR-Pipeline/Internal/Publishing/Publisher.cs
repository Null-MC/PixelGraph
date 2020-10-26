using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal interface IPublisher
    {
        Task PublishAsync(PublishOptions options, CancellationToken token = default);
    }

    internal class Publisher : IPublisher
    {
        private readonly IServiceProvider provider;
        private readonly ILogger logger;


        public Publisher(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<Publisher>>();
        }

        public async Task PublishAsync(PublishOptions options, CancellationToken token = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var pack = new PackProperties {
                Source = Path.GetDirectoryName(options.Profile),
                WriteTime = File.GetLastWriteTime(options.Profile),
            };

            await using (var stream = File.Open(options.Profile, FileMode.Open, FileAccess.Read)) {
                await pack.ReadAsync(stream, token);
            }

            if (options.Properties != null)
                foreach (var property in options.Properties)
                    pack.TrySet(property);

            await using var output = GetOutputWriter(options.Destination, options.Compress);

            if (options.Clean) {
                logger.LogDebug("Cleaning destination...");

                try {
                    output.Clean();

                    logger.LogInformation("Destination directory clean.");
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to clean destination!");
                    throw new ApplicationException("Failed to clean destination!", error);
                }
            }

            await PublishPackMetaAsync(pack, output, token);

            await PublishContentAsync(pack, output, token);
        }

        private static Task PublishPackMetaAsync(PackProperties pack, IOutputWriter writer, CancellationToken token)
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
            return JsonFile.WriteAsync(stream, data, Formatting.Indented, token);
        }

        public async Task PublishContentAsync(PackProperties pack, IOutputWriter writer, CancellationToken token = default)
        {
            var reader = new FileInputReader(pack.Source);
            var loader = new FileLoader(provider, reader);
            var graph = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            var genericPublisher = new GenericTexturePublisher(pack, reader, writer);

            await foreach (var fileObj in loader.LoadAsync(token)) {
                token.ThrowIfCancellationRequested();
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

                        await graph.BuildAsync(texture, token);

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
                            await genericPublisher.PublishAsync(localName, token);
                        }
                        else {
                            await using var srcStream = reader.Open(localName);
                            await using var destStream = writer.WriteFile(localName);
                            await srcStream.CopyToAsync(destStream, token);
                        }

                        logger.LogInformation("Published untracked file {localName}.", localName);
                        break;
                }
            }
        }

        private static bool IsUpToDate(DateTime profileWriteTime, DateTime? sourceWriteTime, DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }

        private IOutputWriter GetOutputWriter(string destination, bool compress)
        {
            if (compress) return new ArchiveOutputWriter(destination);
            
            if (!Directory.Exists(destination)) {
                logger.LogInformation("Creating publish destination directory {destination}.", destination);

                try {
                    Directory.CreateDirectory(destination);
                }
                catch (Exception error) {
                    throw new ApplicationException("Failed to create destination directory!", error);
                }
            }

            return new FileOutputWriter(destination);
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
