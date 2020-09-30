using McPbrPipeline.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Filtering
{
    internal class FilterChain
    {
        private readonly List<IImageFilter> filterList;

        public string SourceFilename {get; set;}
        public string DestinationFilename {get; set;}
        public Rgba32? SourceColor {get; set;}


        public FilterChain()
        {
            filterList = new List<IImageFilter>();
        }

        public void Append(IImageFilter filter)
        {
            filterList.Add(filter);
        }

        public async Task ApplyAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(DestinationFilename))
                throw new ArgumentException("Value cannot be null or empty!", nameof(DestinationFilename));

            using var sourceImage = await LoadSourceImageAsync(token);
            using var targetImage = new Image<Rgba32>(Configuration.Default, sourceImage.Width, sourceImage.Height);

            var brush = new ImageBrush(sourceImage);
            targetImage.Mutate(c => c.Clear(brush));

            targetImage.Mutate(context => {
                foreach (var filter in filterList)
                    filter.Apply(context);
            });

            var path = Path.GetDirectoryName(DestinationFilename);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            await targetImage.SaveAsPngAsync(DestinationFilename, token);
        }

        private async Task<Image> LoadSourceImageAsync(CancellationToken token)
        {
            if (!string.IsNullOrEmpty(SourceFilename))
                return await Image.LoadAsync(SourceFilename, token);

            if (SourceColor.HasValue)
                return new Image<Rgba32>(1, 1, SourceColor.Value);

            throw new SourceEmptyException("No Source image was found, and no color is defined!");
        }
    }
}
