using PixelGraph.Common.ResourcePack;
using System;
using System.Collections.Generic;
using System.Linq;
using PixelGraph.Common.TextureFormats;

namespace PixelGraph.Common.Extensions
{
    internal static class CollectionExtensions
    {
        public static void Update<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var e in collection) action(e);
        }

        public static ResourcePackChannelProperties GetChannel(this IEnumerable<ResourcePackChannelProperties> collection, string encodingChannel)
        {
            return collection.FirstOrDefault(c => EncodingChannel.Is(c.ID, encodingChannel));
        }

        public static bool HasChannel(this IEnumerable<ResourcePackChannelProperties> collection, string encodingChannel)
        {
            return collection.Any(c => EncodingChannel.Is(c.ID, encodingChannel));
        }

        public static bool TryGetChannel<T>(this IEnumerable<ResourcePackChannelProperties> collection, out T channelProperties)
            where T : ResourcePackChannelProperties
        {
            channelProperties = collection.OfType<T>().FirstOrDefault();
            return channelProperties != null;
        }

        public static bool TryGetChannel(this IEnumerable<ResourcePackChannelProperties> collection, string encodingChannel, out ResourcePackChannelProperties channelProperties)
        {
            channelProperties = collection.FirstOrDefault(c => EncodingChannel.Is(c.ID, encodingChannel));
            return channelProperties != null;
        }
    }
}
