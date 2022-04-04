using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Effects;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Preview.Textures
{
    public interface ITexturePreviewBuilder : IDisposable
    {
        ResourcePackInputProperties Input {get; set;}
        ResourcePackProfileProperties Profile {get; set;}
        MaterialProperties Material {get; set;}
        CancellationToken Token {get;}

        Task<Image<TPixel>> BuildAsync<TPixel>(string tag, int? targetFrame = null, int? targetPart = null)
            where TPixel : unmanaged, IPixel<TPixel>;

        void Cancel();
    }

    internal abstract class TexturePreviewBuilderBase : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;
        private readonly IProjectContext projectContext;
        private readonly CancellationTokenSource tokenSource;

        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}
        public MaterialProperties Material {get; set;}

        protected IDictionary<string, Func<ResourcePackProfileProperties, MaterialProperties, ResourcePackChannelProperties[]>> TagMap {get; set;}
        public CancellationToken Token => tokenSource.Token;


        protected TexturePreviewBuilderBase(IServiceProvider provider)
        {
            this.provider = provider;

            projectContext = provider.GetRequiredService<IProjectContext>();
            tokenSource = new CancellationTokenSource();
        }

        public async Task<Image<TPixel>> BuildAsync<TPixel>(string tag, int? targetFrame = 0, int? targetPart = null)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();
            
            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

            await using var scope = serviceBuilder.Build();

            var context = scope.GetRequiredService<ITextureGraphContext>();
            var graph = scope.GetRequiredService<ITextureGraph>();
            var reader = scope.GetRequiredService<IInputReader>();

            context.Input = Input;
            context.Profile = Profile;
            context.Material = Material;

            var matMetaFileIn = NamingStructure.GetInputMetaName(Material);
            context.IsAnimated = reader.FileExists(matMetaFileIn);

            var inputEncoding = GetEncoding(Input?.Format);
            inputEncoding.Merge(Input);
            inputEncoding.Merge(Material);
            context.InputEncoding = inputEncoding.GetMapped().ToList();

            if (TryGetChannels(tag, out var channels))
                context.OutputEncoding.AddRange(channels);

            if (TextureTags.Is(tag, TextureTags.Normal))
                await graph.PreBuildNormalTextureAsync(tokenSource.Token);

            await graph.MapAsync(tag, true, targetFrame, targetPart, Token);
            context.MaxFrameCount = graph.GetMaxFrameCount();

            var regions = scope.GetRequiredService<TextureRegionEnumerator>();
            regions.SourceFrameCount = context.MaxFrameCount;
            regions.DestFrameCount = context.MaxFrameCount;
            regions.TargetFrame = targetFrame;
            regions.TargetPart = targetPart;

            var image = await graph.CreateImageAsync<TPixel>(tag, true, tokenSource.Token);
            if (image == null) return null;

            if (image.Width > 1 || image.Height > 1) {
                var edgeFadeEffect = scope.GetRequiredService<IEdgeFadeImageEffect>();
                var quantizer = new Lazy<IQuantizer>(() => new WuQuantizer(new QuantizerOptions {
                    MaxColors = context.PaletteColors,
                }));

                try {
                    foreach (var part in regions.GetAllPublishRegions()) {
                        foreach (var frame in part.Frames) {
                            var outBounds = targetPart.HasValue
                                ? new Rectangle(0, 0, image.Width, image.Height)
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

        private bool TryGetChannels(string textureTag, out ResourcePackChannelProperties[] channels)
        {
            if (TagMap.TryGetValue(textureTag, out var channelFunc)) {
                channels = channelFunc(Profile, Material);
                return true;
            }

            channels = null;
            return false;
        }

        private static ResourcePackEncoding GetEncoding(string format)
        {
            ResourcePackEncoding encoding = null;

            if (format != null) {
                var factory = TextureFormat.GetFactory(format);
                if (factory != null) encoding = factory.Create();
            }

            return encoding ?? new ResourcePackEncoding();
        }
    }
}
