using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Helix
{
    internal interface IMaterialPropertiesCache
    {
        string RootDirectory {get; set;}

        Task<CacheRegistration<string, MaterialProperties>> RegisterAsync(string localFile, CancellationToken token = default);
        void Release(CacheRegistration<string, MaterialProperties> registration);
        void Clear();
    }

    internal class MaterialPropertiesCache : AsyncRegistrationCounterCache<string, MaterialProperties>, IMaterialPropertiesCache
    {
        private readonly IServiceProvider provider;

        public string RootDirectory {get; set;}


        public MaterialPropertiesCache(IServiceProvider provider) : base(StringComparer.InvariantCultureIgnoreCase)
        {
            this.provider = provider;
        }

        public Task<CacheRegistration<string, MaterialProperties>> RegisterAsync(string localFile, CancellationToken token = default)
        {
            return RegisterAsync(localFile, key => LoadMaterial(key, token));
        }

        public new void Release(CacheRegistration<string, MaterialProperties> registration)
        {
            base.Release(registration);
        }

        private Task<MaterialProperties> LoadMaterial(string localFile, CancellationToken token)
        {
            using var scope = provider.CreateScope();
            var reader = scope.ServiceProvider.GetRequiredService<IInputReader>();
            var materialReader = scope.ServiceProvider.GetRequiredService<IMaterialReader>();

            reader.SetRoot(RootDirectory);
            return materialReader.LoadAsync(localFile, token);
        }
    }
}
