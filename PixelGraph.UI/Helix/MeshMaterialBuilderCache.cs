using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.Materials;
using PixelGraph.UI.Internal.Preview;
using System;
using System.IO;

namespace PixelGraph.UI.Helix
{
    internal interface IMeshMaterialBuilderCache
    {
        public CacheRegistration<string, IMaterialBuilder> Register(string localFile);
        void Release(CacheRegistration<string, IMaterialBuilder> registration);
    }

    internal class MeshMaterialBuilderCache : RegistrationCounterCache<string, IMaterialBuilder>, IMeshMaterialBuilderCache
    {
        private readonly IServiceProvider provider;


        public MeshMaterialBuilderCache(IServiceProvider provider) : base(StringComparer.InvariantCultureIgnoreCase)
        {
            this.provider = provider;
        }

        public CacheRegistration<string, IMaterialBuilder> Register(string localFile, ?)
        {
            return base.Register(localFile, () => GetMaterialBuilder());
        }

        public new void Release(CacheRegistration<string, IMaterialBuilder> registration)
        {
            base.Release(registration);
        }

    }
}
