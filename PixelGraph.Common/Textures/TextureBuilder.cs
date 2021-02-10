using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures
{
    public interface ITextureBuilder<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        ITextureGraph Graph {get; set;}
        MaterialContext Context {get; set;}
        ResourcePackChannelProperties[] InputChannels {get; set;}
        ResourcePackChannelProperties[] OutputChannels {get; set;}
        Image<TPixel> ImageResult {get;}

        Task BuildAsync(CancellationToken token = default);
    }

    internal class TextureBuilder<TPixel> : ITextureBuilder<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly IInputReader reader;
        private readonly List<TextureChannelMapping> mappings;
        private Rgba32 defaultValues;
        private Size bufferSize;
        private bool isGrayscale;

        public ITextureGraph Graph {get; set;}
        public MaterialContext Context {get; set;}
        public ResourcePackChannelProperties[] InputChannels {get; set;}
        public ResourcePackChannelProperties[] OutputChannels {get; set;}
        public Image<TPixel> ImageResult {get; private set;}


        public TextureBuilder(IInputReader reader)
        {
            this.reader = reader;

            mappings = new List<TextureChannelMapping>();
            defaultValues = new Rgba32();
        }

        public async Task BuildAsync(CancellationToken token = default)
        {
            defaultValues.R = defaultValues.G = defaultValues.B = defaultValues.A = 0;
            mappings.Clear();

            foreach (var channel in OutputChannels.Where(c => c.Color != ColorChannel.Magnitude)) {
                if (TryBuildMapping(channel, out var mapping)) mappings.Add(mapping);
            }

            if (!Context.CreateEmpty && mappings.Count == 0) return;

            isGrayscale = mappings.All(x => x.OutputColor == ColorChannel.Red);

            bufferSize = await GetBufferSizeAsync(token);

            foreach (var mappingGroup in mappings.GroupBy(m => m.SourceFilename)) {
                if (mappingGroup.Key == null) continue;
                
                await ApplySourceMappingAsync(mappingGroup.Key, mappingGroup.ToArray(), token);
            }

            foreach (var mapping in mappings.Where(m => m.SourceFilename == null))
                await ApplyMappingAsync(mapping, token);

            if (ImageResult == null && Context.CreateEmpty)
                CreateImageResult();
        }

        public void Dispose()
        {
            ImageResult?.Dispose();
        }

        private bool TryBuildMapping(ResourcePackChannelProperties outputChannel, out TextureChannelMapping mapping)
        {
            var samplerName = outputChannel.Sampler ?? Context.Profile?.Encoding?.Sampler ?? Sampler.Nearest;

            mapping = new TextureChannelMapping {
                OutputColor = outputChannel.Color ?? ColorChannel.None,
                OutputMinValue = (double?)outputChannel.MinValue ?? 0f,
                OutputMaxValue = (double?)outputChannel.MaxValue ?? 1f,
                OutputRangeMin = outputChannel.RangeMin ?? 0,
                OutputRangeMax = outputChannel.RangeMax ?? 255,
                OutputShift = outputChannel.Shift ?? 0,
                OutputPower = (double?)outputChannel.Power ?? 1,
                //OutputPerceptual = outputChannel.Perceptual ?? false,
                OutputInverted = outputChannel.Invert ?? false,
                OutputSampler = samplerName,

                ValueShift = Context.Material.GetChannelShift(outputChannel.ID),
                ValueScale = Context.Material.GetChannelScale(outputChannel.ID),
            };

            var inputChannel = InputChannels.FirstOrDefault(i
                => EncodingChannel.Is(i.ID, outputChannel.ID));

            if (Context.Material.TryGetChannelValue(outputChannel.ID, out var value)) {
                mapping.InputValue = value;
                mapping.ApplyInputChannel(inputChannel);
                return true;
            }

            if (inputChannel?.Texture != null) {
                if (TextureTags.Is(inputChannel.Texture, TextureTags.NormalGenerated)) {
                    mapping.SourceTag = TextureTags.NormalGenerated;
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (TryGetSourceFilename(inputChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(inputChannel);
                    return true;
                }

                if (Context.AutoGenerateOcclusion && TextureTags.Is(inputChannel.Texture, TextureTags.Occlusion)) {
                    mapping.SourceTag = TextureTags.OcclusionGenerated;
                    mapping.InputColor = ColorChannel.Red;
                    //mapping.InputSampler = inputChannel.Sampler;
                    mapping.InputMinValue = 0f;
                    mapping.InputMaxValue = 1f;
                    mapping.InputRangeMin = 0;
                    mapping.InputRangeMax = 255;
                    mapping.InputShift = 0;
                    mapping.InputPower = 1;
                    //mapping.InputPerceptual = false;
                    mapping.InputInverted = true;
                    return true;
                }
            }

            var isOutputSmooth = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Smooth);
            var isOutputRough = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Rough);

            // Rough > Smooth
            if (isOutputSmooth && TryGetInputChannel(EncodingChannel.Rough, out var roughChannel)
                               && TryGetSourceFilename(roughChannel.Texture, out mapping.SourceFilename)) {
                mapping.ApplyInputChannel(roughChannel);
                mapping.InputInverted = true;
                return true;
            }

            // Smooth > Rough
            if (isOutputRough && TryGetInputChannel(EncodingChannel.Smooth, out var smoothChannel)) {
                if (TryGetSourceFilename(smoothChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InputInverted = true;
                    return true;
                }
            }

            //var isOutputF0 = EncodingChannel.Is(outputChannel.ID, EncodingChannel.F0);
            //var isOutputMetal = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Metal);

            //// Metal > F0
            //if (isOutputF0 && TryGetInputChannel(EncodingChannel.Metal, out var metalChannel)) {
            //    if (TryGetSourceFilename(metalChannel.Texture, out mapping.SourceFilename)) {
            //        mapping.ApplyInputChannel(metalChannel);
            //        //mapping.Threshold = 0.5f;
            //        mapping.IsMetalToF0 = true;
            //        return true;
            //    }
            //}

            //// F0 > Metal
            //if (isOutputMetal && TryGetInputChannel(EncodingChannel.F0, out var f0Channel)) {
            //    if (TryGetSourceFilename(f0Channel.Texture, out mapping.SourceFilename)) {
            //        mapping.ApplyInputChannel(f0Channel);
            //        //mapping.Threshold = 0.5f;
            //        mapping.IsF0ToMetal = true;
            //        return true;
            //    }
            //}

            return Context.CreateEmpty;
        }

        private bool TryGetInputChannel(string id, out ResourcePackChannelProperties channel)
        {
            channel = InputChannels.FirstOrDefault(i => EncodingChannel.Is(i.ID, id));
            return channel != null;
        }

        private async Task ApplyMappingAsync(TextureChannelMapping mapping, CancellationToken token)
        {
            if (mapping.SourceTag != null) {
                var options = new OverlayProcessor<Rgb24>.Options {
                    Mappings = new []{mapping},
                    IsGrayscale = isGrayscale,
                };

                if (TextureTags.Is(mapping.SourceTag, TextureTags.NormalGenerated)) {
                    if (Graph.NormalTexture.Width == bufferSize.Width && Graph.NormalTexture.Height == bufferSize.Height) {
                        options.Source = Graph.NormalTexture;
                    }
                    else {
                        var samplerName = mapping.OutputSampler ?? Context.Profile?.Encoding?.Sampler ?? Sampler.Nearest;
                        var sampler = Sampler<Rgb24>.Create(samplerName);
                        sampler.Image = Graph.NormalTexture;
                        sampler.WrapX = Context.Material.WrapX ?? MaterialProperties.DefaultWrap;
                        sampler.WrapY = Context.Material.WrapY ?? MaterialProperties.DefaultWrap;
                        sampler.RangeX = (float)Graph.NormalTexture.Width / bufferSize.Width;
                        sampler.RangeY = (float)Graph.NormalTexture.Height / bufferSize.Height;

                        options.SamplerMap = new Dictionary<ColorChannel, ISampler<Rgb24>> {
                            [ColorChannel.Red] = sampler,
                            [ColorChannel.Green] = sampler,
                            [ColorChannel.Blue] = sampler,
                        };
                    }
                }
                else if (TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated)) {
                    options.Source = await Graph.GetGeneratedOcclusionAsync(token);
                }

                if (options.Source != null) {
                    if (ImageResult == null) {
                        //var size = options.Source.Size();
                        CreateImageResult();
                    }

                    if (options.Source.Width != ImageResult.Width || options.Source.Height != ImageResult.Height) {
                        ApplySamplers(options, new [] {mapping}, options.Source);
                    }

                    var processor = new OverlayProcessor<Rgb24>(options);
                    ImageResult.Mutate(context => context.ApplyProcessor(processor));
                    return;
                }
            }

            if (!mapping.InputValue.HasValue && mapping.OutputShift == 0 && !mapping.OutputInverted) return;

            var value = (double?)mapping.InputValue ?? 0d;

            if (value < mapping.InputMinValue || value > mapping.InputMaxValue) return;

            // Common Processing
            value = (value + mapping.ValueShift) * mapping.ValueScale;

            //if (mapping.OutputPerceptual)
            //    MathEx.LinearToPerceptual(ref value);

            if (!mapping.OutputPower.Equal(1))
                value = Math.Pow(value, mapping.OutputPower);

            if (mapping.OutputInverted) MathEx.Invert(ref value, mapping.OutputMinValue, mapping.OutputMaxValue);

            MathEx.Clamp(ref value, mapping.OutputMinValue, mapping.OutputMaxValue);

            var valueRange = mapping.OutputMaxValue - mapping.OutputMinValue;
            var pixelRange = mapping.OutputRangeMax - mapping.OutputRangeMin;
            var outputScale = pixelRange / valueRange;

            var valueOut = mapping.OutputRangeMin + (value - mapping.OutputMinValue) * outputScale;
            var finalValue = MathEx.ClampRound(valueOut, mapping.OutputRangeMin, mapping.OutputRangeMax);

            if (mapping.OutputShift != 0)
                MathEx.Cycle(ref finalValue, in mapping.OutputShift, in mapping.OutputRangeMin, in mapping.OutputRangeMax);

            if (ImageResult != null) {
                var options = new OverwriteProcessor.Options {
                    IsGrayscale = isGrayscale,
                    Color = mapping.OutputColor,
                    Min = mapping.OutputRangeMin,
                    Max = mapping.OutputRangeMax,
                    Value = finalValue,
                };

                var processor = new OverwriteProcessor(options);
                ImageResult.Mutate(context => context.ApplyProcessor(processor));
            }
            else {
                if (isGrayscale) {
                    defaultValues.R = finalValue;
                    defaultValues.G = finalValue;
                    defaultValues.B = finalValue;
                }
                else {
                    defaultValues.SetChannelValue(in mapping.OutputColor, in finalValue);
                }
            }
        }

        private async Task ApplySourceMappingAsync(string sourceFilename, TextureChannelMapping[] mappingGroup, CancellationToken token)
        {
            if (sourceFilename == null) throw new ArgumentNullException(nameof(sourceFilename));

            await using var sourceStream = reader.Open(sourceFilename);
            using var sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);

            var options = new OverlayProcessor<Rgba32>.Options {
                Mappings = mappingGroup,
                IsGrayscale = isGrayscale,
                Source = sourceImage,
            };

            if (ImageResult == null) {
                CreateImageResult();
            }

            if (sourceImage.Width != ImageResult.Width || sourceImage.Height != ImageResult.Height) {
                ApplySamplers(options, mappingGroup, sourceImage);
            }

            var processor = new OverlayProcessor<Rgba32>(options);
            ImageResult.Mutate(context => context.ApplyProcessor(processor));
        }

        private async Task<Size> GetBufferSizeAsync(CancellationToken token = default)
        {
            var bufferWidth = Context.GetBufferSize();

            if (bufferWidth.HasValue) {
                if (Context.Material.TryGetSourceBounds(out var bounds)) {
                    var aspect = (float)bounds.Height / bounds.Width;

                    var width = bufferWidth.Value;
                    var height = (int)MathF.Ceiling(bufferWidth.Value * aspect);
                    return new Size(width, height);
                }

                return new Size(bufferWidth.Value);
            }

            var actualBounds = await GetActualBoundsAsync(token);
            if (actualBounds.HasValue)
                return actualBounds.Value;

            return new Size(Context.DefaultTextureSize);
        }

        private async Task<Size?> GetActualBoundsAsync(CancellationToken token = default)
        {
            var hasBounds = false;
            var maxWidth = 1;
            var maxHeight = 1;

            foreach (var mappingGroup in mappings.GroupBy(m => m.SourceFilename)) {
                if (mappingGroup.Key == null) continue;

                await using var sourceStream = reader.Open(mappingGroup.Key);
                var info = await Image.IdentifyAsync(Configuration.Default, sourceStream, token);
                if (hasBounds && info.Width <= maxWidth && info.Height <= maxHeight) continue;

                Expand(ref maxWidth, ref maxHeight, info.Width, info.Height);
                hasBounds = true;
            }

            //if (maxWidth > 1 || maxHeight > 1)
            //    Context.ApplyTargetTextureScale(ref maxSize);

            foreach (var mapping in mappings.Where(m => m.SourceFilename == null)) {
                if (TextureTags.Is(mapping.SourceTag, TextureTags.NormalGenerated)) {
                    if (hasBounds && Graph.NormalTexture.Width <= maxWidth && Graph.NormalTexture.Height <= maxHeight) continue;

                    Expand(ref maxWidth, ref maxHeight, Graph.NormalTexture.Width, Graph.NormalTexture.Height);
                    hasBounds = true;
                }

                if (TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated)) {
                    var occlusionTex = await Graph.GetGeneratedOcclusionAsync(token);
                    if (occlusionTex != null) {
                        if (hasBounds && occlusionTex.Width <= maxWidth && occlusionTex.Height <= maxHeight) continue;

                        Expand(ref maxWidth, ref maxHeight, occlusionTex.Width, occlusionTex.Height);
                        hasBounds = true;
                    }
                }
            }

            //if (maxWidth <= 1 && maxHeight <= 1)
            //    return Context.GetDefaultTextureSize();

            if (!hasBounds) return null;
            return new Size(maxWidth, maxHeight);
        }

        private void ApplySamplers<TP>(OverlayProcessor<TP>.Options options, IEnumerable<TextureChannelMapping> mappingGroup, Image<TP> sourceImage)
            where TP : unmanaged, IPixel<TP>
        {
            options.SamplerMap = new Dictionary<ColorChannel, ISampler<TP>>();

            foreach (var mapping in mappingGroup) {
                var samplerName = mapping.OutputSampler ?? Graph.Context.DefaultSampler;
                var sampler = Sampler<TP>.Create(samplerName);
                sampler.Image = sourceImage;
                sampler.WrapX = Context.Material.WrapX ?? MaterialProperties.DefaultWrap;
                sampler.WrapY = Context.Material.WrapY ?? MaterialProperties.DefaultWrap;

                //sampler.RangeX = sampler.RangeY = 1f; // TODO: Set to target size / source size
                sampler.RangeX = (float)sourceImage.Width / bufferSize.Width;
                sampler.RangeY = (float)sourceImage.Height / bufferSize.Height;

                options.SamplerMap[mapping.InputColor] = sampler;
            }
        }

        private void CreateImageResult()
        {
            var pixel = new TPixel();
            pixel.FromRgba32(defaultValues);
            ImageResult = new Image<TPixel>(Configuration.Default, bufferSize.Width, bufferSize.Height, pixel);
        }

        private static void Expand(ref int maxWidth, ref int maxHeight, in int texWidth, in int texHeight)
        {
            var scaleX = (float) texWidth / maxWidth;
            var scaleY = (float) texHeight / maxHeight;
            var scale = Math.Max(scaleX, scaleY);

            maxWidth = (int) Math.Ceiling(maxWidth * scale);
            maxHeight = (int) Math.Ceiling(maxHeight * scale);
        }

        private bool TryGetSourceFilename(string tag, out string filename)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            foreach (var file in reader.EnumerateTextures(Context.Material, tag)) {
                if (!reader.FileExists(file)) continue;

                filename = file;
                return true;
            }

            filename = null;
            return false;
        }
    }

    internal class TextureChannelMapping
    {
        public ColorChannel InputColor;
        //public string InputSampler;
        public decimal? InputValue;
        public double InputMinValue;
        public double InputMaxValue;
        public byte InputRangeMin;
        public byte InputRangeMax;
        public int InputShift;
        public double InputPower;
        //public bool InputPerceptual;
        public bool InputInverted;

        public ColorChannel OutputColor;
        public string OutputSampler;
        public double OutputMinValue;
        public double OutputMaxValue;
        public byte OutputRangeMin;
        public byte OutputRangeMax;
        public int OutputShift;
        public double OutputPower;
        //public bool OutputPerceptual;
        public bool OutputInverted;

        public string SourceTag;
        public string SourceFilename;
        public float ValueShift;
        public float ValueScale;
        //public bool IsMetalToF0;
        //public bool IsF0ToMetal;


        public void ApplyInputChannel(ResourcePackChannelProperties channel)
        {
            InputColor = channel.Color ?? ColorChannel.None;
            //InputSampler = channel.Sampler;
            InputMinValue = (double?)channel.MinValue ?? 0d;
            InputMaxValue = (double?)channel.MaxValue ?? 1d;
            InputRangeMin = channel.RangeMin ?? 0;
            InputRangeMax = channel.RangeMax ?? 255;
            InputShift = channel.Shift ?? 0;
            InputPower = (double?)channel.Power ?? 1d;
            //InputPerceptual = channel.Perceptual ?? false;
            InputInverted = channel.Invert ?? false;
        }
    }
}
