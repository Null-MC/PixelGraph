using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Filtering
{
    internal class TextureFilter
    {
        private readonly PackProperties pack;


        public TextureFilter(PackProperties pack)
        {
            this.pack = pack;
        }

        public void Apply(Image image, PbrProperties texture, TextureEncoding encoding)
        {
            // TODO: apply modifications

            ApplyScaling(image, texture, encoding);

            Resize(image, texture);
        }

        private void ApplyScaling(Image image, PbrProperties texture, TextureEncoding encoding)
        {
            var scaleOptions = new ScaleProcessor.Options {
                Red = GetScale(texture, encoding.R),
                Green = GetScale(texture, encoding.G),
                Blue = GetScale(texture, encoding.B),
                Alpha = GetScale(texture, encoding.A),
            };

            if (!scaleOptions.Any) return;
            var processor = new ScaleProcessor(scaleOptions);
            image.Mutate(c => c.ApplyProcessor(processor));
        }

        private void Resize(Image image, PbrProperties texture)
        {
            if (!(texture?.ResizeEnabled ?? true)) return;
            if (!pack.TextureSize.HasValue && !pack.TextureScale.HasValue) return;

            var (width, height) = image.Size();

            var resampler = KnownResamplers.Bicubic;
            if (pack.Sampler != null && Samplers.TryParse(pack.Sampler, out var _resampler))
                resampler = _resampler;

            if (pack.TextureSize.HasValue) {
                if (width == pack.TextureSize) return;

                image.Mutate(c => c.Resize(pack.TextureSize.Value, 0, resampler));
            }
            else {
                var targetWidth = (int)Math.Max(width * pack.TextureScale.Value, 1f);
                var targetHeight = (int)Math.Max(height * pack.TextureScale.Value, 1f);

                image.Mutate(c => c.Resize(targetWidth, targetHeight, resampler));
            }
        }

        private static float GetScale(PbrProperties texture, string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 1f;
            return scaleMap.TryGetValue(channel, out var value) ? value(texture) : 1f;
        }

        private static readonly Dictionary<string, Func<PbrProperties, float>> scaleMap = new Dictionary<string, Func<PbrProperties, float>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.AlbedoR] = t => t.AlbedoScaleR,
            [EncodingChannel.AlbedoG] = t => t.AlbedoScaleG,
            [EncodingChannel.AlbedoB] = t => t.AlbedoScaleB,
            [EncodingChannel.AlbedoA] = t => t.AlbedoScaleA,
            [EncodingChannel.Height] = t => t.HeightScale,
            // AO
            [EncodingChannel.Smooth] = t => t.SmoothScale,
            [EncodingChannel.PerceptualSmooth] = t => t.SmoothScale, // TODO: scale in linear space?
            [EncodingChannel.Rough] = t => t.RoughScale,
            [EncodingChannel.Reflect] = t => t.ReflectScale,
            // Porosity-SSS
            [EncodingChannel.Emissive] = t => t.EmissiveScale,
        };
    }
}
