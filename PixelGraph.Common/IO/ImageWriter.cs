using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public interface IImageWriter
    {
        //IImageEncoder GetEncoder(string format);
        Task WriteAsync(Image image, string localFile, string format, CancellationToken token);
    }

    internal class ImageWriter : IImageWriter
    {
        private readonly Dictionary<string, Lazy<IImageEncoder>> map;
        private readonly IOutputWriter writer;

        //public ResourcePackOutputProperties Output {get; set;}


        public ImageWriter(IOutputWriter writer)
        {
            this.writer = writer;

            map = new Dictionary<string, Lazy<IImageEncoder>>(StringComparer.InvariantCultureIgnoreCase) {
                ["bmp"] = new Lazy<IImageEncoder>(() => new BmpEncoder()),
                ["png"] = new Lazy<IImageEncoder>(() => new PngEncoder()),
                ["tga"] = new Lazy<IImageEncoder>(() => new TgaEncoder()),
                ["gif"] = new Lazy<IImageEncoder>(() => new GifEncoder()),
            };
        }

        public async Task WriteAsync(Image image, string localFile, string format, CancellationToken token)
        {
            var encoder = GetEncoder(format);
            await using var stream = writer.Open(localFile);
            await image.SaveAsync(stream, encoder, token);
        }

        private IImageEncoder GetEncoder(string format)
        {
            if (map.TryGetValue(format, out var mapLazy)) return mapLazy.Value;
            throw new ApplicationException($"Unsupported image encoding '{format}'!");
        }
    }
}
