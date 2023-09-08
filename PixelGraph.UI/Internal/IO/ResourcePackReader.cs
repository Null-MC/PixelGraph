using System.IO;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.UI.Internal.IO;

public static class ResourcePackReader
{
    private static readonly IDeserializer deserializer;


    static ResourcePackReader()
    {
        deserializer = new DeserializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();
    }

    public static PackInputEncoding ParseInput(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        return deserializer.Deserialize<PackInputEncoding>(streamReader);
    }

    public static PublishProfileProperties ParseProfile(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        return deserializer.Deserialize<PublishProfileProperties>(streamReader);
    }
}