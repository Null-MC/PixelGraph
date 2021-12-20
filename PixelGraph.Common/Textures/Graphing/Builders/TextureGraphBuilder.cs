﻿using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageExtensions = PixelGraph.Common.IO.ImageExtensions;

namespace PixelGraph.Common.Textures.Graphing.Builders
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
            var allOutputTextures = Context.OutputEncoding.GetTexturesWithMappings().ToArray();

            try {
                await Graph.PreBuildNormalTextureAsync(token);
            }
            catch (HeightSourceEmptyException) {}

            foreach (var texture in allOutputTextures)
                await Graph.MapAsync(texture, createEmpty, null, null, token);

            Context.MaxFrameCount = Graph.GetMaxFrameCount();
            
            foreach (var texture in allOutputTextures) {
                var channels = texture.Channels.Where(c => c.HasMapping).ToArray();

                if (channels.Any()) {
                    var hasAlpha = channels.Any(c => c.Color == ColorChannel.Alpha);
                    var hasColor = channels.Any(c => c.Color != ColorChannel.Red);

                    if (hasAlpha) {
                        using var image = await Graph.CreateImageAsync<Rgba32>(texture, createEmpty, token);
                        await ProcessTextureAsync(image, texture, ImageChannels.ColorAlpha, token);
                    }
                    else if (hasColor) {
                        using var image = await Graph.CreateImageAsync<Rgb24>(texture, createEmpty, token);
                        await ProcessTextureAsync(image, texture, ImageChannels.Color, token);
                    }
                    else {
                        using var image = await Graph.CreateImageAsync<L8>(texture, createEmpty, token);
                        await ProcessTextureAsync(image, texture, ImageChannels.Gray, token);
                    }
                }

                if (Context.IsAnimated || Context.IsImport)
                    await CopyMetaAsync(texture, token);
            }
        }

        protected virtual async Task ProcessTextureAsync<TPixel>(Image<TPixel> image, string textureTag, ImageChannels type, CancellationToken token = default)
            where TPixel : unmanaged, IPixel<TPixel>
        {
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

            //if (Context.Material.PublishInventory ?? false) {
            //    var partFrame = Regions.GetPublishPartFrame(0, maxFrameCount, 0);
            //    // TODO: create inventory texture?
            //}
        }

        protected abstract Task SaveImagePartAsync<TPixel>(Image<TPixel> image, TexturePublishPart part, ImageChannels type, string destFile, string textureTag, CancellationToken token)
            where TPixel : unmanaged, IPixel<TPixel>;

        protected async Task CopyPropertiesAsync(CancellationToken token)
        {
            var propsFileIn = NamingStructure.GetInputPropertiesName(Context.Material);
            if (!Reader.FileExists(propsFileIn)) return;

            var propsFileOut = NamingStructure.GetOutputPropertiesName(Context.Material, Context.PublishAsGlobal);

            await using var sourceStream = Reader.Open(propsFileIn);
            await Writer.OpenAsync(propsFileOut, async destStream => {
                await sourceStream.CopyToAsync(destStream, token);
            }, token);
        }

        protected async Task ImportMetaAsync(CancellationToken token)
        {
            var path = Context.Material.LocalPath;

            foreach (var file in Reader.EnumerateFiles(path, "*.mcmeta")) {
                var name = Path.GetFileNameWithoutExtension(file);

                var ext = Path.GetExtension(name);
                if (!ImageExtensions.Supports(ext)) continue;

                name = Path.GetFileNameWithoutExtension(name);
                if (!Context.Material.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) continue;

                var metaFileOut = NamingStructure.GetInputMetaName(Context.Material);
                await CopyMetaFileAsync(file, metaFileOut, token);
            }
        }

        protected async Task CopyMetaAsync(string tag, CancellationToken token)
        {
            var metaFileIn = NamingStructure.GetInputMetaName(Context.Material, tag);

            if (!Reader.FileExists(metaFileIn)) {
                metaFileIn = NamingStructure.GetInputMetaName(Context.Material);
                if (!Reader.FileExists(metaFileIn)) return;
            }

            var metaFileOut = NamingStructure.GetOutputMetaName(Context.Profile, Context.Material, tag, Context.PublishAsGlobal);

            await CopyMetaFileAsync(metaFileIn, metaFileOut, token);
        }

        private async Task CopyMetaFileAsync(string metaFileIn, string metaFileOut, CancellationToken token)
        {
            await using var sourceStream = Reader.Open(metaFileIn);
            await Writer.OpenAsync(metaFileOut, async destStream => {
                await sourceStream.CopyToAsync(destStream, token);
            }, token);
        }
    }
}
