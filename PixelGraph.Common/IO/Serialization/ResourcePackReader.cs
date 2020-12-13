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

    internal class ResourcePackReader : IResourcePackReader
    {
        private readonly IInputReader reader;
        private readonly IDeserializer deserializer;


        public ResourcePackReader(IInputReader reader)
        {
            this.reader = reader;

            deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
        }

        public async Task<ResourcePackInputProperties> ReadInputAsync(string localFile)
        {
            ResourcePackInputProperties packInput = null;
            if (reader.FileExists(localFile)) {
                await using var stream = reader.Open(localFile);
                using var streamReader = new StreamReader(stream);
                packInput = deserializer.Deserialize<ResourcePackInputProperties>(streamReader);
            }

            return packInput ?? new ResourcePackInputProperties();
        }

        public async Task<ResourcePackProfileProperties> ReadProfileAsync(string localFile)
        {
            await using var stream = reader.Open(localFile);
            using var streamReader = new StreamReader(stream);
            var pack = deserializer.Deserialize<ResourcePackProfileProperties>(streamReader)
                   ?? new ResourcePackProfileProperties();

            pack.LocalFile = localFile;
            return pack;
        }
    }
}
