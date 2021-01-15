using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
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
        MaterialProperties Material {get; set;}
        ResourcePackChannelProperties[] InputChannels {get; set;}
        ResourcePackChannelProperties[] OutputChannels {get; set;}
        Image<TPixel> ImageResult {get;}
        Size DefaultSize {get; set;}
        bool AutoGenerateOcclusion {get; set;}
        bool CreateEmpty {get; set;}

        Task BuildAsync(CancellationToken token = default);
    }

    internal class TextureBuilder<TPixel> : ITextureBuilder<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly IInputReader reader;
        private Rgba32 defaultValues;
        private bool isGrayscale;

        public ITextureGraph Graph {get; set;}
        public MaterialProperties Material {get; set;}
        public ResourcePackChannelProperties[] InputChannels {get; set;}
        public ResourcePackChannelProperties[] OutputChannels {get; set;}
        public Size DefaultSize {get; set;}
        public Image<TPixel> ImageResult {get; private set;}
        public bool AutoGenerateOcclusion {get; set;}
        public bool CreateEmpty {get; set;}


        public TextureBuilder(IInputReader reader)
        {
            this.reader = reader;

            defaultValues = new Rgba32();
            DefaultSize = new Size(1);
            CreateEmpty = true;
        }

        public async Task BuildAsync(CancellationToken token = default)
        {
            var mappings = new List<TextureChannelMapping>();

            foreach (var channel in OutputChannels.Where(c => c.Color != ColorChannel.Magnitude)) {
                if (TryBuildMapping(channel, out var mapping)) mappings.Add(mapping);
            }

            if (!CreateEmpty && mappings.Count == 0) return;

            isGrayscale = mappings.All(x => x.OutputColor == ColorChannel.Red);

            foreach (var mappingGroup in mappings.GroupBy(m => m.SourceFilename)) {
                if (mappingGroup.Key == null) continue;
                
                await ApplySourceMappingAsync(mappingGroup.Key, mappingGroup, token);
            }

            foreach (var mapping in mappings.Where(m => m.SourceFilename == null))
                await ApplyMappingAsync(mapping, token);

            if (ImageResult == null)
                CreateImageResult(DefaultSize);
        }

        public void Dispose()
        {
            ImageResult?.Dispose();
        }

        private bool TryBuildMapping(ResourcePackChannelProperties outputChannel, out TextureChannelMapping mapping)
        {
            mapping = new TextureChannelMapping {
                OutputColor = outputChannel.Color ?? ColorChannel.None,
                OutputMinValue = (float?)outputChannel.MinValue ?? 0f,
                OutputMaxValue = (float?)outputChannel.MaxValue ?? 1f,
                OutputRangeMin = outputChannel.RangeMin ?? 0,
                OutputRangeMax = outputChannel.RangeMax ?? 255,
                OutputShift = outputChannel.Shift ?? 0,
                OutputPower = (float?)outputChannel.Power ?? 1f,
                InvertOutput = outputChannel.Invert ?? false,

                Shift = Material.GetChannelShift(outputChannel.ID),
                Scale = Material.GetChannelScale(outputChannel.ID),
            };

            var isOutputSmooth = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Smooth);
            var isOutputRough = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Rough);

            var inputChannel = InputChannels.FirstOrDefault(i
                => EncodingChannel.Is(i.ID, outputChannel.ID));

            if (Material.TryGetChannelValue(outputChannel.ID, out var value)) {
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

                if (AutoGenerateOcclusion && TextureTags.Is(inputChannel.Texture, TextureTags.Occlusion)) {
                    mapping.SourceTag = TextureTags.OcclusionGenerated;
                    mapping.InputColor = ColorChannel.Red;
                    mapping.InputMinValue = 0f;
                    mapping.InputMaxValue = 1f;
                    mapping.InputRangeMin = 0;
                    mapping.InputRangeMax = 255;
                    mapping.InputShift = 0;
                    mapping.InputPower = 1f;
                    mapping.InvertInput = true;
                    return true;
                }
            }

            // Rough > Smooth
            if (isOutputSmooth && TryGetInputChannel(EncodingChannel.Rough, out var roughChannel)
                               && TryGetSourceFilename(roughChannel.Texture, out mapping.SourceFilename)) {
                mapping.ApplyInputChannel(roughChannel);
                mapping.InvertInput = true;
                return true;
            }

            // Smooth > Rough
            if (isOutputRough && TryGetInputChannel(EncodingChannel.Smooth, out var smoothChannel)) {
                if (TryGetSourceFilename(smoothChannel.Texture, out mapping.SourceFilename)) {
                    mapping.ApplyInputChannel(smoothChannel);
                    mapping.InvertInput = true;
                    return true;
                }
            }

            return CreateEmpty;
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
                    options.Source = Graph.NormalTexture;
                }
                else if (TextureTags.Is(mapping.SourceTag, TextureTags.OcclusionGenerated)) {
                    options.Source = await Graph.GetGeneratedOcclusionAsync(token);
                }

                if (options.Source != null) {
                    if (ImageResult == null) {
                        var size = options.Source.Size();
                        CreateImageResult(in size);
                    }

                    var processor = new OverlayProcessor<Rgb24>(options);
                    ImageResult.Mutate(context => context.ApplyProcessor(processor));
                    return;
                }
            }

            if (!mapping.InputValue.HasValue && mapping.OutputShift == 0 && !mapping.InvertOutput) return;

            var value = (double?)mapping.InputValue ?? 0d;

            if (value < mapping.InputMinValue || value > mapping.InputMaxValue) return;

            // Common Processing
            value = (value + mapping.Shift) * mapping.Scale;

            if (!mapping.OutputPower.Equal(1f))
                value = Math.Pow(value, mapping.OutputPower);

            if (mapping.InvertOutput) MathEx.Invert(ref value, mapping.OutputMinValue, mapping.OutputMaxValue);

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

        private async Task ApplySourceMappingAsync(string sourceFilename, IEnumerable<TextureChannelMapping> mappings, CancellationToken token)
        {
            if (sourceFilename == null) throw new ArgumentNullException(nameof(sourceFilename));

            await using var sourceStream = reader.Open(sourceFilename);
            using var sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);

            var mappingArray = mappings as TextureChannelMapping[] ?? mappings.ToArray();

            var options = new OverlayProcessor<Rgba32>.Options {
                Mappings = mappingArray,
                IsGrayscale = isGrayscale, //mappingArray.All(x => x.OutputColor == ColorChannel.Red),
                Source = sourceImage,
            };

            if (ImageResult == null) {
                var size = options.Source.Size();
                CreateImageResult(in size);
            }

            var processor = new OverlayProcessor<Rgba32>(options);
            ImageResult.Mutate(context => context.ApplyProcessor(processor));
        }

        private void CreateImageResult(in Size size)
        {
            var pixel = new TPixel();
            pixel.FromRgba32(defaultValues);
            ImageResult = new Image<TPixel>(Configuration.Default, size.Width, size.Height, pixel);
        }

        private bool TryGetSourceFilename(string tag, out string filename)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            foreach (var file in reader.EnumerateTextures(Material, tag)) {
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
        public decimal? InputValue;
        public float InputMinValue;
        public float InputMaxValue;
        public byte InputRangeMin;
        public byte InputRangeMax;
        public int InputShift;
        public float InputPower;
        public bool InvertInput;

        public ColorChannel OutputColor;
        public float OutputMinValue;
        public float OutputMaxValue;
        public byte OutputRangeMin;
        public byte OutputRangeMax;
        public int OutputShift;
        public float OutputPower;
        public bool InvertOutput;

        public string SourceTag;
        public string SourceFilename;
        public float Shift;
        public float Scale;


        public void ApplyInputChannel(ResourcePackChannelProperties channel)
        {
            InputColor = channel.Color ?? ColorChannel.None;
            InputMinValue = (float?)channel.MinValue ?? 0f;
            InputMaxValue = (float?)channel.MaxValue ?? 1f;
            InputRangeMin = channel.RangeMin ?? 0;
            InputRangeMax = channel.RangeMax ?? 255;
            InputShift = channel.Shift ?? 0;
            InputPower = (float?)channel.Power ?? 1f;
            InvertInput = channel.Invert ?? false;
        }
    }
}
