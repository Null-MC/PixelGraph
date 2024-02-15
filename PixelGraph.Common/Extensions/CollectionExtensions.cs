using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using System.Threading.Tasks.Dataflow;

namespace PixelGraph.Common.Extensions;

internal static class CollectionExtensions
{
    public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> updateCollection, IDictionary<TKey, TValue> addCollection)
    {
        foreach (var (k, v) in addCollection)
            updateCollection[k] = v;
    }

    public static void Update<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var e in collection) action(e);
    }

    public static PackEncodingChannel? GetChannel(this IEnumerable<PackEncodingChannel> collection, string encodingChannel)
    {
        return collection.FirstOrDefault(c => EncodingChannel.Is(c.ID, encodingChannel));
    }

    public static bool HasChannel(this IEnumerable<PackEncodingChannel> collection, string encodingChannel)
    {
        return collection.Any(c => EncodingChannel.Is(c.ID, encodingChannel));
    }

    public static bool HasNormalChannels(this IEnumerable<PackEncodingChannel> collection)
    {
        return collection.Any(c => EncodingChannel.Is(c.ID, EncodingChannel.NormalX)
                                   || EncodingChannel.Is(c.ID, EncodingChannel.NormalY)
                                   || EncodingChannel.Is(c.ID, EncodingChannel.NormalZ));
    }

    //public static bool TryGetChannel<T>(this IEnumerable<ResourcePackChannelProperties> collection, out T channelProperties)
    //    where T : ResourcePackChannelProperties
    //{
    //    channelProperties = collection.OfType<T>().FirstOrDefault();
    //    return channelProperties != null;
    //}

    public static bool TryGetChannel(this IEnumerable<PackEncodingChannel> collection, string encodingChannel, out PackEncodingChannel? channelProperties)
    {
        channelProperties = collection.FirstOrDefault(c => EncodingChannel.Is(c.ID, encodingChannel));
        return channelProperties != null;
    }

    public static async Task AsyncParallelForEach<T>(this IEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler? scheduler = null, CancellationToken token = default)
    {
        var options = new ExecutionDataflowBlockOptions {
            CancellationToken = token,
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            BoundedCapacity = 32,
        };

        if (scheduler != null)
            options.TaskScheduler = scheduler;

        var block = new ActionBlock<T>(body, options);
        foreach (var item in source) {
            token.ThrowIfCancellationRequested();
            await block.SendAsync(item, token);
        }

        block.Complete();
        await block.Completion;
    }

    //public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null, CancellationToken token = default)
    //{
    //    var options = new ExecutionDataflowBlockOptions {
    //        CancellationToken = token,
    //        MaxDegreeOfParallelism = maxDegreeOfParallelism,
    //        BoundedCapacity = 32,
    //    };

    //    if (scheduler != null)
    //        options.TaskScheduler = scheduler;

    //    var block = new ActionBlock<T>(body, options);
    //    await foreach (var item in source.WithCancellation(token)) {
    //        await block.SendAsync(item, token);
    //    }

    //    block.Complete();
    //    await block.Completion;
    //}
}