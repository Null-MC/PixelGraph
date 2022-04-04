using PixelGraph.Common.Extensions;
using PixelGraph.Common.ResourcePack;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization
{
    public interface IResourcePackReader
    {
        Task<ResourcePackInputProperties> ReadInputAsync(string filename);
        Task<ResourcePackProfileProperties> ReadProfileAsync(string filename);
    }

    public class ResourcePackReader : IResourcePackReader
    {
        private static readonly IDeserializer deserializer;
        private readonly IInputReader reader;


        static ResourcePackReader()
        {
            deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
        }

        public ResourcePackReader(IInputReader reader)
        {
            this.reader = reader;
        }

        public async Task<ResourcePackInputProperties> ReadInputAsync(string localFile)
        {
            ResourcePackInputProperties packInput = null;
            if (reader.FileExists(localFile)) {
                await using var stream = reader.Open(localFile);
                packInput = ParseInput(stream);
            }

            return packInput ?? new ResourcePackInputProperties();
        }

        public async Task<ResourcePackProfileProperties> ReadProfileAsync(string localFile)
        {
            await using var stream = reader.Open(localFile);
            var pack = ParseProfile(stream) ?? new ResourcePackProfileProperties();
            pack.LocalFile = localFile;
            return pack;
        }

        public static ResourcePackInputProperties ParseInput(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return deserializer.Deserialize<ResourcePackInputProperties>(streamReader);
        }

        public static ResourcePackProfileProperties ParseProfile(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return deserializer.Deserialize<ResourcePackProfileProperties>(streamReader);
        }
    }
}
