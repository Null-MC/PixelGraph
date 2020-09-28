using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Textures;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Publishing
{
    internal class TexturePublisher
    {
        public async Task PublishAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default)
        {
            await PublishAlbedoAsync(profile, texture, token);
            await PublishNormalAsync(profile, texture, token);
        }

        private async Task PublishAlbedoAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var albedoTexture = GetFilename(
                path: profile.GetSourcePath(texture.Path),
                exactName: texture.Map.Albedo?.Texture,
                matchName: $"{texture.Name}.albedo");

            if (albedoTexture == null) {
                // LOG: no albedo texture found.
                return;
            }

            var filters = new FilterCollection();

            if (profile.HasTextureDimensions) {
                filters.Append(new ResizeFilter {
                    TargetWidth = profile.TextureWidth ?? 0,
                    TargetHeight = profile.TextureHeight ?? 0,
                });
            }

            using var imageSource = await Image.LoadAsync(albedoTexture, token);

            if (!filters.Empty)
                filters.Apply(imageSource);

            var destinationFilename = profile.GetDestinationPath(texture.Path, texture.Name);

            await imageSource.SaveAsPngAsync(destinationFilename, token);
        }

        private async Task PublishNormalAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var normalTexture = GetFilename(
                path: profile.GetSourcePath(texture.Path),
                exactName: texture.Map.Normal?.Texture,
                matchName: $"{texture.Name}.normal");

            var filters = new FilterCollection();

            if (profile.HasTextureDimensions) {
                filters.Append(new ResizeFilter {
                    TargetWidth = profile.TextureWidth ?? 0,
                    TargetHeight = profile.TextureHeight ?? 0,
                });
            }

            if (normalTexture != null) {
                // existing normal map
            }
            else if (texture.Map.Normal != null && texture.Map.Normal.FromHeight) {
                // generate from height
                normalTexture = GetFilename(
                    path: profile.GetSourcePath(texture.Path),
                    exactName: texture.Map.Height?.Texture,
                    matchName: $"{texture.Name}.height");

                filters.Append(new NormalMapFilter());
            }
            else {
                // LOG: no normal texture found.
                return;
            }

            using var imageSource = await Image.LoadAsync(normalTexture, token);

            if (!filters.Empty)
                filters.Apply(imageSource);

            var destinationFilename = profile.GetDestinationPath(texture.Path, texture.Name);

            await imageSource.SaveAsPngAsync(destinationFilename, token);
        }

        private static string GetFilename(string path, string exactName, string matchName)
        {
            if (exactName != null) {
                var filename = Path.Combine(path, exactName);
                if (File.Exists(filename)) return filename;
            }

            foreach (var filename in Directory.EnumerateFiles(path, $"{matchName}.*")) {
                var extension = Path.GetExtension(filename);
                if (string.Equals(".json", extension, StringComparison.InvariantCultureIgnoreCase)) continue;

                return filename;
            }

            return null;
        }
    }
}
