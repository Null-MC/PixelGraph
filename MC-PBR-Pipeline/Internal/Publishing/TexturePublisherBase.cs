using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal abstract class TexturePublisherBase
    {
        protected IProfile Profile {get;}
        protected IInputReader Reader {get;}
        protected IOutputWriter Writer {get;}


        protected TexturePublisherBase(
            IProfile profile,
            IInputReader reader,
            IOutputWriter writer)
        {
            Profile = profile;
            Reader = reader;
            Writer = writer;
        }

        protected async Task PublishAsync(string sourceFile, Rgba32? sourceColor, string destinationFile, Action<IImageProcessingContext> processAction, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(destinationFile))
                throw new ArgumentException("Value cannot be null or empty!", nameof(destinationFile));

            using var sourceImage = await LoadSourceImageAsync(sourceFile, sourceColor, token);
            using var targetImage = new Image<Rgba32>(Configuration.Default, sourceImage.Width, sourceImage.Height);

            var brush = new ImageBrush(sourceImage);
            targetImage.Mutate(c => c.Clear(brush));
            targetImage.Mutate(processAction);

            var path = Path.GetDirectoryName(destinationFile);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            await using var stream = Writer.WriteFile(destinationFile);
            await targetImage.SaveAsPngAsync(stream, token);
        }

        protected void Resize(IImageProcessingContext context, IPbrProperties texture)
        {
            if (!(texture?.ResizeEnabled ?? true)) return;
            if (!Profile.TextureSize.HasValue && !Profile.TextureScale.HasValue) return;

            var (width, height) = context.GetCurrentSize();

            var resampler = KnownResamplers.Bicubic;
            if (Profile.ResizeSampler != null && Samplers.TryParse(Profile.ResizeSampler, out var _resampler))
                resampler = _resampler;

            if (Profile.TextureSize.HasValue) {
                if (width == Profile.TextureSize) return;

                context.Resize(Profile.TextureSize.Value, 0, resampler);
            }
            else {
                var targetWidth = (int)Math.Max(width * Profile.TextureScale.Value, 1f);
                var targetHeight = (int)Math.Max(height * Profile.TextureScale.Value, 1f);

                context.Resize(targetWidth, targetHeight, resampler);
            }
        }

        private async Task<Image> LoadSourceImageAsync(string sourceFile, Rgba32? sourceColor, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(sourceFile)) {
                await using var stream = Reader.Open(sourceFile);
                return await Image.LoadAsync(Configuration.Default, stream, token);
            }

            if (sourceColor.HasValue)
                return new Image<Rgba32>(Configuration.Default, 1, 1, sourceColor.Value);

            throw new SourceEmptyException("No Source image was found, and no color is defined!");
        }

        protected static string GetFilename(IPbrProperties texture, string type, string path, string exactName)
        {
            var matchName = GetMatchName(texture, type);

            if (exactName != null && TextureMap.TryGetValue(exactName, out var remap)) {
                matchName = GetMatchName(texture, exactName);
                exactName = remap(texture);
            }

            if (exactName != null) {
                var filename = Path.Combine(path, exactName);
                if (File.Exists(filename)) return filename;
            }

            foreach (var filename in Directory.EnumerateFiles(path, matchName)) {
                var extension = Path.GetExtension(filename);

                if (ImageExtensions.Supported.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    return filename;
            }

            return null;
        }

        protected static Task PublishMcMetaAsync(JToken metadata, string textureDestinationFilename, CancellationToken token)
        {
            var mcMetaDestinationFilename = $"{textureDestinationFilename}.mcmeta";
            return JsonFile.WriteAsync(mcMetaDestinationFilename, metadata, Formatting.Indented, token);
        }

        private static string GetMatchName(IPbrProperties texture, string type)
        {
            return texture.UseGlobalMatching
                ? GlobalMatchMap[type](texture.Name)
                : LocalMatchMap[type];
        }

        private static readonly Dictionary<string, Func<IPbrProperties, string>> TextureMap = new Dictionary<string, Func<IPbrProperties, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = t => t.AlbedoTexture,
            [TextureTags.Height] = t => t.HeightTexture,
            [TextureTags.Normal] = t => t.NormalTexture,
            [TextureTags.Specular] = t => t.SpecularTexture,
            [TextureTags.Emissive] = t => t.EmissiveTexture,
        };

        private static readonly Dictionary<string, string> LocalMatchMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = "albedo.*",
            [TextureTags.Height] = "height.*",
            [TextureTags.Normal] = "normal.*",
            [TextureTags.Specular] = "specular.*",
            [TextureTags.Emissive] = "emissive.*",
        };

        private static readonly Dictionary<string, Func<string, string>> GlobalMatchMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = item => $"{item}.*",
            [TextureTags.Height] = item => $"{item}_h.*",
            [TextureTags.Normal] = item => $"{item}_n.*",
            [TextureTags.Specular] = item => $"{item}_s.*",
            [TextureTags.Emissive] = item => $"{item}_e.*",
        };
    }
}
