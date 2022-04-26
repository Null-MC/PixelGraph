using PixelGraph.Common.ResourcePack;
using System.Collections.Generic;

namespace PixelGraph.Common.Projects
{
    public class ProjectData
    {
        public string Name {get; set;}
        public string Description {get; set;}
        public string Author {get; set;}
        public ResourcePackInputProperties Input {get; set;}
        public List<ResourcePackProfileProperties> Profiles {get; set;}


        public ProjectData()
        {
            Input = new ResourcePackInputProperties();
            Profiles = new List<ResourcePackProfileProperties>();
        }
    }
}
