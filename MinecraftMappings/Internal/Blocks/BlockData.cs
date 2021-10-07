using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinecraftMappings.Internal.Blocks
{
    public interface IBlockData
    {
        string Name {get;}
    }

    public interface IBlockData<out TBlockVersion> : IBlockData
        where TBlockVersion : BlockDataVersion
    {
        TBlockVersion GetLatestVersion();
    }

    public abstract class BlockData : IBlockData
    {
        public string Name {get;}


        protected BlockData(string name)
        {
            Name = name;
        }

        public static IEnumerable<T> FromAssembly<T>()
            where T : IBlockData
        {
            return Assembly.GetExecutingAssembly()
                .ExportedTypes.Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => (T) Activator.CreateInstance(t));
        }
    }

    public abstract class BlockData<TVersion> : BlockData, IBlockData<TVersion>
        where TVersion : BlockDataVersion, new()
    {
        public List<TVersion> Versions {get;}


        protected BlockData(string name) : base(name)
        {
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
    }

    public abstract class BlockDataVersion
    {
        public string Id {get; set;}
        public GameVersion MinVersion {get; set;}
        public GameVersion MaxVersion {get; set;}
        public int FrameCount {get; set;} = 1;
    }
}
