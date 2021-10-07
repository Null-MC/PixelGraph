using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinecraftMappings.Internal.Models
{
    public interface IModelData
    {
        string Name {get;}
        ModelVersion GetLatestVersion();
        //ModelVersion GetVersion(Version version);
    }

    public abstract class ModelData : IModelData
    {
        public string Name {get; set;}
        public List<ModelVersion> Versions {get; set;}


        protected ModelData(string name)
        {
            Name = name;
            Versions = new List<ModelVersion>();
        }

        public ModelVersionBuilder AddVersion(string id, string version)
        {
            var modelVersion = new ModelVersion {
                Id = id,
                TextVersion = version,
            };

            Versions.Add(modelVersion);
            return new ModelVersionBuilder(modelVersion);
        }

        public ModelVersion GetLatestVersion()
        {
            return Versions.OrderByDescending(v => v.ParsedVersion)
                .FirstOrDefault();
        }

        public static IEnumerable<T> FromAssembly<T>()
            where T : IModelData
        {
            return Assembly.GetExecutingAssembly()
                .ExportedTypes.Where(t => !t.IsAbstract)
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Select(t => (T) Activator.CreateInstance(t));
        }
    }
}
