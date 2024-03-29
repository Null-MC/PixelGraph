﻿using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.UI.Internal.Caching;
using PixelGraph.UI.Internal.Projects;

namespace PixelGraph.UI.Internal;

internal class MaterialPropertiesCache(
    IServiceProvider provider,
    IProjectContextManager projectContextMgr)
    : AsyncRegistrationCounterCache<string, MaterialProperties>(StringComparer.InvariantCultureIgnoreCase)
{
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
        var projectContext = projectContextMgr.GetContextRequired();
        var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

        serviceBuilder.Initialize();
        serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

        await using var scope = serviceBuilder.Build();

        var materialReader = scope.GetRequiredService<IMaterialReader>();
        return await materialReader.LoadAsync(localFile, token)
            ?? throw new ApplicationException("Failed to load material!");
    }
}
