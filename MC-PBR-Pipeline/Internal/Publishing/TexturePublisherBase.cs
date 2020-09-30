using McPbrPipeline.Internal.Textures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        protected IPublishProfile Profile {get;}


        protected TexturePublisherBase(IPublishProfile profile)
        {
            Profile = profile;
        }

        protected static string GetFilename(TextureCollection texture, string type, string path, string exactName)
        {
            var matchName = GetMatchName(texture, type);

            if (exactName != null && TextureMap.TryGetValue(exactName, out var remap)) {
                matchName = GetMatchName(texture, exactName);
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

        protected static Task PublishMcMetaAsync(JToken metadata, string textureDestinationFilename, CancellationToken token)
        {
            var mcMetaDestinationFilename = $"{textureDestinationFilename}.mcmeta";
            return JsonFile.WriteAsync(mcMetaDestinationFilename, metadata, Formatting.Indented, token);
        }

        private static string GetMatchName(TextureCollection texture, string type)
        {
            return texture.UseGlobalMatching
                ? GlobalMatchMap[type](texture.Name)
                : LocalMatchMap[type];
        }

        private static readonly Dictionary<string, Func<TextureMap, string>> TextureMap = new Dictionary<string, Func<TextureMap, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = map => map.Albedo.Texture,
            [TextureTags.Height] = map => map.Height.Texture,
            [TextureTags.Normal] = map => map.Normal.Texture,
            [TextureTags.Specular] = map => map.Specular.Texture,
        };

        private static readonly Dictionary<string, string> LocalMatchMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = "albedo.*",
            [TextureTags.Height] = "height.*",
            [TextureTags.Normal] = "normal.*",
            [TextureTags.Specular] = "specular.*",
        };

        private static readonly Dictionary<string, Func<string, string>> GlobalMatchMap = new Dictionary<string, Func<string, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            [TextureTags.Albedo] = item => $"{item}.*",
            [TextureTags.Height] = item => $"{item}_h.*",
            [TextureTags.Normal] = item => $"{item}_n.*",
            [TextureTags.Specular] = item => $"{item}_s.*",
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
