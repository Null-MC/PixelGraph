using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.Extensions
{
    internal static class CollectionExtensions
    {
        public static void Update<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var e in collection) action(e);
        }

        public static bool HasChannel(this IEnumerable<ResourcePackChannelProperties> collection, string encodingChannel)
        {
            return collection.Any(c => EncodingChannel.Is(c.ID, encodingChannel));
        }
    }
}
