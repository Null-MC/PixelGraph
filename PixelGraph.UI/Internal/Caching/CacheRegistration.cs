namespace PixelGraph.UI.Internal.Caching;

public class CacheRegistration<TKey, TValue>(TKey key, TValue data, Guid registrationId)
{
    public TKey Key {get;} = key;
    public TValue Value {get;} = data;
    public Guid RegistrationId {get;} = registrationId;
}

internal class CounterCacheItem<T>(T item)
{
    public T Item {get;} = item;
    public List<Guid> Registrations {get;} = [];
}
