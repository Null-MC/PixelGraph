using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization;

public interface IMaterialWriter
{
    Task WriteAsync(MaterialProperties material, CancellationToken token = default);
}

internal class MaterialWriter : IMaterialWriter
{
    private static readonly ISerializer serializer;
    private readonly IOutputWriter writer;


    static MaterialWriter()
    {
        serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithEmissionPhaseObjectGraphVisitor(args => new CustomGraphVisitor(args.InnerVisitor))
            .Build();
    }

    public MaterialWriter(IOutputWriter writer)
    {
        this.writer = writer;
    }
        
    public Task WriteAsync(MaterialProperties material, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(material.LocalFilename);

        return writer.OpenWriteAsync(material.LocalFilename, async stream => {
            await using var streamWriter = new StreamWriter(stream);
            serializer.Serialize(streamWriter, material);
        }, token);
    }
}