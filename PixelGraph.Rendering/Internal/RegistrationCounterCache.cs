using System;
using System.Collections.Generic;

namespace PixelGraph.Rendering.Internal
{
    internal abstract class RegistrationCounterCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, CounterCacheItem<TValue>> map;


        protected RegistrationCounterCache(IEqualityComparer<TKey> keyComparer)
        {
            map = new Dictionary<TKey, CounterCacheItem<TValue>>(keyComparer);
        }

        protected CacheRegistration<TValue> Register(TKey key, Func<TValue> createFunc)
        {
            var registrationId = Guid.NewGuid();
            var counter = GetOrCreate(key, createFunc);
            counter.Registrations.Add(registrationId);

            return new CacheRegistration<TValue>(registrationId, counter.Item);
        }

        protected void Release(TKey key, CacheRegistration<TValue> registration)
        {
            if (!map.TryGetValue(key, out var counter)) return;
            if (!counter.Registrations.Remove(registration.Id)) return;

            if (counter.Registrations.Count == 0) {
                if (counter.Item is IDisposable disposable) disposable.Dispose();
                map.Remove(key);
            }
        }

        private CounterCacheItem<TValue> GetOrCreate(TKey key, Func<TValue> createValueFunc)
        {
            if (map.TryGetValue(key, out var counter)) return counter;

            var value = createValueFunc();
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

    internal class CacheRegistration<T>
    {
        public Guid Id {get;}
        public T Data {get;}


        public CacheRegistration(Guid id, T data)
        {
            Id = id;
            Data = data;
        }
    }
}
