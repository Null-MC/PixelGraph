using McPbrPipeline.Internal.Filtering;
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
        Task PublishAsync(string sourcePath, string destinationPath, bool compress = false, CancellationToken token = default);
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

        public async Task PublishAsync(string profileFilename, string destination, bool compress = false, CancellationToken token = default)
        {
            if (profileFilename == null) throw new ArgumentNullException(nameof(profileFilename));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var sourcePath = Path.GetDirectoryName(profileFilename);
            var profile = await JsonFile.ReadAsync<Profile>(profileFilename, token);

            profile.Source = sourcePath;

            await using var output = GetOutputWriter(destination, compress);

            // TODO: pre-clean?

            await PublishPackMetaAsync(profile, output, token);

            await PublishContentAsync(profile, output, token);
        }

        private static Task PublishPackMetaAsync(IProfile profile, IOutputWriter writer, CancellationToken token)
        {
            var packMeta = new PackMetadata {
                PackFormat = profile.PackFormat,
                Description = profile.Description ?? string.Empty,
            };

            if (profile.Tags != null) {
                packMeta.Description += $"\n{string.Join(' ', profile.Tags)}";
            }

            var data = new {pack = packMeta};
            using var stream = writer.WriteFile("pack.mcmeta");
            return JsonFile.WriteAsync(stream, data, Formatting.Indented, token);
        }

        public async Task PublishContentAsync(IProfile profile, IOutputWriter writer, CancellationToken token = default)
        {
            var reader = new FileInputReader(profile.Source);
            var loader = new FileLoader(provider, reader);

            var albedoPublisher = new AlbedoTexturePublisher(profile, reader, writer);
            var normalPublisher = new NormalTexturePublisher(profile, reader, writer);
            var specularPublisher = new SpecularTexturePublisher(profile, reader, writer);
            var emissivePublisher = new EmissiveTexturePublisher(profile, reader, writer);
            var genericPublisher = new GenericTexturePublisher(profile, reader, writer);

            await foreach (var fileObj in loader.LoadAsync(token)) {
                token.ThrowIfCancellationRequested();

                switch (fileObj) {
                    case TextureCollection texture: {
                        logger.LogDebug($"Publishing texture '{texture.Name}'.");

                        if (profile.IncludeAlbedo ?? true) {
                            try {
                                await albedoPublisher.PublishAsync(texture, token);

                                logger.LogInformation($"Albedo texture generated for item '{texture.Name}'.");
                            }
                            catch (SourceEmptyException) {
                                logger.LogWarning($"No albedo texture to publish for item '{texture.Name}'.");
                            }
                            catch (Exception error) {
                                logger.LogError(error, $"Failed to publish albedo texture '{texture.Name}'!");
                            }
                        }

                        if (profile.IncludeNormal ?? true) {
                            try {
                                await normalPublisher.PublishAsync(texture, token);

                                logger.LogInformation($"Normal texture generated for item '{texture.Name}'.");
                            }
                            catch (SourceEmptyException) {
                                logger.LogWarning($"No normal texture to publish for item '{texture.Name}'.");
                            }
                            catch (Exception error) {
                                logger.LogError(error, $"Failed to publish normal texture '{texture.Name}'!");
                            }
                        }

                        if (profile.IncludeSpecular ?? true) {
                            try {
                                await specularPublisher.PublishAsync(texture, token);

                                logger.LogInformation($"Specular texture generated for item '{texture.Name}'.");
                            }
                            catch (SourceEmptyException) {
                                logger.LogWarning($"No specular texture to publish for item '{texture.Name}'.");
                            }
                            catch (Exception error) {
                                logger.LogError(error, $"Failed to publish specular texture '{texture.Name}'!");
                            }
                        }

                        if (profile.IncludeEmissive ?? true) {
                            try {
                                await emissivePublisher.PublishAsync(texture, token);

                                logger.LogInformation($"Emissive texture generated for item '{texture.Name}'.");
                            }
                            catch (SourceEmptyException) {
                                //logger.LogWarning($"No emissive texture to publish for item '{texture.Name}'.");
                            }
                            catch (Exception error) {
                                logger.LogError(error, $"Failed to publish emissive texture '{texture.Name}'!");
                            }
                        }

                        break;
                    }
                    case string localName: {
                        logger.LogDebug($"Publishing untracked file '{localName}'.");

                        var extension = Path.GetExtension(localName);
                        if (ImageExtensions.Supported.Contains(extension, StringComparer.InvariantCultureIgnoreCase)) {
                            await genericPublisher.PublishAsync(localName, token);
                        }
                        else {
                            var filename = profile.GetSourcePath(localName);
                            await using var srcStream = File.Open(filename, FileMode.Open, FileAccess.Read);
                            await using var destStream = writer.WriteFile(localName);
                            await srcStream.CopyToAsync(destStream, token);
                        }

                        break;
                    }
                }
            }
        }

        private IOutputWriter GetOutputWriter(string destination, bool compress)
        {
            if (compress) return new ArchiveOutputWriter(destination);
            
            if (!Directory.Exists(destination)) {
                logger.LogInformation($"Creating publish destination directory '{destination}'.");

                try {
                    Directory.CreateDirectory(destination);
                }
                catch (Exception error) {
                    throw new ApplicationException("Failed to create destination directory!", error);
                }
            }

            return new FileOutputWriter(destination);
        }
    }
}
