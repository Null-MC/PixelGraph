using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;

namespace McPbrPipeline.Publishing
{
    internal interface ITexturePublisher
    {
        Task PublishAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default);
    }

    internal class TexturePublisher : ITexturePublisher
    {
        public async Task PublishAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default)
        {
            await PublishAlbedoAsync(profile, texture, token);
            await PublishNormalAsync(profile, texture, token);
            await PublishSpecularAsync(profile, texture, token);
        }

        private Task PublishAlbedoAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var filters = new FilterChain {
                DestinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}.png"),
                SourceFilename = GetFilename(
                    path: profile.GetSourcePath(texture.Path),
                    exactName: texture.Map.Albedo?.Texture,
                    matchName: $"{texture.Name}.albedo"),
            };

            if (profile.HasTextureDimensions) {
                filters.Append(new ResizeFilter {
                    TargetWidth = profile.TextureWidth ?? 0,
                    TargetHeight = profile.TextureHeight ?? 0,
                });
            }

            try {
                return filters.ApplyAsync(token);
            }
            catch (ArgumentException) {
                // TODO: log
                return Task.CompletedTask;
            }
        }

        private Task PublishNormalAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var fromHeight = texture.Map.Normal?.FromHeight ?? true;

            string exactName, matchName;
            if (fromHeight) {
                exactName = texture.Map.Height?.Texture;
                matchName = $"{texture.Name}.height";
            }
            else {
                exactName = texture.Map.Normal?.Texture;
                matchName = $"{texture.Name}.normal";
            }

            var filters = new FilterChain {
                DestinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}_n.png"),
                SourceFilename = GetFilename(
                    path: profile.GetSourcePath(texture.Path),
                    exactName: exactName,
                    matchName: matchName),
            };

            if (fromHeight) {
                var options = new NormalMapOptions();

                if (texture.Map.Normal?.Blur != null)
                    options.Blur = texture.Map.Normal.Blur;

                if (texture.Map.Normal?.DownSample != null)
                    options.DownSample = texture.Map.Normal.DownSample;

                if (texture.Map.Normal?.Strength != null)
                    options.Strength = texture.Map.Normal.Strength;

                if (texture.Map.Normal?.Wrap != null)
                    options.Wrap = texture.Map.Normal.Wrap;

                filters.Append(new NormalMapFilter(options));
            }

            if (profile.HasTextureDimensions) {
                filters.Append(new ResizeFilter {
                    TargetWidth = profile.TextureWidth ?? 0,
                    TargetHeight = profile.TextureHeight ?? 0,
                });
            }

            try {
                return filters.ApplyAsync(token);
            }
            catch (ArgumentException) {
                // TODO: log
                return Task.CompletedTask;
            }
        }

        private Task PublishSpecularAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var filters = new FilterChain {
                DestinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}_s.png"),
                SourceFilename = GetFilename(
                    path: profile.GetSourcePath(texture.Path),
                    exactName: texture.Map.Specular?.Texture,
                    matchName: $"{texture.Name}.specular"),
            };

            if (texture.Map.Specular?.Color != null)
                filters.SourceColor = Rgba32.ParseHex(texture.Map.Specular.Color);

            if (profile.HasTextureDimensions) {
                filters.Append(new ResizeFilter {
                    TargetWidth = profile.TextureWidth ?? 0,
                    TargetHeight = profile.TextureHeight ?? 0,
                });
            }

            try {
                return filters.ApplyAsync(token);
            }
            catch (ArgumentException) {
                // TODO: log
                return Task.CompletedTask;
            }
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
