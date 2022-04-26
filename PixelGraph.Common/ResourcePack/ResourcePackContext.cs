using System;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackContext
    {
        //public string RootPath {get; set;}
        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}
        public DateTime LastUpdated {get; set;}
    }
}
