using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Effects;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface ITexturePreviewBuilder : IDisposable
    {
        ResourcePackInputProperties Input {get; set;}
        MaterialProperties Material {get; set;}
        ResourcePackProfileProperties Profile {get; set;}
        CancellationToken Token {get;}

        Task<Image<TPixel>> BuildAsync<TPixel>(string tag, int? targetFrame = null, int? targetPart = null)
            where TPixel : unmanaged, IPixel<TPixel>;
        void Cancel();
    }

    internal abstract class TexturePreviewBuilderBase : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;
        private readonly CancellationTokenSource tokenSource;

        public ResourcePackInputProperties Input {get; set;}
        public MaterialProperties Material {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}

        protected IDictionary<string, Func<ResourcePackProfileProperties, MaterialProperties, TextureMapping[]>> TagMap {get; set;}
        public CancellationToken Token => tokenSource.Token;


        protected TexturePreviewBuilderBase(IServiceProvider provider)
        {
            this.provider = provider;
            tokenSource = new CancellationTokenSource();
        }

        public async Task<Image<TPixel>> BuildAsync<TPixel>(string tag, int? targetFrame = 0, int? targetPart = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graph = scope.ServiceProvider.GetRequiredService<ITextureGraph>();
            var regions = scope.ServiceProvider.GetRequiredService<ITextureRegionEnumerator>();
            var reader = scope.ServiceProvider.GetRequiredService<IInputReader>();

            context.Input = Input;
            context.Profile = Profile;
            context.Material = Material;

            var matMetaFileIn = NamingStructure.GetInputMetaName(Material);
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            context.InputEncoding = GetEncoding(Input?.Format);
            context.InputEncoding.Merge(Input);
            context.InputEncoding.Merge(Material);
            //= inputEncoding.GetTexturesWithMappings();

            context.OutputEncoding = new TextureMappingCollection();
            if (TryGetChannels(tag, out var channels))
                context.OutputEncoding.AddRange(channels);

            if (TextureTags.Is(tag, TextureTags.Normal))
                await graph.PreBuildNormalTextureAsync(tokenSource.Token);

            await graph.MapAsync(tag, true, targetFrame, targetPart, Token);

            var image = await graph.CreateImageAsync<TPixel>(tag, true, tokenSource.Token);
            if (image == null) return null;

            if (image.Width > 1 || image.Height > 1) {
                var edgeFadeEffect = scope.ServiceProvider.GetRequiredService<IEdgeFadeImageEffect>();
                var quantizer = new Lazy<IQuantizer>(() => new WuQuantizer(new QuantizerOptions {
                    MaxColors = context.PaletteColors,
                }));

                try {
                    foreach (var part in regions.GetAllPublishRegions(context.MaxFrameCount, targetFrame, targetPart)) {
                        foreach (var frame in part.Frames) {
                            var outBounds = targetPart.HasValue ? new Rectangle(0, 0, image.Width, image.Height)
                                : frame.SourceBounds.ScaleTo(image.Width, image.Height);

                            edgeFadeEffect.Apply(image, tag, outBounds);

                            if (context.EnablePalette) {
                                image.Mutate(imgContext => imgContext.Quantize(quantizer.Value));
                            }
                        }
                    }
                }
                catch {
                    image.Dispose();
                    throw;
                }
            }

            return image;
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        private bool TryGetChannels(string textureTag, out TextureMapping[] textureMaps)
        {
            if (TagMap.TryGetValue(textureTag, out var channelFunc)) {
                textureMaps = channelFunc(Profile, Material);
                return true;
            }

            textureMaps = null;
            return false;
        }

        private static TextureMappingCollection GetEncoding(string format)
        {
            TextureMappingCollection encoding = null;

            if (format != null) {
                var factory = TextureFormat.GetFactory(format);
                if (factory != null) encoding = factory.Create();
            }

            return encoding ?? new TextureMappingCollection();
        }
    }
}
