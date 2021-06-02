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
        where TVersion : BlockDataVersion, new()
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

        protected void AddVersion(string id, Action<TVersion> versionAction)
        {
            var version = new TVersion {
                Id = id,
            };

            versionAction(version);
            Versions.Add(version);
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
        public GameVersion MinVersion {get; set;}
        public GameVersion MaxVersion {get; set;}
        public int FrameCount {get; set;} = 1;
    }
}
