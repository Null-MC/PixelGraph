using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal interface ITexturePublisher
    {
        Task PublishAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default);
    }

    internal class TexturePublisher : ITexturePublisher
    {
        private readonly ILogger logger;


        public TexturePublisher(ILogger<TexturePublisher> logger)
        {
            this.logger = logger;
        }

        public async Task PublishAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default)
        {
            logger.LogDebug($"Publishing texture '{texture.Name}'.");

            try {
                await new AlbedoTexturePublisher(profile).PublishAsync(texture, token);
            }
            catch (SourceEmptyException) {
                logger.LogDebug($"No albedo texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish albedo texture '{texture.Name}'!");
            }

            try {
                await new NormalTexturePublisher(profile).PublishAsync(texture, token);
            }
            catch (SourceEmptyException) {
                logger.LogDebug($"No normal texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish normal texture '{texture.Name}'!");
            }

            try {
                await new SpecularTexturePublisher(profile).PublishAsync(texture, token);
            }
            catch (SourceEmptyException) {
                logger.LogDebug($"No specular texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish specular texture '{texture.Name}'!");
            }
        }
    }
}
