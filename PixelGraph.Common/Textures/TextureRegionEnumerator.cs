﻿using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Textures.Graphing;

namespace PixelGraph.Common.Textures;

public class TextureRegionEnumerator
    (ITextureGraphContext context)
{
    public int SourceFrameCount {get; set;} = 1;
    public int DestFrameCount {get; set;} = 1;
    public int? TargetFrame {get; set;}
    public int? TargetPart {get; set;}

    private int ActualDestFrameCount => TargetFrame.HasValue ? 1 : DestFrameCount;


    public IEnumerable<TextureRenderFrame> GetAllRenderRegions()
    {
        if (TargetFrame.HasValue || TargetPart.HasValue) {
            yield return GetRenderRegion(TargetFrame ?? 0);
            yield break;
        }

        for (var frame = 0; frame < DestFrameCount; frame++)
            yield return GetRenderRegion(frame);
    }

    private TextureRenderFrame GetRenderRegion(int frameIndex)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        var renderFrame = new TextureRenderFrame();

        if (context.IsMaterialCtm) {
            var tileRange = TargetPart.HasValue
                ? Enumerable.Repeat(TargetPart.Value, 1)
                : Enumerable.Range(0, GetPublishTileCount());

            if (CtmTypes.IsRepeatType(context.Material.CTM?.Method)) {
                var tile = new TextureRenderTile();
                renderFrame.Tiles = [tile];

                if (TargetPart.HasValue)
                    GetFrameTileBounds(frameIndex, SourceFrameCount, TargetPart.Value, out tile.SourceBounds);
                else
                    GetFrameBounds(frameIndex, SourceFrameCount, out tile.SourceBounds);

                GetFrameBounds(in frameIndex, ActualDestFrameCount, out tile.DestBounds);
            }
            else {
                renderFrame.Tiles = tileRange
                    .Select(z => {
                        var tile = new TextureRenderTile();

                        GetFrameTileBounds(frameIndex, SourceFrameCount, z, out tile.SourceBounds);

                        if (TargetPart.HasValue)
                            GetFrameBounds(frameIndex, ActualDestFrameCount, out tile.DestBounds);
                        else
                            GetFrameTileBounds(frameIndex, ActualDestFrameCount, z, out tile.DestBounds);

                        return tile;
                    }).ToArray();
            }
        }
        else {
            var tile = new TextureRenderTile();


            if (TargetPart.HasValue)
                GetFrameTileBounds(frameIndex, SourceFrameCount, TargetPart.Value, out tile.SourceBounds);
            else
                GetFrameBounds(frameIndex, SourceFrameCount, out tile.SourceBounds);

            GetFrameBounds(frameIndex, ActualDestFrameCount, out tile.DestBounds);

            renderFrame.Tiles = [tile];
        }

        return renderFrame;
    }

    public IEnumerable<TexturePublishPart> GetAllPublishRegions()
    {
        IEnumerable<TexturePublishFrame> GetFrames(int partIndex) {
            if (TargetFrame.HasValue) return [GetPublishPartFrame(TargetFrame.Value, partIndex)];

            return Enumerable.Range(0, DestFrameCount)
                .Select(i => GetPublishPartFrame(i, partIndex));
        }

        if (TargetPart.HasValue) {
            var p = TargetPart.Value;

            return [
                new TexturePublishPart {
                    Name = GetTileName(p),
                    TileIndex = p,
                    Frames = GetFrames(p).ToArray(),
                }
            ];
        }

        var tileCount = GetPublishTileCount();

        return Enumerable.Range(0, tileCount)
            .Select(t => new TexturePublishPart {
                Name = GetTileName(t),
                TileIndex = t,
                Frames = GetFrames(t).ToArray(),
            });
    }

    private string? GetTileName(int index)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        if (context.IsMaterialMultiPart)
            return context.Material.Parts?[index].Name;

        if (context.IsMaterialCtm) {
            var start = context.Material?.CTM?.TileStartIndex ??
                        context.Profile?.TileStartIndex ?? 1;

            return (start + index).ToString();
        }

        return context.Material.Name;
    }

    public TexturePublishFrame GetPublishPartFrame(int frameIndex, int tileIndex)
    {
        var frameHeight = 1d;
        if (!TargetFrame.HasValue && ActualDestFrameCount > 1)
            frameHeight = 1d / ActualDestFrameCount;

        var frame = new TexturePublishFrame {
            //SourceBounds = GetFrameTileBounds(frameIndex, SourceFrameCount, tileIndex),
            DestBounds = new UVRegion {
                Left = 0d,
                Top = frameIndex * frameHeight,
                Right = 1d,
                Bottom = (frameIndex + 1) * frameHeight,
            },
        };

        GetFrameTileBounds(frameIndex, SourceFrameCount, tileIndex, out frame.SourceBounds);
        return frame;
    }

    private int GetPublishTileCount()
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        if (context.IsMaterialMultiPart)
            return context.Material.Parts?.Count ?? 0;

        if (context.IsMaterialCtm && context.Material?.CTM != null)
            return CtmTypes.GetBounds(context.Material.CTM)?.Total ?? 1;

        return 1;
    }

    private static void GetFrameBounds(in int frame, in int frameCount, out UVRegion region)
    {
        var frameHeight = 1d;
        if (frameCount > 1) frameHeight /= frameCount;
        var wrappedIndex = frame % frameCount;

        region.Left = 0d;
        region.Top = wrappedIndex * frameHeight;
        region.Right = 1d;
        region.Bottom = (wrappedIndex + 1) * frameHeight;
    }

    public void GetFrameTileBounds(in int frameIndex, in int frameCount, in int tileIndex, out UVRegion region)
    {
        ArgumentNullException.ThrowIfNull(context.Material);

        var frameHeight = 1d;
        if (frameCount > 1) frameHeight /= frameCount;
        var wrappedIndex = frameIndex % frameCount;

        if (context.IsMaterialMultiPart && context.Material.Parts != null) {
            var part = context.Material.Parts[tileIndex];
            var (maxWidth, maxHeight) = context.Material.GetMultiPartBounds();

            if (maxWidth == 0 || maxHeight == 0) {
                region = UVRegion.Empty;
                return;
            }

            region.Left = (part.Left ?? 0) / (double)maxWidth;
            region.Top = (part.Top ?? 0) / (double)maxHeight * frameHeight + wrappedIndex * frameHeight;
            region.Right = ((part.Left ?? 0) + (part.Width ?? 1)) / (double)maxWidth;
            region.Bottom = ((part.Top ?? 0) + (part.Height ?? 1)) / (double)maxHeight * frameHeight;
            return;
        }

        if (context.IsMaterialCtm && context.Material.CTM != null) {
            var bounds = CtmTypes.GetBounds(context.Material.CTM);
            var tileCountX = bounds?.Width ?? 1;
            var tileCountY = bounds?.Height ?? 1;

            var tileWidth = 1d / tileCountX;
            var tileHeight = frameHeight / tileCountY;

            var col = tileIndex % tileCountX;
            var row = (int)Math.Floor(tileIndex / (double)tileCountX + double.Epsilon);

            region.Left = col * tileWidth;
            region.Top = wrappedIndex * frameHeight + row * tileHeight;
            region.Right = (col + 1) * tileWidth;
            region.Bottom = wrappedIndex * frameHeight + (row + 1) * tileHeight;
            return;
        }

        GetFrameBounds(frameIndex, frameCount, out region);
    }
}

public class TextureRenderFrame
{
    public TextureRenderTile[]? Tiles {get; set;}
}

public class TextureRenderTile
{
    public UVRegion SourceBounds;
    public UVRegion DestBounds;
}

public class TexturePublishPart
{
    public string? Name {get; init;}
    public int TileIndex {get; init;}
    public TexturePublishFrame[] Frames {get; init;} = [];
}

public class TexturePublishFrame
{
    public UVRegion SourceBounds;
    public UVRegion DestBounds;
}