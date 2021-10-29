using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Textures.Graphing;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.Textures
{
    public interface ITextureRegionEnumerator
    {
        IEnumerable<TextureRenderFrame> GetAllRenderRegions(int? index, int frameCount);
        IEnumerable<TexturePublishPart> GetAllPublishRegions(int frameCount, int? frame = null, int? part = null);
        TextureRenderFrame GetRenderRegion(int index, int frameCount);
        TexturePublishFrame GetPublishPartFrame(int frameIndex, int frameCount, int tileIndex);
    }

    internal class TextureRegionEnumerator : ITextureRegionEnumerator
    {
        private readonly ITextureGraphContext context;


        public TextureRegionEnumerator(ITextureGraphContext context)
        {
            this.context = context;
        }

        public IEnumerable<TextureRenderFrame> GetAllRenderRegions(int? index, int frameCount)
        {
            if (index.HasValue) {
                yield return GetRenderRegion(index.Value, frameCount);
                yield break;
            }

            for (var frame = 0; frame < frameCount; frame++)
                yield return GetRenderRegion(frame, frameCount);
        }

        public TextureRenderFrame GetRenderRegion(int index, int frameCount)
        {
            var frame = new TextureRenderFrame {
                Index = index % frameCount,
                Bounds = GetFrameBounds(index, frameCount),
            };

            var tileCount = GetPublishTileCount();
            //var isFullCtm = CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Full);
            //var isCompactCtm = CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Compact);

            if (context.Material.CTM?.Method != null) {
                frame.Tiles = Enumerable.Range(0, tileCount)
                    .Select(z => new TextureRenderTile {
                        Index = z,
                        Bounds = GetFrameTileBounds(index, frameCount, z),
                    }).ToArray();
            }
            else {
                frame.Tiles = new[] {
                    new TextureRenderTile {
                        Index = 0,
                        Bounds = GetFrameBounds(index, frameCount),
                    },
                };
            }

            return frame;
        }

        public IEnumerable<TexturePublishPart> GetAllPublishRegions(int frameCount, int? frame = null, int? part = null)
        {
            if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));

            IEnumerable<TexturePublishFrame> GetFrames(int partIndex) {
                if (frame.HasValue) return new [] {GetPublishPartFrame(frame.Value, frameCount, partIndex)};

                return Enumerable.Range(0, frameCount)
                    .Select(f => GetPublishPartFrame(f, frameCount, partIndex));
            }

            if (part.HasValue) {
                return new TexturePublishPart[] {
                    new() {
                        Name = GetTileName(part.Value),
                        TileIndex = part.Value,
                        Frames = GetFrames(part.Value).ToArray(),
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
                return index.ToString();

            return context.Material.Name;
        }

        public TexturePublishFrame GetPublishPartFrame(int frameIndex, int frameCount, int tileIndex)
        {
            var frameHeight = 1f / frameCount;

            return new TexturePublishFrame {
                FrameIndex = frameIndex,
                SourceBounds = GetFrameTileBounds(frameIndex, frameCount, tileIndex),
                DestBounds = new RectangleF {
                    X = 0f,
                    Y = frameIndex * frameHeight,
                    Width = 1f,
                    Height = frameHeight,
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

        private RectangleF GetFrameBounds(int frame, int frameCount)
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

                return new RectangleF {
                    X = (float)(part.Left ?? 0) / maxWidth,
                    Y = (float)(part.Top ?? 0) / maxHeight * frameHeight + wrappedIndex * frameHeight,
                    Width = (float)(part.Width ?? 0) / maxWidth,
                    Height = (float)(part.Height ?? 0) / maxHeight * frameHeight,
                };
            }

            if (context.IsMaterialCtm) {
                var bounds = CtmTypes.GetBounds(context.Material.CTM);
                var tileCountX = bounds?.Width ?? 1;
                var tileCountY = bounds?.Height ?? 1;

                var tileWidth = 1f / tileCountX;
                var tileHeight = frameHeight / tileCountY;

                return new RectangleF {
                    X = (tileIndex % tileCountX) * tileWidth,
                    Y = wrappedIndex * frameHeight + tileIndex / tileCountX * tileHeight,
                    Width = tileWidth,
                    Height = tileHeight,
                };
            }

            //if (CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Horizontal)
            // || CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Vertical)) {
            //    const float tileWidth = 1f / 4f;

            //    return new RectangleF {
            //        X = tileIndex * tileWidth,
            //        Y = wrappedIndex * frameHeight,
            //        Width = tileWidth,
            //        Height = frameHeight,
            //    };
            //}

            //if (CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Compact)) {
            //    const float tileWidth = 1f / 5f;

            //    return new RectangleF {
            //        X = tileIndex * tileWidth,
            //        Y = wrappedIndex * frameHeight,
            //        Width = tileWidth,
            //        Height = frameHeight,
            //    };
            //}

            //if (CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Full)) {
            //    const float tileWidth = 1f / 12f;
            //    var tileHeight = frameHeight / 4f;

            //    return new RectangleF {
            //        X = (tileIndex % 12) * tileWidth,
            //        Y = wrappedIndex * frameHeight + tileIndex / 12 * tileHeight,
            //        Width = tileWidth,
            //        Height = tileHeight,
            //    };
            //}

            //if (CtmTypes.Is(context.Material.CTM?.Method, CtmTypes.Repeat)) {
            //}

            return GetFrameBounds(frameIndex, frameCount);
        }
    }

    public class TextureRenderFrame
    {
        public int Index {get; set;}
        public RectangleF Bounds {get; set;}
        public TextureRenderTile[] Tiles {get; set;}


        //public IEnumerable<TextureRenderTile> GetAllTiles(int? part = null)
        //{
        //    if (part.HasValue) {
        //        yield return Tiles[part.Value];
        //        yield break;
        //    }

        //    foreach (var tile in Tiles)
        //        yield return tile;
        //}
    }

    public class TextureRenderTile
    {
        public int Index {get; set;}
        public RectangleF Bounds {get; set;}
    }

    public class TexturePublishPart
    {
        public string Name {get; set;}
        public int TileIndex {get; set;}
        public TexturePublishFrame[] Frames {get; set;}
    }

    public class TexturePublishFrame
    {
        public int FrameIndex {get; set;}
        public RectangleF SourceBounds {get; set;}
        public RectangleF DestBounds {get; set;}
    }
}
