using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Textures;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Publishing
{
    internal interface ITexturePublisher
    {
        Task PublishAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token = default);
    }

    internal class TexturePublisher : ITexturePublisher
    {
        private const string Tag_Albedo = "#albedo";
        private const string Tag_Height = "#height";
        private const string Tag_Normal = "#normal";
        private const string Tag_Specular = "#specular";

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
            var filters = new FilterChain {
                DestinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}.png"),
                SourceFilename = GetFilename(texture, Tag_Albedo,
                    path: profile.GetSourcePath(texture.Path, texture.Name),
                    exactName: texture.Map.Albedo?.Texture),
            };

            if (profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);
        }

        private async Task PublishNormalAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var fromHeight = texture.Map.Normal?.FromHeight ?? true;

            var filters = new FilterChain {
                DestinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}_n.png"),
            };

            if (fromHeight) {
                filters.SourceFilename = GetFilename(texture, Tag_Height,
                    path: profile.GetSourcePath(texture.Path, texture.Name),
                    exactName: texture.Map.Normal?.Heightmap ?? texture.Map.Height?.Texture);

                var options = new NormalMapOptions();

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
                filters.SourceFilename = GetFilename(texture, Tag_Normal,
                    path: profile.GetSourcePath(texture.Path, texture.Name),
                    exactName: texture.Map.Normal?.Texture);
            }

            if (profile.TextureSize.HasValue) {
                filters.Append(new ResizeFilter {
                    TargetSize = profile.TextureSize.Value,
                });
            }

            await filters.ApplyAsync(token);
        }

        private async Task PublishSpecularAsync(IPublishProfile profile, TextureCollection texture, CancellationToken token)
        {
            var specularMap = texture.Map.Specular;

            var filters = new FilterChain {
                DestinationFilename = profile.GetDestinationPath(texture.Path, $"{texture.Name}_s.png"),
                SourceFilename = GetFilename(texture, Tag_Specular,
                    path: profile.GetSourcePath(texture.Path, texture.Name),
                    exactName: texture.Map.Specular?.Texture),
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

        private static readonly Dictionary<string, Func<TextureMap, string>> TextureMap = new Dictionary<string, Func<TextureMap, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Tag_Albedo] = map => map.Albedo.Texture,
            [Tag_Height] = map => map.Height.Texture,
            [Tag_Normal] = map => map.Normal.Texture,
            [Tag_Specular] = map => map.Specular.Texture,
        };

        private static readonly Dictionary<string, string> MatchMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            [Tag_Albedo] = "albedo.*",
            [Tag_Height] = "height.*",
            [Tag_Normal] = "normal.*",
            [Tag_Specular] = "specular.*",
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
