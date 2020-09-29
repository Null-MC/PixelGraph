using McPbrPipeline.Internal.Serialization;
using McPbrPipeline.Internal.Textures;
using Newtonsoft.Json;
using Serilog;
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
        private readonly ITexturePublisher texturePublisher;


        public Publisher(
            ITextureLoader textureLoader,
            ITexturePublisher texturePublisher)
        {
            this.textureLoader = textureLoader;
            this.texturePublisher = texturePublisher;
        }

        public async Task PublishAsync(string sourcePath, string destinationPath, CancellationToken token = default)
        {
            var packFilename = Path.Combine(sourcePath, "pack.json");

            if (!File.Exists(packFilename))
                throw new ApplicationException($"No pack.json file found in directory '{sourcePath}'!");

            var profile = await JsonFile.ReadAsync<PublishProfile>(packFilename, token);

            profile.Source = sourcePath;
            profile.Destination = destinationPath;

            if (!Directory.Exists(destinationPath)) {
                Log.Debug($"Creating publish destination directory '{destinationPath}'.");
                Directory.CreateDirectory(destinationPath);
            }

            // TODO: pre-clean?

            // Publish Pack Metadata
            await PublishPackMetaAsync(profile, token);

            // Publish Textures
            await foreach (var texture in textureLoader.LoadAsync(sourcePath, token))
                await texturePublisher.PublishAsync(profile, texture, token);

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

            return JsonFile.WriteAsync(packMetaFilename, packMeta, Formatting.Indented, token);
        }
    }
}
