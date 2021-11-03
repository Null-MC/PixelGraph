using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
    internal class GenericTexturePublisher
    {
        protected ResourcePackProfileProperties Pack {get;}
        protected IInputReader Reader {get;}
        protected IOutputWriter Writer {get;}


        public GenericTexturePublisher(
            ResourcePackProfileProperties pack,
            IInputReader reader,
            IOutputWriter writer)
        {
            Pack = pack;
            Reader = reader;
            Writer = writer;
        }

        //public async Task PublishAsync(string filename, CancellationToken token)
        //{
        //    var path = Path.GetDirectoryName(filename);
        //    var name = Path.GetFileNameWithoutExtension(filename);
        //    var newName = $"{name}.png";

        //    var destinationFile = path == null ? newName : PathEx.Join(path, newName);

        //    await PublishAsync(filename, null, destinationFile, token);
        //}

        public async Task PublishAsync(string sourceFile, Rgba32? sourceColor, string destinationFile, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(destinationFile))
                throw new ArgumentException("Value cannot be null or empty!", nameof(destinationFile));

            using var sourceImage = await LoadSourceImageAsync(sourceFile, sourceColor, token);
            using var resizedImage = Resize(sourceImage);

            await Writer.OpenAsync(destinationFile, async stream => {
                await (resizedImage ?? sourceImage).SaveAsPngAsync(stream, token);
            }, token);
        }

        protected Image Resize<TPixel>(Image<TPixel> source)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (!Pack.TextureSize.HasValue && !Pack.TextureScale.HasValue) return null;

            var samplerName = Pack.Encoding?.Sampler ?? Samplers.Samplers.Nearest;
            var packSampler = Sampler<TPixel>.Create(samplerName);
            packSampler.Image = source;
            packSampler.WrapX = false;
            packSampler.WrapY = false;
            packSampler.Bounds = new RectangleF(0f, 0f, 1f, 1f);

            var (width, height) = source.Size();

            var options = new ResizeProcessor<TPixel>.Options {
                Sampler = packSampler,
            };

            int targetWidth, targetHeight;
            if (Pack.TextureSize.HasValue) {
                // Preserve aspect
                if (source.Width == Pack.TextureSize.Value) return null;

                var aspect = height / (float) width;
                targetWidth = Pack.TextureSize.Value;
                targetHeight = (int)(Pack.TextureSize.Value * aspect);
                packSampler.RangeX = source.Width / (float)Pack.TextureSize.Value;
                packSampler.RangeY = source.Height / (float)Pack.TextureSize.Value;
            }
            else {
                // scale all
                var scale = (float)Pack.TextureScale.Value;
                targetWidth = (int)Math.Max(width * scale, 1f);
                targetHeight = (int)Math.Max(height * scale, 1f);
                packSampler.RangeX = 1f / scale;
                packSampler.RangeY = 1f / scale;
            }

            var processor = new ResizeProcessor<TPixel>(options);
            var resizedImage = new Image<Rgba32>(Configuration.Default, targetWidth, targetHeight);

            try {
                resizedImage.Mutate(c => c.ApplyProcessor(processor));
                return resizedImage;
            }
            catch {
                resizedImage.Dispose();
                throw;
            }
        }

        private async Task<Image<Rgba32>> LoadSourceImageAsync(string sourceFile, Rgba32? sourceColor, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(sourceFile)) {
                await using var stream = Reader.Open(sourceFile);
                return await Image.LoadAsync<Rgba32>(Configuration.Default, stream, token);
            }

            if (sourceColor.HasValue) {
                // TODO: support texture sizing?

                return new Image<Rgba32>(Configuration.Default, 1, 1, sourceColor.Value);
            }

            throw new SourceEmptyException("No Source image was found, and no color is defined!");
        }
    }
}
