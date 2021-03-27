using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization
{
    public interface IMaterialWriter
    {
        Task WriteAsync(MaterialProperties material);
    }

    internal class MaterialWriter : IMaterialWriter
    {
        private readonly IOutputWriter writer;
        private readonly ISerializer serializer;


        public MaterialWriter(IOutputWriter writer)
        {
            this.writer = writer;

            serializer = new SerializerBuilder()
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .Build();
        }
        
        public async Task WriteAsync(MaterialProperties material)
        {
            await using var stream = writer.Open(material.LocalFilename);
            await using var streamWriter = new StreamWriter(stream);
            serializer.Serialize(streamWriter, material);
        }
    }
}
