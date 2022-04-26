using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixelGraph.UI.Internal.Extensions
{
    internal static class ObservableCollectionExtensions
    {
        public static void Update<T>(this ObservableCollection<T> collection, IList<T> items)
        {
            var count = collection.Count;
            for (var i = count - 1; i >= 0; i--) {
                if (!items.Contains(collection[i]))
                    collection.RemoveAt(i);
            }

            count = items.Count;
            for (var i = 0; i < count; i++) {
                if (!collection.Contains(items[i]))
                    collection.Add(items[i]);
            }
        }
    }
}
