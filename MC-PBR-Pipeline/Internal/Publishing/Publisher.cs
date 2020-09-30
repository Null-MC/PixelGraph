using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Serialization;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal interface IPublisher
    {
        Task PublishAsync(string sourcePath, string destinationPath, CancellationToken token = default);
    }

    internal class Publisher : IPublisher
    {
        private readonly ITextureLoader textureLoader;
        private readonly ILogger logger;


        public Publisher(
            ILogger<Publisher> logger,
            ITextureLoader textureLoader)
        {
            this.textureLoader = textureLoader;
            this.logger = logger;
        }

        public async Task PublishAsync(string profileFilename, string destinationPath, CancellationToken token = default)
        {
            if (profileFilename == null) throw new ArgumentNullException(nameof(profileFilename));
            if (destinationPath == null) throw new ArgumentNullException(nameof(destinationPath));

            var sourcePath = Path.GetDirectoryName(profileFilename);

            var profile = await JsonFile.ReadAsync<PublishProfile>(profileFilename, token);

            profile.Source = sourcePath;
            profile.Destination = destinationPath;

            if (!Directory.Exists(destinationPath)) {
                logger.LogInformation($"Creating publish destination directory '{destinationPath}'.");

                try {
                    Directory.CreateDirectory(destinationPath);
                }
                catch (Exception error) {
                    throw new ApplicationException("Failed to create destination directory!", error);
                }
            }

            // TODO: pre-clean?

            // Publish Pack Metadata
            await PublishPackMetaAsync(profile, token);

            // Publish Textures
            await foreach (var texture in textureLoader.LoadAsync(sourcePath, token))
                await PublishTextureAsync(profile, texture, token);

            // TODO: copy all content (except destination) from source to destination...
            // TODO: apply filtering
        }

        private static Task PublishPackMetaAsync(IPublishProfile profile, CancellationToken token)
        {
            var packMeta = new PackMetadata {
                PackFormat = profile.PackFormat,
                Description = profile.Description ?? string.Empty,
            };

            if (profile.Tags != null) {
                packMeta.Description += $"\n{string.Join(' ', profile.Tags)}";
            }

            var packMetaFilename = profile.GetDestinationPath("pack.mcmeta");

            var data = new {pack = packMeta};
            return JsonFile.WriteAsync(packMetaFilename, data, Formatting.Indented, token);
        }

        public async Task PublishTextureAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default)
        {
            logger.LogDebug($"Publishing texture '{texture.Name}'.");

            try {
                await new AlbedoTexturePublisher(profile).PublishAsync(texture, token);

                logger.LogInformation($"Albedo texture generated for item '{texture.Name}'.");
            }
            catch (SourceEmptyException) {
                logger.LogWarning($"No albedo texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish albedo texture '{texture.Name}'!");
            }

            try {
                await new NormalTexturePublisher(profile).PublishAsync(texture, token);

                logger.LogInformation($"Normal texture generated for item '{texture.Name}'.");
            }
            catch (SourceEmptyException) {
                logger.LogWarning($"No normal texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish normal texture '{texture.Name}'!");
            }

            try {
                await new SpecularTexturePublisher(profile).PublishAsync(texture, token);

                logger.LogInformation($"Specular texture generated for item '{texture.Name}'.");
            }
            catch (SourceEmptyException) {
                logger.LogWarning($"No specular texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish specular texture '{texture.Name}'!");
            }
        }
    }
}
