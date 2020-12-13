using PixelGraph.Common.Extensions;
using PixelGraph.Common.ResourcePack;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization
{
    public interface IResourcePackWriter
    {
        Task WriteAsync(string localFile, ResourcePackInputProperties packInput);
        Task WriteAsync(string localFile, ResourcePackProfileProperties packProfile);
    }

    internal class ResourcePackWriter : IResourcePackWriter
    {
        private readonly IOutputWriter writer;
        private readonly ISerializer serializer;


        public ResourcePackWriter(IOutputWriter writer)
        {
            this.writer = writer;

            serializer = new SerializerBuilder()
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .Build();
        }
        
        public async Task WriteAsync(string localFile, ResourcePackInputProperties packInput)
        {
            await using var stream = writer.Open(localFile);
            await using var streamWriter = new StreamWriter(stream);
            serializer.Serialize(streamWriter, packInput);
        }

        public async Task WriteAsync(string localFile, ResourcePackProfileProperties packProfile)
        {
            await using var stream = writer.Open(localFile);
            await using var streamWriter = new StreamWriter(stream);
            serializer.Serialize(streamWriter, packProfile);
        }
    }
}
