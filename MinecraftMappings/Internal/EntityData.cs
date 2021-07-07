using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinecraftMappings.Internal
{
    //public interface IEntityData
    //{
    //    string Name {get;}
    //}

    public abstract class EntityData<TVersion> //: IEntityData
        where TVersion : EntityDataVersion
    {
        public string Name {get;}
        public List<TVersion> Versions {get;}


        protected EntityData(string name)
        {
            Name = name;

            Versions = new List<TVersion>();
        }

        public TVersion GetLatestVersion()
        {
            // WARN: temp hack - not actually using version!
            return Versions.FirstOrDefault();
        }

        public static IEnumerable<T> FindEntityData<T>()
            where T : EntityData<TVersion>
        {
            return Assembly.GetExecutingAssembly()
                .ExportedTypes.Where(t => !t.IsAbstract)
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => (T) Activator.CreateInstance(t));
        }
    }

    public abstract class EntityDataVersion
    {
        public string Id {get; set;}
        public string Path {get; set;}
        public string MinVersion {get; set;}
        public string MaxVersion {get; set;}
        //public int FrameCount {get; set;} = 1;
    }
}
