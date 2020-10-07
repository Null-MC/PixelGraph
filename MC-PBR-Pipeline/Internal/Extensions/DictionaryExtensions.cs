using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Extensions
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createFunc)
        {
            if (dictionary.TryGetValue(key, out var value)) return value;
            return dictionary[key] = createFunc();
        }
    }
}
