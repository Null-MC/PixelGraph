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
        TEntityVersion GetVersion(Version version);
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
            return Versions.OrderByDescending(v => v.ParsedVersion)
                .FirstOrDefault();
        }

        public TVersion GetVersion(Version version)
        {
            return Versions.OrderByDescending(e => e.ParsedVersion)
                .FirstOrDefault(e => e.ParsedVersion <= version);
        }
    }

    public abstract class EntityDataVersion
    {
        private readonly Lazy<Version> _parsedVersion;

        public string Id {get; set;}
        public string Path {get; set;}
        //public string MinVersion {get; set;}
        //public string MaxVersion {get; set;}
        public string TextVersion {get; set;}
        //public int FrameCount {get; set;} = 1;

        public List<UVRegion> UVMappings {get; set;}

        public Version ParsedVersion => _parsedVersion.Value;


        protected EntityDataVersion()
        {
            _parsedVersion = new Lazy<Version>(() => {
                if (TextVersion == null) return null;
                return Version.Parse(TextVersion);
            });

            UVMappings = new List<UVRegion>();
        }
    }
}
