using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.UI.Internal.Caching;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal interface IMaterialPropertiesCache
    {
        Task<CacheRegistration<string, MaterialProperties>> RegisterAsync(string localFile, CancellationToken token = default);
        void Release(CacheRegistration<string, MaterialProperties> registration);
        void Clear();
    }

    internal class MaterialPropertiesCache : AsyncRegistrationCounterCache<string, MaterialProperties>, IMaterialPropertiesCache
    {
        private readonly IServiceProvider provider;
        private readonly IProjectContext projectContext;


        public MaterialPropertiesCache(
            IServiceProvider provider,
            IProjectContext projectContext) : base(StringComparer.InvariantCultureIgnoreCase)
        {
            this.provider = provider;
            this.projectContext = projectContext;
        }

        public Task<CacheRegistration<string, MaterialProperties>> RegisterAsync(string localFile, CancellationToken token = default)
        {
            return RegisterAsync(localFile, key => LoadMaterial(key, token));
        }

        public new void Release(CacheRegistration<string, MaterialProperties> registration)
        {
            base.Release(registration);
        }

        private async Task<MaterialProperties> LoadMaterial(string localFile, CancellationToken token)
        {
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

            await using var scope = serviceBuilder.Build();

            var materialReader = scope.GetRequiredService<IMaterialReader>();
            return await materialReader.LoadAsync(localFile, token);
        }
    }
}
