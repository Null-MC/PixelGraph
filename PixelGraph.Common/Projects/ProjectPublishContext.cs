using System;

namespace PixelGraph.Common.Projects;

public class ProjectPublishContext
{
    public IProjectDescription Project {get; set;}
    public PublishProfileProperties Profile {get; set;}
    public DateTime LastUpdated {get; set;}
}