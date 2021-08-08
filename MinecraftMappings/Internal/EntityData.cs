using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinecraftMappings.Internal
{
    public interface IEntityData
    {
        string Name {get;}
    }

    public interface IEntityData<out TEntityVersion> : IEntityData
        where TEntityVersion : EntityDataVersion, new()
    {
        TEntityVersion GetLatestVersion();
    }

    public abstract class EntityData : IEntityData
    {
        public string Name {get;}


        protected EntityData(string name)
        {
            Name = name;
        }

        public static IEnumerable<T> FromAssembly<T>()
            where T : IEntityData
        {
            return Assembly.GetExecutingAssembly()
                .ExportedTypes.Where(t => !t.IsAbstract)
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => (T) Activator.CreateInstance(t));
        }
    }

    public abstract class EntityData<TVersion> : EntityData, IEntityData<TVersion>
        where TVersion : EntityDataVersion, new()
    {
        public List<TVersion> Versions {get;}


        protected EntityData(string name) : base(name)
        {
            Versions = new List<TVersion>();
        }

        public TVersion GetLatestVersion()
        {
            // WARN: temp hack - not actually using version!
            return Versions.FirstOrDefault();
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
