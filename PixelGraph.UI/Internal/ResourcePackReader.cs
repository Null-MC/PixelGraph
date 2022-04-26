using PixelGraph.Common.Extensions;
using PixelGraph.Common.ResourcePack;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.UI.Internal
{
    public class ResourcePackReader
    {
        private static readonly IDeserializer deserializer;


        static ResourcePackReader()
        {
            deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
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
