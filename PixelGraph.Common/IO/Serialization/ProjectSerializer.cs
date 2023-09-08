using PixelGraph.Common.Extensions;
using PixelGraph.Common.Projects;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization;
//public interface IProjectSerializer
//{
//    Task<ProjectData> LoadAsync(string filename);
//    Task SaveAsync(ProjectData data, string filename);
//}

public class ProjectSerializer //: IProjectSerializer
{
    private static readonly ISerializer serializer;
    private static readonly IDeserializer deserializer;


    static ProjectSerializer()
    {
        serializer = new SerializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .WithEmissionPhaseObjectGraphVisitor(args => new CustomGraphVisitor(args.InnerVisitor))
            .Build();

        deserializer = new DeserializerBuilder()
            .WithTypeConverter(new YamlStringEnumConverter())
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();
    }

    public async Task<ProjectData> LoadAsync(string localFile)
    {
        await using var stream = File.OpenRead(localFile);
        return Parse(stream);
    }

    public async Task SaveAsync(ProjectData data, string filename)
    {
        await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write);
        Write(stream, data);
    }

    public static ProjectData Parse(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        return deserializer.Deserialize<ProjectData>(streamReader);
    }

    public static void Write(Stream stream, ProjectData data)
    {
        using var streamWriter = new StreamWriter(stream);
        serializer.Serialize(streamWriter, data);
    }
}