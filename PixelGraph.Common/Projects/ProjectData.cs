using PixelGraph.Common.ResourcePack;

namespace PixelGraph.Common.Projects;

public interface IProjectDescription : ICloneable
{
    string? Name {get;}
    string? Description {get;}
    string? Author {get;}
    PackInputEncoding? Input {get;}
}

public class ProjectData : IProjectDescription
{
    public string? Name {get; set;}
    public string? Description {get; set;}
    public string? Author {get; set;}
    public PackInputEncoding? Input {get; set;}
    public List<PublishProfileProperties>? Profiles {get; set;}


    public ProjectData()
    {
        Input = new PackInputEncoding();
        Profiles = new List<PublishProfileProperties>();
    }

    public object Clone()
    {
        var data = (ProjectData)MemberwiseClone();
        data.Input = (PackInputEncoding?)Input?.Clone();
        data.Profiles = Profiles?.Select(p => (PublishProfileProperties)p.Clone()).ToList();
        return data;
    }
}
