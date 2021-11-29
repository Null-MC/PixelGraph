using PixelGraph.Common.ResourcePack;
using System.Collections.Generic;

namespace PixelGraph.Common.Projects
{
    public class ProjectData
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public ResourcePackInputProperties Input {get; set;}
        public List<ResourcePackProfileProperties> Profiles {get; set;}


        public ProjectData()
        {
            Profiles = new List<ResourcePackProfileProperties>();
        }
    }
}
