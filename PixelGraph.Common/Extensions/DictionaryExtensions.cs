using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Extensions
{
    internal static class DictionaryExtensions
    {
        //public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        //{
        //    return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        //}

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createFunc)
        {
            if (dictionary.TryGetValue(key, out var value)) return value;
            return dictionary[key] = createFunc();
        }

        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> sourceDictionary, IDictionary<TKey, TValue> updateDictionary)
        {
            foreach (var key in updateDictionary.Keys)
                sourceDictionary[key] = updateDictionary[key];
        }
    }
}
