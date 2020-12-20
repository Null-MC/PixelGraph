using System;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal class ImagePreviewCache
    {
        private readonly Dictionary<string, ImagePreview> items;


        public ImagePreviewCache()
        {
            items = new Dictionary<string, ImagePreview>();
        }

        public async Task Image GetAsync(string localFile)
        {
            if (items.)
        }

        public void Release(string localFile)
        {
            if (!items.TryGetValue(localFile, out var item)) return;

            item.Dispose();
            items.Remove(localFile);
        }
    }

    internal class ImagePreview : IDisposable
    {
        public Image Image {get; set;}


        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
