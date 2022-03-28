using PixelGraph.Common.Extensions;
using PixelGraph.Common.ResourcePack;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization
{
    public interface IResourcePackWriter
    {
        Task WriteAsync(string localFile, ResourcePackInputProperties packInput, CancellationToken token = default);
        Task WriteAsync(string localFile, ResourcePackProfileProperties packProfile, CancellationToken token = default);
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
        
        public Task WriteAsync(string localFile, ResourcePackInputProperties packInput, CancellationToken token = default)
        {
            return writer.OpenWriteAsync(localFile, async stream => {
                await using var streamWriter = new StreamWriter(stream);
                serializer.Serialize(streamWriter, packInput);
            }, token);
        }

        public Task WriteAsync(string localFile, ResourcePackProfileProperties packProfile, CancellationToken token = default)
        {
            return writer.OpenWriteAsync(localFile, async stream => {
                await using var streamWriter = new StreamWriter(stream);
                serializer.Serialize(streamWriter, packProfile);
            }, token);
        }
    }
}
