﻿using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal abstract class TexturePublisherBase
    {
        protected PackProperties Pack {get;}
        protected IInputReader Reader {get;}
        protected IOutputWriter Writer {get;}


        protected TexturePublisherBase(
            PackProperties pack,
            IInputReader reader,
            IOutputWriter writer)
        {
            Pack = pack;
            Reader = reader;
            Writer = writer;
        }

        protected async Task PublishAsync(string sourceFile, Rgba32? sourceColor, string destinationFile, Action<IImageProcessingContext> processAction, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(destinationFile))
                throw new ArgumentException("Value cannot be null or empty!", nameof(destinationFile));

            using var sourceImage = await LoadSourceImageAsync(sourceFile, sourceColor, token);
            using var targetImage = new Image<Rgba32>(Configuration.Default, sourceImage.Width, sourceImage.Height);

            var brush = new ImageBrush(sourceImage);
            targetImage.Mutate(c => c.Clear(brush));
            targetImage.Mutate(processAction);

            Writer.Prepare();

            await using var stream = Writer.WriteFile(destinationFile);
            await targetImage.SaveAsPngAsync(stream, token);
        }

        protected void Resize(IImageProcessingContext context, PbrProperties texture)
        {
            if (!(texture?.ResizeEnabled ?? true)) return;
            if (!Pack.TextureSize.HasValue && !Pack.TextureScale.HasValue) return;

            var (width, height) = context.GetCurrentSize();

            var resampler = KnownResamplers.Bicubic;
            if (Pack.Sampler != null && Samplers.TryParse(Pack.Sampler, out var _resampler))
                resampler = _resampler;

            if (Pack.TextureSize.HasValue) {
                if (width == Pack.TextureSize) return;

                context.Resize(Pack.TextureSize.Value, 0, resampler);
            }
            else {
                var targetWidth = (int)Math.Max(width * Pack.TextureScale.Value, 1f);
                var targetHeight = (int)Math.Max(height * Pack.TextureScale.Value, 1f);

                context.Resize(targetWidth, targetHeight, resampler);
            }
        }

        private async Task<Image> LoadSourceImageAsync(string sourceFile, Rgba32? sourceColor, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(sourceFile)) {
                await using var stream = Reader.Open(sourceFile);
                return await Image.LoadAsync(Configuration.Default, stream, token);
            }

            if (sourceColor.HasValue) {
                // TODO: support texture sizing?

                return new Image<Rgba32>(Configuration.Default, 1, 1, sourceColor.Value);
            }

            throw new SourceEmptyException("No Source image was found, and no color is defined!");
        }
    }
}
