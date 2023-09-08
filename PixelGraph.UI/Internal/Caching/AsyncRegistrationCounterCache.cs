using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Caching;

internal abstract class AsyncRegistrationCounterCache<TKey, TValue>
{
    private readonly Dictionary<TKey, CounterCacheItem<TValue>> map;


    protected AsyncRegistrationCounterCache(IEqualityComparer<TKey> keyComparer)
    {
        map = new Dictionary<TKey, CounterCacheItem<TValue>>(keyComparer);
    }

    protected async Task<CacheRegistration<TKey, TValue>> RegisterAsync(TKey key, Func<TKey, Task<TValue>> createFunc)
    {
        var registrationId = Guid.NewGuid();
        var counter = await GetOrCreateAsync(key, createFunc);
        counter.Registrations.Add(registrationId);

        return new CacheRegistration<TKey, TValue>(key, counter.Item, registrationId);
    }

    protected void Release(CacheRegistration<TKey, TValue> registration)
    {
        if (!map.TryGetValue(registration.Key, out var counter)) return;
        if (!counter.Registrations.Remove(registration.RegistrationId)) return;

        if (counter.Registrations.Count == 0) {
            if (counter.Item is IDisposable disposable) disposable.Dispose();
            map.Remove(registration.Key);
        }
    }

    public void Clear()
    {
        foreach (var counter in map.Values) {
            if (counter.Item is IDisposable disposable)
                disposable.Dispose();
        }

        map.Clear();
    }

    private async Task<CounterCacheItem<TValue>> GetOrCreateAsync(TKey key, Func<TKey, Task<TValue>> createValueFunc)
    {
        if (map.TryGetValue(key, out var counter)) return counter;

        var value = await createValueFunc(key);
        counter = new CounterCacheItem<TValue>(value);
        map[key] = counter;

        return counter;
    }
}