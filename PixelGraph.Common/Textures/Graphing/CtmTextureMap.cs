using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PixelGraph.Common.Textures.Graphing
{
    internal class CtmTextureMap
    {
        private readonly IOutputWriter writer;
        private readonly IImageWriter imageWriter;
        private readonly Dictionary<string, CtmTextureMapTag> map;


        public CtmTextureMap(
            IOutputWriter writer,
            IImageWriter imageWriter)
        {
            this.writer = writer;
            this.imageWriter = imageWriter;

            map = new Dictionary<string, CtmTextureMapTag>(StringComparer.InvariantCultureIgnoreCase);
        }

        public async Task WriteTileAsync<TSource>(string textureTag, Image<TSource> image, ImageChannels type, string localFile, Point location, CancellationToken token = default)
            where TSource : unmanaged, IPixel<TSource>
        {
            if (!map.TryGetValue(textureTag, out var tagMap))
                throw new ApplicationException($"No CTM mapping found for tag '{textureTag}'!");

            //if (!tagMap.IsCreated) await tagMap.CreateAsync();

            //var ext = Path.GetExtension(tagMap.LocalFilename)?.TrimStart('.');
            //var encoder = GetEncoder(ext, type);
            
            var tileBounds = new Rectangle {
                X = location.X,
                Y = location.Y,
                Width = image.Width,
                Height = image.Height,
            };

            var options = new CopyRegionProcessor<TSource>.Options {
                SourceImage = image,
            };

            await using var stream = writer.OpenReadWrite(tagMap.LocalFilename);

            Image ctmImage = null;
            try {
                if (!tagMap.IsCreated) {
                    ctmImage = new Image<Rgba32>(Configuration.Default, tagMap.TotalWidth, tagMap.TotalHeight);
                    tagMap.IsCreated = true;
                }
                else {
                    ctmImage = await LoadImageAsync(stream, type, token);
                }

                var processor = new CopyRegionProcessor<TSource>(options);
                ctmImage.Mutate(context => context.ApplyProcessor(processor, tileBounds));

                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);

                await imageWriter.WriteAsync(image, type, tagMap.LocalFilename, token);
            }
            finally {
                ctmImage?.Dispose();
            }
        }

        private static async Task<Image> LoadImageAsync(Stream stream, ImageChannels type, CancellationToken token)
        {
            switch (type) {
                case ImageChannels.Gray:
                    return await Image.LoadAsync<L16>(Configuration.Default, stream, token);
                case ImageChannels.Color:
                    return await Image.LoadAsync<Rgb24>(Configuration.Default, stream, token);
                case ImageChannels.ColorAlpha:
                    return await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                default:
                    throw new ApplicationException();
            }
        }
    }

    internal class CtmTextureMapTag
    {
        public string LocalFilename {get; set;}
        public bool IsCreated {get; set;}
        public int TotalWidth {get; set;}
        public int TotalHeight {get; set;}


        //public async Task<Image<TPixel>> WriteAsync<TPixel>()
        //    where TPixel : unmanaged, IPixel<TPixel>
        //{
        //    var ext = Path.GetExtension(LocalFilename)?.TrimStart('.');

        //    var encoder = GetEncoder(ext, type);

        //    var tileBounds = new Rectangle {
        //        X = location.X,
        //        Y = location.Y,
        //        Width = image.Width,
        //        Height = image.Height,
        //    };

        //    await using var stream = writer.OpenReadWrite(LocalFilename);

        //    if (!IsCreated) {
        //        stream.SetLength(0);
        //        image = new Image<TPixel>();
        //        IsCreated = true;
        //    }
        //    else {
        //        return await LoadImageAsync(stream, type, token);
        //        //
        //    }

        //}

        private async Task CreateAsync()
        {
            //...
            IsCreated = true;
        }

        private static async Task<Image> LoadImageAsync(Stream stream, ImageChannels type, CancellationToken token)
        {
            switch (type) {
                case ImageChannels.Gray:
                    return await Image.LoadAsync<L16>(Configuration.Default, stream, token);
                case ImageChannels.Color:
                    return await Image.LoadAsync<Rgb24>(Configuration.Default, stream, token);
                case ImageChannels.ColorAlpha:
                    return await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
                default:
                    throw new ApplicationException();
            }
        }
    }
}
