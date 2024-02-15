using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageExtensions = PixelGraph.Common.IO.ImageExtensions;

namespace PixelGraph.Common.Textures.Graphing.Builders;

internal abstract class TextureGraphBuilder
{
    private readonly ILogger logger;

    protected IServiceProvider Provider {get;}
    protected IInputReader Reader {get;}
    protected ITextureReader TexReader {get;}
    protected ITextureWriter TexWriter {get;}
    protected IOutputWriter Writer {get;}
    protected IImageWriter ImageWriter {get;}
    protected ITextureGraphContext Context {get;}
    protected ITextureGraph Graph {get;}
    protected IPublishSummary Summary {get;}


    protected TextureGraphBuilder(IServiceProvider provider, ILogger logger)
    {
        Provider = provider;
        this.logger = logger;

        Reader = provider.GetRequiredService<IInputReader>();
        TexReader = provider.GetRequiredService<ITextureReader>();
        TexWriter = provider.GetRequiredService<ITextureWriter>();
        Writer = provider.GetRequiredService<IOutputWriter>();
        ImageWriter = provider.GetRequiredService<IImageWriter>();
        Context = provider.GetRequiredService<ITextureGraphContext>();
        Graph = provider.GetRequiredService<ITextureGraph>();
        Summary = provider.GetRequiredService<IPublishSummary>();
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
                    using var image = await Graph.CreateImageAsync<Rgba32>(tag, createEmpty, token);
                    await ProcessTextureAsync(image, tag, ImageChannels.ColorAlpha, token);
                }
                else if (hasColor) {
                    using var image = await Graph.CreateImageAsync<Rgb24>(tag, createEmpty, token);
                    await ProcessTextureAsync(image, tag, ImageChannels.Color, token);
                }
                else {
                    using var image = await Graph.CreateImageAsync<L8>(tag, createEmpty, token);
                    await ProcessTextureAsync(image, tag, ImageChannels.Gray, token);
                }

                Summary.IncrementTextureCount();
            }

            if (Context.IsAnimated || Context.IsImport)
                await CopyMetaAsync(tag, token);
        }
    }

    protected virtual async Task ProcessTextureAsync<TPixel>(Image<TPixel>? image, string textureTag, ImageChannels type, CancellationToken token = default)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        ArgumentNullException.ThrowIfNull(Context.Material);
        ArgumentNullException.ThrowIfNull(Context.Profile);

        if (image == null) {
            logger.LogWarning("No texture sources found for item {DisplayName} texture {textureTag}.", Context.Material.DisplayName, textureTag);
            return;
        }

        var sourcePath = Context.Material.LocalPath;

        if (!Context.IsImport) {
            if (!Context.PublishAsGlobal || (Context.IsMaterialCtm && !Context.Material.UseGlobalMatching))
                sourcePath = PathEx.Join(sourcePath, Context.Material.Name);
        }

        var maxFrameCount = Graph.GetMaxFrameCount();
        var usePlaceholder = Context.Material.CTM?.Placeholder ?? false;
        var ext = NamingStructure.GetExtension(Context.Profile);

        var regions = Provider.GetRequiredService<TextureRegionEnumerator>();
        regions.SourceFrameCount = maxFrameCount;
        regions.DestFrameCount = maxFrameCount;

        foreach (var part in regions.GetAllPublishRegions()) {
            string destFile;
            if (usePlaceholder && part.TileIndex == 0) {
                var placeholderPath = PathEx.Join("assets", "minecraft", "textures", "block");
                if (!Context.Mapping.TryMap(placeholderPath, Context.Material.Name, out var destPath, out var destName)) continue;

                var destTagName = TexWriter.TryGet(textureTag, destName, ext, Context.PublishAsGlobal);
                if (destTagName == null) {
                    // WARN: WHAT DO WE DO?!
                    throw new NotImplementedException();
                }

                destFile = PathEx.Join(destPath, destTagName);
            }
            else {
                if (!Context.Mapping.TryMap(sourcePath, part.Name, out var destPath, out var destName)) continue;

                var destTagName = TexWriter.TryGet(textureTag, destName, ext, Context.PublishAsGlobal);
                if (destTagName == null) {
                    // WARN: WHAT DO WE DO?!
                    throw new NotImplementedException();
                }

                if (!Context.PublishAsGlobal) destPath = PathEx.Join(destPath, destName);
                destFile = PathEx.Join(destPath, destTagName);
            }

            var diskSize = await SaveImagePartAsync(image, part, type, destFile, textureTag, token);
            Summary.AddRawBytes(image.Width, image.Height);
            Summary.AddDiskBytes(diskSize);
        }
    }

    protected abstract Task<long> SaveImagePartAsync<TPixel>(Image<TPixel> image, TexturePublishPart part, ImageChannels type, string destFile, string textureTag, CancellationToken token)
        where TPixel : unmanaged, IPixel<TPixel>;

    protected async Task CopyPropertiesAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(Context.Material);

        var propsFileIn = NamingStructure.GetInputPropertiesName(Context.Material);
        if (!Reader.FileExists(propsFileIn)) return;

        var propsFileOut = NamingStructure.GetOutputPropertiesName(Context.Material, Context.PublishAsGlobal);

        await using var sourceStream = Reader.Open(propsFileIn)
            ?? throw new ApplicationException("Failed to open properties file stream!");

        await Writer.OpenWriteAsync(propsFileOut, async destStream => {
            await sourceStream.CopyToAsync(destStream, token);
        }, token);
    }

    protected async Task ImportMetaAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(Context.Material);

        var path = Context.Material.LocalPath;

        foreach (var file in Reader.EnumerateFiles(path, "*.mcmeta")) {
            var name = Path.GetFileNameWithoutExtension(file);

            var ext = Path.GetExtension(name);
            if (!ImageExtensions.Supports(ext)) continue;

            name = Path.GetFileNameWithoutExtension(name);
            if (!string.Equals(name, Context.Material.Name, StringComparison.InvariantCultureIgnoreCase)) continue;

            var metaFileOut = NamingStructure.GetInputMetaName(Context.Material);
            await CopyMetaFileAsync(file, metaFileOut, token);
        }
    }

    protected async Task CopyMetaAsync(string tag, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(Context.Material);

        var metaFileIn = TexWriter.GetInputMetaName(Context.Material, tag);

        if (!Reader.FileExists(metaFileIn)) {
            metaFileIn = NamingStructure.GetInputMetaName(Context.Material);
            if (!Reader.FileExists(metaFileIn)) return;
        }

        var partNames = Context.IsMaterialMultiPart
            ? Context.Material.Parts.Select(p => p.Name)
            : Enumerable.Repeat(Context.Material.Name, 1);

        foreach (var partName in partNames) {
            if (!Context.Mapping.TryMap(Context.Material.LocalPath, partName, out var destPath, out var destName)) continue;

            var metaFileOut = TexWriter.GetOutputMetaName(Context.Profile, destPath, destName, tag, Context.PublishAsGlobal);
            await CopyMetaFileAsync(metaFileIn, metaFileOut, token);
        }
    }

    private async Task CopyMetaFileAsync(string metaFileIn, string metaFileOut, CancellationToken token)
    {
        await using var sourceStream = Reader.Open(metaFileIn);
        if (sourceStream == null) throw new ApplicationException("Failed to open meta file stream!");

        await Writer.OpenWriteAsync(metaFileOut, async destStream => {
            await sourceStream.CopyToAsync(destStream, token);
        }, token);
    }
}