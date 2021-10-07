using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinecraftMappings.Internal.Items
{
    public interface IItemData
    {
        string Name {get;}
    }

    public interface IItemData<out TItemVersion> : IItemData
        where TItemVersion : ItemDataVersion
    {
        TItemVersion GetLatestVersion();
    }

    public abstract class ItemData : IItemData
    {
        public string Name {get;}


        protected ItemData(string name)
        {
            Name = name;
        }

        public static IEnumerable<T> FromAssembly<T>()
            where T : IItemData
        {
            return Assembly.GetExecutingAssembly()
                .ExportedTypes.Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => (T) Activator.CreateInstance(t));
        }
    }

    public abstract class ItemData<TVersion> : ItemData, IItemData<TVersion>
        where TVersion : ItemDataVersion, new()
    {
        public List<TVersion> Versions {get;}


        protected ItemData(string name) : base(name)
        {
            Versions = new List<TVersion>();
        }

        public TVersion GetLatestVersion()
        {
            // WARN: temp hack - not actually using version!
            return Versions.FirstOrDefault();
        }

        protected void AddVersion(string id, Action<TVersion> versionAction = null)
        {
            var version = new TVersion {
                Id = id,
            };

            versionAction?.Invoke(version);
            Versions.Add(version);
        }
    }
}
