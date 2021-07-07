using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.Textures.Graphing
{
    internal abstract class TextureGraphBuilder
    {
        private readonly ILogger logger;

        protected IInputReader Reader {get;}
        protected IOutputWriter Writer {get;}
        protected IImageWriter ImageWriter {get;}
        protected ITextureRegionEnumerator Regions {get;}
        protected ITextureGraphContext Context {get;}
        protected ITextureGraph Graph {get;}


        protected TextureGraphBuilder(
            ITextureGraphContext context,
            ITextureGraph graph,
            IInputReader reader,
            IOutputWriter writer,
            IImageWriter imageWriter,
            ITextureRegionEnumerator regions,
            ILogger logger)
        {
            this.logger = logger;

            Reader = reader;
            Writer = writer;
            ImageWriter = imageWriter;
            Regions = regions;
            Context = context;
            Graph = graph;
        }

        /// <summary>
        /// Publishes all textures with mapped output.
        /// </summary>
        /// <returns>An array of all published texture tags.</returns>
        protected async Task ProcessAllTexturesAsync(bool createEmpty, CancellationToken token)
        {
            var allOutputTags = Context.OutputEncoding
                .Select(e => e.Texture).Distinct().ToArray();

            try {
                await Graph.PreBuildNormalTextureAsync(token);
            }
            catch (HeightSourceEmptyException) {}

            foreach (var tag in allOutputTags)
                await Graph.MapAsync(tag, createEmpty, null, null, token);

            Context.MaxFrameCount = Graph.GetMaxFrameCount();
            
            foreach (var tag in allOutputTags) {
                var tagOutputEncoding = Context.OutputEncoding
                    .Where(e => TextureTags.Is(e.Texture, tag)).ToArray();

                if (tagOutputEncoding.Any()) {
                    var hasAlpha = tagOutputEncoding.Any(c => c.Color == ColorChannel.Alpha);
                    var hasColor = tagOutputEncoding.Any(c => c.Color != ColorChannel.Red);

                    if (hasAlpha) {
                        await ProcessTextureAsync<Rgba32>(tag, ImageChannels.ColorAlpha, createEmpty, token);
                    }
                    else if (hasColor) {
                        await ProcessTextureAsync<Rgb24>(tag, ImageChannels.Color, createEmpty, token);
                    }
                    else {
                        await ProcessTextureAsync<L8>(tag, ImageChannels.Gray, createEmpty, token);
                    }
                }

                if (Context.IsAnimated)
                    await CopyMetaAsync(tag, token);
            }
        }

        protected async Task ProcessTextureAsync<TPixel>(string textureTag, ImageChannels type, bool createEmpty, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var image = await Graph.CreateImageAsync<TPixel>(textureTag, createEmpty, token);

            if (image == null) {
                logger.LogWarning("No texture sources found for item {DisplayName} texture {textureTag}.", Context.Material.DisplayName, textureTag);
                return;
            }

            var sourcePath = Context.Material.LocalPath;
            if (!Context.PublishAsGlobal || (Context.IsMaterialCtm && !Context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, Context.Material.Name);

            var maxFrameCount = Graph.GetMaxFrameCount();
            var usePlaceholder = Context.Material.CTM?.Placeholder ?? false;
            var ext = NamingStructure.GetExtension(Context.Profile);

            foreach (var part in Regions.GetAllPublishRegions(maxFrameCount)) {
                string destFile;
                if (usePlaceholder && part.TileIndex == 0) {
                    var placeholderPath = PathEx.Join("assets", "minecraft", "textures", "block");
                    if (!Context.Mapping.TryMap(placeholderPath, Context.Material.Name, out var destPath, out var destName)) continue;

                    var destTagName = NamingStructure.Get(textureTag, destName, ext, Context.PublishAsGlobal);
                    destFile = PathEx.Join(destPath, destTagName);
                }
                else {
                    if (!Context.Mapping.TryMap(sourcePath, part.Name, out var destPath, out var destName)) continue;

                    var destTagName = NamingStructure.Get(textureTag, destName, ext, Context.PublishAsGlobal);
                    destFile = PathEx.Join(destPath, destTagName);
                }

                await SaveImagePartAsync(image, part, type, destFile, textureTag, token);
            }
        }

        protected abstract Task SaveImagePartAsync<TPixel>(Image<TPixel> image, TexturePublishPart part, ImageChannels type, string destFile, string textureTag, CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>;

        protected async Task CopyPropertiesAsync(CancellationToken token)
        {
            var propsFileIn = NamingStructure.GetInputPropertiesName(Context.Material);
            if (!Reader.FileExists(propsFileIn)) return;

            var propsFileOut = NamingStructure.GetOutputPropertiesName(Context.Material, Context.PublishAsGlobal);

            await using var sourceStream = Reader.Open(propsFileIn);
            await using var destStream = Writer.Open(propsFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }

        protected async Task CopyMetaAsync(string tag, CancellationToken token)
        {
            var metaFileIn = NamingStructure.GetInputMetaName(Context.Material, tag);
            if (!Reader.FileExists(metaFileIn)) {
                metaFileIn = NamingStructure.GetInputMetaName(Context.Material);
                if (!Reader.FileExists(metaFileIn)) return;
            }

            var metaFileOut = NamingStructure.GetOutputMetaName(Context.Profile, Context.Material, tag, Context.PublishAsGlobal);

            await using var sourceStream = Reader.Open(metaFileIn);
            await using var destStream = Writer.Open(metaFileOut);
            await sourceStream.CopyToAsync(destStream, token);
        }
    }
}
