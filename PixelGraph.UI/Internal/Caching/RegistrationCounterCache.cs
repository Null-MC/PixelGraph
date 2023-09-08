using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Caching;

internal abstract class RegistrationCounterCache<TKey, TValue>
{
    private readonly Dictionary<TKey, CounterCacheItem<TValue>> map;


    protected RegistrationCounterCache(IEqualityComparer<TKey> keyComparer)
    {
        map = new Dictionary<TKey, CounterCacheItem<TValue>>(keyComparer);
    }

    protected CacheRegistration<TKey, TValue> Register(TKey key, Func<TKey, TValue> createFunc)
    {
        var registrationId = Guid.NewGuid();
        var counter = GetOrCreate(key, createFunc);
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

    private CounterCacheItem<TValue> GetOrCreate(TKey key, Func<TKey, TValue> createValueFunc)
    {
        if (map.TryGetValue(key, out var counter)) return counter;

        var value = createValueFunc(key);
        counter = new CounterCacheItem<TValue>(value);
        map[key] = counter;

        return counter;
    }
}

internal class CounterCacheItem<T>
{
    public T Item {get;}
    public List<Guid> Registrations {get;}


    public CounterCacheItem(T item)
    {
        Item = item;

        Registrations = new List<Guid>();
    }
}

public class CacheRegistration<TKey, TValue>
{
    public TKey Key {get;}
    public TValue Value {get;}
    public Guid RegistrationId {get;}


    public CacheRegistration(TKey key, TValue data, Guid registrationId)
    {
        Key = key;
        Value = data;
        RegistrationId = registrationId;
    }
}