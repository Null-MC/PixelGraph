﻿using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace PixelGraph.Common.Textures;

public interface IItemTextureGenerator
{
    Task<Image<Rgba32>?> CreateAsync(ITextureGraph graph, CancellationToken token = default);
}

internal class ItemTextureGenerator(
    IServiceProvider provider,
    ITextureGraphContext context,
    ITextureSourceGraph sourceGraph,
    ITextureNormalGraph normalGraph,
    ITextureOcclusionGraph occlusionGraph,
    ITextureReader texReader,
    IInputReader reader)
    : IItemTextureGenerator
{
    private const int TargetFrame = 0;

    private Image<Rgba32>? emissiveImage;


    public async Task<Image<Rgba32>?> CreateAsync(ITextureGraph graph, CancellationToken token = default)
    {
        Image<Rgba32>? image = null;

        emissiveImage = null;
        try {
            image = await BuildAlbedoBufferAsync(token);
            if (image == null) return null;

            try {
                await graph.PreBuildNormalTextureAsync(token);
            }
            catch (HeightSourceEmptyException) {}

            var emissiveChannel = context.InputEncoding.GetChannel(TextureTags.Emissive);
            var emissiveInfo = await GetEmissiveInfoAsync(token);

            var inventoryOptions = new ItemProcessor<L8, Rgba32>.Options {
                NormalSampler = normalGraph.GetNormalSampler(),
                OcclusionInputColor = ColorChannel.Red,
                OcclusionSampler = await occlusionGraph.GetSamplerAsync(token),
                LightDirection = new Vector3(-1f, -3f, 5f),
            };

            MathEx.Normalize(ref inventoryOptions.LightDirection);

            if (occlusionGraph.HasTexture) {
                var occlusionMapping = new TextureChannelMapping();
                occlusionMapping.ApplyInputChannel(occlusionGraph.Channel);

                inventoryOptions.OcclusionMapping = new PixelMapping(occlusionMapping);
            }

            if (emissiveInfo != null) {
                inventoryOptions.EmissiveColor = emissiveChannel?.Color ?? ColorChannel.Red;
                inventoryOptions.EmissiveSampler = await GetEmissiveSamplerAsync(emissiveInfo.LocalFile, token);
            }

            if (inventoryOptions.NormalSampler != null || inventoryOptions.OcclusionSampler != null) {
                var processor = new ItemProcessor<L8, Rgba32>(inventoryOptions);
                var regions = provider.GetRequiredService<TextureRegionEnumerator>();
                regions.SourceFrameCount = 1; //FrameCount;
                regions.DestFrameCount = 1;
                regions.TargetFrame = 0;

                if (context.IsMaterialCtm)
                    regions.TargetPart = 0;

                //regions.TargetPart = TargetPart;

                foreach (var part in regions.GetAllPublishRegions()) {
                    var frame = part.Frames.FirstOrDefault();
                    if (frame == null) continue;

                    if (inventoryOptions.NormalSampler != null) {
                        regions.GetFrameTileBounds(TargetFrame, normalGraph.NormalFrameCount, part.TileIndex, out var region);
                        inventoryOptions.NormalSampler.SetBounds(in region);
                    }

                    if (inventoryOptions.OcclusionSampler != null) {
                        regions.GetFrameTileBounds(TargetFrame, occlusionGraph.FrameCount, part.TileIndex, out var region);
                        inventoryOptions.OcclusionSampler.SetBounds(in region);
                    }

                    if (emissiveChannel != null && emissiveInfo != null && inventoryOptions.EmissiveSampler != null) {
                        regions.GetFrameTileBounds(TargetFrame, emissiveInfo.FrameCount, part.TileIndex, out var region);
                        inventoryOptions.EmissiveSampler.SetBounds(in region);
                    }

                    var outBounds = frame.DestBounds.ScaleTo(image.Width, image.Height);
                    image.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }

            // Make image square
            if (image.Width != image.Height) {
                var targetSize = Math.Max(image.Width, image.Height);
                var temp = SquareImage(image, targetSize);

                try {
                    image.Dispose();
                    image = temp;
                }
                catch {
                    temp.Dispose();
                    throw;
                }
            }

            return image;
        }
        catch {
            image?.Dispose();
            throw;
        }
        finally {
            emissiveImage?.Dispose();
        }
    }

    private async Task<Image<Rgba32>?> BuildAlbedoBufferAsync(CancellationToken token)
    {
        using var scope = provider.CreateScope();
        var subContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
        var builder = scope.ServiceProvider.GetRequiredService<ITextureBuilder>();

        subContext.Project = context.Project;
        subContext.Profile = context.Profile;
        subContext.Material = context.Material;
        subContext.IsAnimated = context.IsAnimated;
        //subContext.PackWriteTime = ;

        if (context.InputEncoding.TryGetChannel(EncodingChannel.ColorRed, out var redInputChannel) && redInputChannel != null)
            subContext.InputEncoding.Add(redInputChannel);

        if (context.InputEncoding.TryGetChannel(EncodingChannel.ColorGreen, out var greenInputChannel) && greenInputChannel != null)
            subContext.InputEncoding.Add(greenInputChannel);

        if (context.InputEncoding.TryGetChannel(EncodingChannel.ColorBlue, out var blueInputChannel) && blueInputChannel != null)
            subContext.InputEncoding.Add(blueInputChannel);

        if (context.InputEncoding.TryGetChannel(EncodingChannel.Opacity, out var opacityInputChannel) && opacityInputChannel != null)
            subContext.InputEncoding.Add(opacityInputChannel);

        builder.InputChannels = subContext.InputEncoding.ToArray();
        builder.OutputChannels = [
            new ResourcePackColorRedChannelProperties(TextureTags.Color, ColorChannel.Red),
            new ResourcePackColorGreenChannelProperties(TextureTags.Color, ColorChannel.Green),
            new ResourcePackColorBlueChannelProperties(TextureTags.Color, ColorChannel.Blue),
            new ResourcePackOpacityChannelProperties(TextureTags.Color, ColorChannel.Alpha) {DefaultValue = 1m}
        ];

        if (context.IsMaterialCtm)
            builder.TargetPart = 0;

        builder.TargetFrame = TargetFrame;
        await builder.MapAsync(false, token);
        //if (!builder.HasMappedSources) return null;

        float? aspect = null;
        if (context.IsMaterialMultiPart) {
            var (width, height) = context.Material.GetMultiPartBounds();
            aspect = (float)height / width;
        }

        var targetSize = context.GetMaterialSize();

        if (!targetSize.HasValue) {
            if (TextureSizeUtility.TryGetItemSize(context.Profile, out var itemSize, aspect))
                targetSize = itemSize;
            else if (TextureSizeUtility.TryGetBlockSize(context.Profile, out var blockSize, aspect))
                targetSize = blockSize;
            else if (TextureSizeUtility.TryGetTextureSize(context.Profile, out var texSize, aspect))
                targetSize = texSize;
        }

        Image<Rgba32>? image = null;
        try {
            image = await builder.BuildAsync<Rgba32>(false, targetSize, token);
            return image;
        }
        catch {
            image?.Dispose();
            throw;
        }
    }

    private Task<TextureSource?> GetEmissiveInfoAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        var emissiveFile = texReader.EnumerateInputTextures(context.Material, TextureTags.Emissive).FirstOrDefault();
        if (emissiveFile == null) return Task.FromResult<TextureSource?>(null);

        return sourceGraph.GetOrCreateAsync(emissiveFile, token);
    }

    private async Task<ISampler<Rgba32>> GetEmissiveSamplerAsync(string emissiveFile, CancellationToken token)
    {
        await using var stream = reader.Open(emissiveFile)
            ?? throw new ApplicationException("Failed to open file stream!");

        emissiveImage = await Image.LoadAsync<Rgba32>(stream, token);

        var samplerName = context.GetSamplerName(EncodingChannel.Emissive);// context.Profile?.Encoding?.Emissive.Sampler ?? context.DefaultSampler;

        var sampler = context.CreateSampler(emissiveImage, samplerName);

        // TODO: SET THESE PROPERLY!
        sampler.RangeX = 1f;
        sampler.RangeY = 1f;

        return sampler;
    }

    private static Image<TPixel> SquareImage<TPixel>(Image<TPixel> image, int targetSize)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var temp = new Image<TPixel>(targetSize, targetSize);

        try {
            var outBounds = new Rectangle(
                (targetSize - image.Width) / 2,
                (targetSize - image.Height) / 2,
                image.Width, image.Height);

            ImageProcessors.ImageProcessors.CopyRegion(image, 0, 0, temp, outBounds);

            return temp;
        }
        catch {
            temp.Dispose();
            throw;
        }
    }
}