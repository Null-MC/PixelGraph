using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinecraftMappings.Internal
{
    //public interface IBlockData
    //{
    //    string Name {get;}
    //}
    
    public abstract class BlockData<TVersion> //: IBlockData
        where TVersion : BlockDataVersion
    {
        public string Name {get;}
        public List<TVersion> Versions {get;}


        protected BlockData(string name)
        {
            Name = name;

            Versions = new List<TVersion>();
        }

        public TVersion GetLatestVersion()
        {
            // WARN: temp hack - not actually using version!
            return Versions.FirstOrDefault();
        }

        public static IEnumerable<T> FindBlockData<T>()
            where T : BlockData<TVersion>
        {
            return Assembly.GetExecutingAssembly()
                .ExportedTypes.Where(t => !t.IsAbstract)
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => (T) Activator.CreateInstance(t));
        }
    }

    public abstract class BlockDataVersion
    {
        public string Id {get; set;}
        public string MinVersion {get; set;}
        public string MaxVersion {get; set;}
        public int FrameCount {get; set;} = 1;
    }
}
