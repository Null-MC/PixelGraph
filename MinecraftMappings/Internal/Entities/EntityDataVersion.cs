using System.Collections.Generic;
using MinecraftMappings.Internal.Models;

namespace MinecraftMappings.Internal.Entities
{
    public abstract class EntityDataVersion : Versionable
    {
        public string Id {get; set;}
        public string Path {get; set;}
        //public string MinVersion {get; set;}
        //public string MaxVersion {get; set;}
        //public int FrameCount {get; set;} = 1;

        public List<EntityElement> Elements {get; set;}
        public List<UVRegion> UVMappings {get; set;}


        protected EntityDataVersion()
        {
            Elements = new List<EntityElement>();
            UVMappings = new List<UVRegion>();
        }
    }
}
