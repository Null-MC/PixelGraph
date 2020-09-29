using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                await PublishAlbedoAsync(profile, texture, token);
            }
            catch (SourceEmptyException) {
                logger.LogDebug($"No albedo texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish albedo texture '{texture.Name}'!");
            }

            try {
                await PublishNormalAsync(profile, texture, token);
            }
            catch (SourceEmptyException) {
                logger.LogDebug($"No normal texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish normal texture '{texture.Name}'!");
            }

            try {
                await PublishSpecularAsync(profile, texture, token);
            }
            catch (SourceEmptyException) {
                logger.LogDebug($"No specular texture to publish for item '{texture.Name}'.");
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to publish specular texture '{texture.Name}'!");
            }
        }

        private async Task PublishAlbedoAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var sourcePath = profile.GetSourcePath(texture.Path, texture.Name);
            var destinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}.png");

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Albedo, sourcePath, texture.Map.Albedo?.Texture),
            };

            if (profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (texture.Map.Albedo?.Metadata != null)
                await PublishMcMetaAsync(texture.Map.Albedo.Metadata, destinationFilename, token);
        }

        private async Task PublishNormalAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var sourcePath = profile.GetSourcePath(texture.Path, texture.Name);
            var destinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}_n.png");

            var fromHeight = texture.Map.Normal?.FromHeight ?? true;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
            };

            if (fromHeight) {
                filters.SourceFilename = GetFilename(texture, TextureTags.Height, sourcePath,
                    texture.Map.Normal?.Heightmap ?? texture.Map.Height?.Texture);

                var options = new NormalMapOptions();

                if (texture.Map.Normal?.DepthScale.HasValue ?? false)
                    options.DepthScale = texture.Map.Normal.DepthScale.Value;

                if (texture.Map.Normal?.Blur != null)
                    options.Blur = texture.Map.Normal.Blur.Value;

                if (texture.Map.Normal?.DownSample != null)
                    options.DownSample = texture.Map.Normal.DownSample.Value;

                if (texture.Map.Normal?.Strength != null)
                    options.Strength = texture.Map.Normal.Strength.Value;

                if (texture.Map.Normal?.Wrap != null)
                    options.Wrap = texture.Map.Normal.Wrap.Value;

                filters.Append(new NormalMapFilter(options));
            }
            else {
                filters.SourceFilename = GetFilename(texture, TextureTags.Normal, sourcePath, texture.Map.Normal?.Texture);
            }

            if (profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (texture.Map.Normal?.Metadata != null)
                await PublishMcMetaAsync(texture.Map.Normal.Metadata, destinationFilename, token);
        }

        private async Task PublishSpecularAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var sourcePath = profile.GetSourcePath(texture.Path, texture.Name);
            var destinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}_s.png");
            var specularMap = texture.Map.Specular;

            var filters = new FilterChain {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Specular, sourcePath, texture.Map.Specular?.Texture),
            };

            if (texture.Map.Specular?.Color != null)
                filters.SourceColor = Rgba32.ParseHex(texture.Map.Specular.Color);

            if (specularMap?.HasScaling() ?? false) {
                // TODO: set channel min-max using material channel mapping
                var options = new ScaleOptions {
                    Red = specularMap.MetalScale,
                    Green = specularMap.SmoothScale,
                    Blue = specularMap.EmissiveScale,
                };

                filters.Append(new ScaleFilter(options));
            }

            if (specularMap?.HasOffsets() ?? false) {
                // TODO: set channel min-max using material channel mapping
                var options = new RangeOptions {
                    RedMin = specularMap.MetalMin,
                    RedMax = specularMap.MetalMax,
                    GreenMin = specularMap.SmoothMin,
                    GreenMax = specularMap.SmoothMax,
                    BlueMin = specularMap.EmissiveMin,
                    BlueMax = specularMap.EmissiveMax,
                };

                filters.Append(new RangeFilter(options));
            }

            if (profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);

            if (texture.Map.Specular?.Metadata != null)
                await PublishMcMetaAsync(texture.Map.Specular.Metadata, destinationFilename, token);
        }

        private static string GetFilename(TextureCollection texture, string type, string path, string exactName)
        {
            var matchName = MatchMap[type];

            if (exactName != null && TextureMap.TryGetValue(exactName, out var remap)) {
                matchName = MatchMap[exactName];
                exactName = remap(texture.Map);
            }

            if (exactName != null) {
                var filename = Path.Combine(path, exactName);
                if (File.Exists(filename)) return filename;
            }

            foreach (var filename in Directory.EnumerateFiles(path, matchName)) {
                var extension = Path.GetExtension(filename);

                if (SupportedImageExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    return filename;
            }

            return null;
        }

        private static Task PublishMcMetaAsync(JToken metadata, string textureDestinationFilename, CancellationToken token)
        {
            var mcMetaDestinationFilename = $"{textureDestinationFilename}.mcmeta";
            return JsonFile.WriteAsync(mcMetaDestinationFilename, metadata, Formatting.Indented, token);
        }

        private static readonly Dictionary<string, Func<TextureMap, string>> TextureMap = new Dictionary<string, Func<TextureMap, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = map => map.Albedo.Texture,
            [TextureTags.Height] = map => map.Height.Texture,
            [TextureTags.Normal] = map => map.Normal.Texture,
            [TextureTags.Specular] = map => map.Specular.Texture,
        };

        private static readonly Dictionary<string, string> MatchMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = "albedo.*",
            [TextureTags.Height] = "height.*",
            [TextureTags.Normal] = "normal.*",
            [TextureTags.Specular] = "specular.*",
        };

        private static readonly string[] SupportedImageExtensions = {
            ".bmp",
            ".png",
            ".tga",
            ".gif",
            ".jpg",
            ".jpeg",
        };
    }
}
