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
        bool CreateEmpty {get; set;}

        Task BuildAsync(CancellationToken token = default);
    }

    internal class TextureBuilder<TPixel> : ITextureBuilder<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly IInputReader reader;
        private Rgba32 defaultValues;

        public ITextureGraph Graph {get; set;}
        public MaterialProperties Material {get; set;}
        public ResourcePackChannelProperties[] InputChannels {get; set;}
        public ResourcePackChannelProperties[] OutputChannels {get; set;}
        public Size DefaultSize {get; set;}
        public Image<TPixel> ImageResult {get; private set;}
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

            foreach (var channel in OutputChannels) {
                if (TryBuildMapping(channel, out var mapping))
                    mappings.Add(mapping);
            }

            if (!CreateEmpty && mappings.Count == 0) return;

            foreach (var mappingGroup in mappings.GroupBy(m => m.SourceFilename)) {
                if (mappingGroup.Key == null) continue;
                
                await ApplySourceMappingAsync(mappingGroup.Key, mappingGroup, token);
            }

            foreach (var mapping in mappings.Where(m => m.SourceFilename == null))
                await ApplyMappingAsync(mapping, token);

            if (ImageResult == null) {
                //var size = Graph.GetSourceSize();
                CreateImageResult(DefaultSize);
            }
        }

        public void Dispose()
        {
            ImageResult?.Dispose();
        }

        private bool TryBuildMapping(ResourcePackChannelProperties outputChannel, out TextureChannelMapping mapping)
        {
            mapping = new TextureChannelMapping {
                OutputColor = outputChannel.Color ?? ColorChannel.None,
                OutputMin = outputChannel.MinValue ?? 0,
                OutputMax = outputChannel.MaxValue ?? 255,
                OutputShift = outputChannel.Shift ?? 0,
                OutputPower = (float?)outputChannel.Power ?? 1f,
                InvertOutput = outputChannel.Invert ?? false,

                Shift = Material.GetChannelShift(outputChannel.ID),
                Scale = Material.GetChannelScale(outputChannel.ID),
            };

            var isOutputSmooth = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Smooth);
            var isOutputRough = EncodingChannel.Is(outputChannel.ID, EncodingChannel.Rough);

            if (Material.TryGetChannelValue(outputChannel.ID, out var value)) {
                mapping.InputValue = value;
                return true;
            }

            var inputChannel = InputChannels.FirstOrDefault(i
                => EncodingChannel.Is(i.ID, outputChannel.ID));

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

                var autoGenOcclusion = Graph.Context.Profile?.AutoGenerateOcclusion
                    ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;

                if (autoGenOcclusion && TextureTags.Is(inputChannel.Texture, TextureTags.Occlusion)) {
                    mapping.SourceTag = TextureTags.OcclusionGenerated;
                    mapping.InputColor = ColorChannel.Red;
                    mapping.InputMin = 0;
                    mapping.InputMax = 255;
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

            var value = (mapping.InputValue ?? 0) / 255f;

            value += mapping.Shift;
            value *= mapping.Scale;

            if (MathF.Abs(mapping.OutputPower - 1f) > float.Epsilon)
                value = MathF.Pow(value, mapping.OutputPower);

            if (mapping.InvertOutput) MathEx.Invert(ref value);

            MathEx.Saturate(value, out var finalValue);
            MathEx.Cycle(ref finalValue, in mapping.OutputShift);

            if (mapping.OutputMin != 0 || mapping.OutputMax != 255) {
                var f = finalValue / 255f;
                var range = mapping.OutputMax - mapping.OutputMin;
                finalValue = MathEx.Clamp(mapping.OutputMin + (int) (f * range), mapping.OutputMin, mapping.OutputMax);
            }

            if (ImageResult != null) {
                var options = new OverwriteProcessor.Options {
                    Color = mapping.OutputColor,
                    Value = finalValue,
                    Min = mapping.OutputMin,
                    Max = mapping.OutputMax,
                };

                var processor = new OverwriteProcessor(options);
                ImageResult.Mutate(context => context.ApplyProcessor(processor));
            }
            else {
                defaultValues.SetChannelValue(in mapping.OutputColor, in finalValue);
            }
        }

        private async Task ApplySourceMappingAsync(string sourceFilename, IEnumerable<TextureChannelMapping> mappings, CancellationToken token)
        {
            if (sourceFilename == null) throw new ArgumentNullException(nameof(sourceFilename));

            await using var sourceStream = reader.Open(sourceFilename);
            using var sourceImage = await Image.LoadAsync<Rgba32>(Configuration.Default, sourceStream, token);

            var options = new OverlayProcessor<Rgba32>.Options {
                Mappings = mappings as TextureChannelMapping[] ?? mappings.ToArray(),
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
        public byte? InputValue;
        public byte InputMin;
        public byte InputMax;
        public int InputShift;
        public float InputPower;
        public bool InvertInput;

        public ColorChannel OutputColor;
        public byte OutputMin;
        public byte OutputMax;
        public int OutputShift;
        public float OutputPower;
        public bool InvertOutput;

        public string SourceTag;
        public string SourceFilename;
        public int Shift;
        public float Scale;


        public void ApplyInputChannel(ResourcePackChannelProperties channel)
        {
            InputColor = channel.Color ?? ColorChannel.None;
            InputMin = channel.MinValue ?? 0;
            InputMax = channel.MaxValue ?? 255;
            InputShift = channel.Shift ?? 0;
            InputPower = (float?)channel.Power ?? 1f;
            InvertInput = channel.Invert ?? false;
        }
    }
}
