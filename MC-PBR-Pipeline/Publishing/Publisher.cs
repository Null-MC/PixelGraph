using McPbrPipeline.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Publishing
{
    internal class Publisher
    {
        public PublishProfile Profile {get; set;}


        public async Task PublishAsync(CancellationToken token = default)
        {
            // TODO: pre-clean?

            await PublishPackMetaAsync(token);

            var sourceTexturePath = GetSourcePath("assets", "minecraft", "textures");

            foreach (var file in Directory.EnumerateFiles(sourceTexturePath, "*.*", SearchOption.TopDirectoryOnly)) {
                //
            }

            //foreach (var path in Directory.EnumerateDirectories(assetsPath, ))

                // TODO: copy all content (except destination) from source to destination...
                // TODO: apply filtering
        }

        private Task PublishPackMetaAsync(CancellationToken token)
        {
            var packMeta = new PackMetadata {
                PackFormat = Profile.PackFormat,
                Description = Profile.Description ?? string.Empty,
            };

            if (Profile.Tags != null) {
                if (packMeta.Description.Length > 0) packMeta.Description += "\r\n";
                packMeta.Description += $"\r\n{string.Join(' ', Profile.Tags)}";
            }

            var packMetaFilename = GetDestinationPath("pack.mcmeta");
            return WriteJsonAsync(packMetaFilename, packMeta, token);
        }

        private string GetSourcePath(params string[] path)
        {
            var source = Path.GetFullPath(Profile.Source);
            return Path.Combine(new[] {source}.Union(path).ToArray());
        }

        private string GetDestinationPath(params string[] path)
        {
            var destination = Path.GetFullPath(Profile.Destination);
            return Path.Combine(new[] { destination }.Union(path).ToArray());
        }

        private static async Task WriteJsonAsync(string filename, object content, CancellationToken token)
        {
            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer);
            await new JObject(content).WriteToAsync(jsonWriter, token);
        }
    }
}
