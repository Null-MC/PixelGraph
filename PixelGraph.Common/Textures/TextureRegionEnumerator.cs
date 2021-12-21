using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.Textures
{
    public interface ITextureRegionEnumerator
    {
        int SourceFrameCount {get; set;}
        int DestFrameCount {get; set;}
        int? TargetFrame {get; set;}
        int? TargetPart {get; set;}

        IEnumerable<TextureRenderFrame> GetAllRenderRegions();
        IEnumerable<TexturePublishPart> GetAllPublishRegions();
        TexturePublishFrame GetPublishPartFrame(int frameIndex, int tileIndex);
        RectangleF GetFrameTileBounds(int frameIndex, int frameCount, int tileIndex);
    }

    internal class TextureRegionEnumerator : ITextureRegionEnumerator
    {
        private readonly ITextureGraphContext context;

        public int SourceFrameCount {get; set;}
        public int DestFrameCount {get; set;}
        public int? TargetFrame {get; set;}
        public int? TargetPart {get; set;}

        private int ActualDestFrameCount => TargetFrame.HasValue ? 1 : DestFrameCount;


        public TextureRegionEnumerator(ITextureGraphContext context)
        {
            this.context = context;

            SourceFrameCount = 1;
            DestFrameCount = 1;
        }

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
            var renderFrame = new TextureRenderFrame();

            if (context.IsMaterialCtm && !CtmTypes.IsConnectedType(context.Material.CTM?.Method)) {
                var tileCount = GetPublishTileCount();
                var tileRange = TargetPart.HasValue
                    ? Enumerable.Repeat(TargetPart.Value, 1)
                    : Enumerable.Range(0, tileCount);

                renderFrame.Tiles = tileRange
                    .Select(z => new TextureRenderTile {
                        SourceBounds = GetFrameTileBounds(frameIndex, SourceFrameCount, z),
                        DestBounds = GetFrameTileBounds(frameIndex, ActualDestFrameCount, z),
                    }).ToArray();
            }
            else {
                renderFrame.Tiles = new[] {
                    new TextureRenderTile {
                        SourceBounds = TargetPart.HasValue
                            ? GetFrameTileBounds(frameIndex, SourceFrameCount, TargetPart.Value)
                            : GetFrameBounds(frameIndex, SourceFrameCount),
                        DestBounds = GetFrameBounds(frameIndex, ActualDestFrameCount),
                    },
                };
            }

            return renderFrame;
        }

        public IEnumerable<TexturePublishPart> GetAllPublishRegions()
        {
            IEnumerable<TexturePublishFrame> GetFrames(int partIndex) {
                if (TargetFrame.HasValue) return new [] {GetPublishPartFrame(TargetFrame.Value, partIndex)};

                return Enumerable.Range(0, DestFrameCount)
                    .Select(f => GetPublishPartFrame(f, partIndex));
            }

            if (TargetPart.HasValue) {
                var p = TargetPart.Value;

                return new TexturePublishPart[] {
                    new() {
                        Name = GetTileName(p),
                        TileIndex = p,
                        Frames = GetFrames(p).ToArray(),
                    }
                };
            }

            var tileCount = GetPublishTileCount();

            return Enumerable.Range(0, tileCount)
                .Select(t => new TexturePublishPart {
                    Name = GetTileName(t),
                    TileIndex = t,
                    Frames = GetFrames(t).ToArray(),
                });
        }

        private string GetTileName(int index)
        {
            if (context.IsMaterialMultiPart)
                return context.Material.Parts[index].Name;

            if (context.IsMaterialCtm)
                return (index + 1).ToString();

            return context.Material.Name;
        }

        public TexturePublishFrame GetPublishPartFrame(int frameIndex, int tileIndex)
        {
            var frameHeight = 1d;
            if (!TargetFrame.HasValue && ActualDestFrameCount > 1)
                frameHeight = 1d / ActualDestFrameCount;

            return new TexturePublishFrame {
                SourceBounds = GetFrameTileBounds(frameIndex, SourceFrameCount, tileIndex),
                DestBounds = new RectangleF {
                    X = 0f,
                    Y = (float)(frameIndex * frameHeight),
                    Width = 1f,
                    Height = (float)frameHeight,
                },
            };
        }

        private int GetPublishTileCount()
        {
            if (context.IsMaterialMultiPart)
                return context.Material.Parts.Count;

            if (context.IsMaterialCtm)
                return CtmTypes.GetBounds(context.Material?.CTM)?.Total ?? 1;

            return 1;
        }

        private static RectangleF GetFrameBounds(int frame, int frameCount)
        {
            var frameHeight = 1f;
            if (frameCount > 1) frameHeight /= frameCount;
            var wrappedIndex = frame % frameCount;

            return new RectangleF(0f, wrappedIndex * frameHeight, 1f, frameHeight);
        }

        public RectangleF GetFrameTileBounds(int frameIndex, int frameCount, int tileIndex)
        {
            var frameHeight = 1f;
            if (frameCount > 1) frameHeight /= frameCount;
            var wrappedIndex = frameIndex % frameCount;

            if (context.IsMaterialMultiPart) {
                var part = context.Material.Parts[tileIndex];
                var (maxWidth, maxHeight) = context.Material.GetMultiPartBounds();
                if (maxWidth == 0 || maxHeight == 0) return RectangleF.Empty;

                return new RectangleF {
                    X = (float)(part.Left ?? 0) / maxWidth,
                    Y = (float)(part.Top ?? 0) / maxHeight * frameHeight + wrappedIndex * frameHeight,
                    Width = (float)(part.Width ?? 1) / maxWidth,
                    Height = (float)(part.Height ?? 1) / maxHeight * frameHeight,
                };
            }

            if (context.IsMaterialCtm) {
                var bounds = CtmTypes.GetBounds(context.Material.CTM);
                var tileCountX = bounds?.Width ?? 1;
                var tileCountY = bounds?.Height ?? 1;

                var tileWidth = 1f / tileCountX;
                var tileHeight = frameHeight / tileCountY;

                var row = (int)(tileIndex / tileCountX + 0.25f);

                return new RectangleF {
                    X = tileIndex % tileCountX * tileWidth,
                    Y = wrappedIndex * frameHeight + row * tileHeight,
                    Width = tileWidth,
                    Height = tileHeight,
                };
            }

            return GetFrameBounds(frameIndex, frameCount);
        }
    }

    public class TextureRenderFrame
    {
        public TextureRenderTile[] Tiles {get; set;}
    }

    public class TextureRenderTile
    {
        public RectangleF SourceBounds {get; set;}
        public RectangleF DestBounds {get; set;}
    }

    public class TexturePublishPart
    {
        public string Name {get; set;}
        public int TileIndex {get; set;}
        public TexturePublishFrame[] Frames {get; set;}
    }

    public class TexturePublishFrame
    {
        public RectangleF SourceBounds {get; set;}
        public RectangleF DestBounds {get; set;}
    }
}
