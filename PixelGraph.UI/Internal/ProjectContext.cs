using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;

namespace PixelGraph.UI.Internal
{
    public class ProjectContext
    {
        public ProjectData Project {get; set;}
        public PublishProfileProperties SelectedProfile {get; set;}
        public string ProjectFilename {get; set;}
        public string RootDirectory {get; set;}
    }
}
