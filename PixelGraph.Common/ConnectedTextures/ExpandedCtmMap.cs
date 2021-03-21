using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace PixelGraph.Common.ConnectedTextures
{
    internal class ExpandedCtmMap
    {
        public IEnumerable<ImagePart> GetExpandedCtmRegions(int tileWidth, int tileHeight)
        {
            var hx = tileWidth / 2;
            var hy = tileHeight / 2;

            yield return new ImagePart {
                Name = "0",
                Bounds = GetTileBounds(in tileWidth, in tileHeight, 0, 0),
                Mappings = new [] {
                    new ImagePart.MultiPart {
                        SourceBounds = GetTileBounds(in tileWidth, in tileHeight, 0, 0),
                        DestBounds = GetTileBounds(in tileWidth, in tileHeight, 0, 0),
                    },
                },
            };

            yield return new ImagePart {
                Name = "1",
                Bounds = GetTileBounds(in tileWidth, in tileHeight, 1, 0),
                Mappings = new[] {
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(0, 0, hx, tileHeight),
                        DestBounds = new Rectangle(tileWidth, 0, hx, tileHeight),
                    },
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(tileWidth*3, 0, hx, tileHeight),
                        DestBounds = new Rectangle(tileWidth, 0, hx, tileHeight),
                    },
                },
            };

            yield return new ImagePart {
                Name = "2",
                Bounds = GetTileBounds(in tileWidth, in tileHeight, 2, 0),
                Mappings = new[] {
                    new ImagePart.MultiPart {
                        SourceBounds = GetTileBounds(in tileWidth, in tileHeight, 3, 0),
                        DestBounds = GetTileBounds(in tileWidth, in tileHeight, 2, 0),
                    },
                },
            };

            yield return new ImagePart {
                Name = "3",
                Bounds = GetTileBounds(in tileWidth, in tileHeight, 3, 0),
                Mappings = new[] {
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(tileWidth*3, 0, hx, tileHeight),
                        DestBounds = new Rectangle(tileWidth*3, 0, hx, tileHeight),
                    },
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(hx, 0, hx, tileHeight),
                        DestBounds = new Rectangle(tileWidth*3 + hx, 0, hx, tileHeight),
                    },
                },
            };

            yield return new ImagePart {
                Name = "4",
                Bounds = GetTileBounds(in tileWidth, in tileHeight, 4, 0),
                Mappings = new[] {
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(0, 0, hx, hy),
                        DestBounds = new Rectangle(tileWidth*4, 0, hx, hy),
                    },
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(tileWidth*3 + hx, 0, hx, hy),
                        DestBounds = new Rectangle(tileWidth*4 + hx, 0, hx, hy),
                    },
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(tileWidth*2, hy, hx, hy),
                        DestBounds = new Rectangle(tileWidth*4, hy, hx, hy),
                    },
                    new ImagePart.MultiPart {
                        SourceBounds = new Rectangle(tileWidth*4 + hx, hy, hx, hy),
                        DestBounds = new Rectangle(tileWidth*4 + hx, hy, hx, hy),
                    },
                },
            };

            //...

            yield return new ImagePart {
                Name = "26",
                Bounds = GetTileBounds(in tileWidth, in tileHeight, 2, 2),
                Mappings = new [] {
                    new ImagePart.MultiPart {
                        SourceBounds = GetTileBounds(in tileWidth, in tileHeight, 1, 0),
                        DestBounds = GetTileBounds(in tileWidth, in tileHeight, 2, 2),
                    }
                }
            };

            //...

            throw new NotImplementedException();
        }

        private Rectangle GetTileBounds(in int tileWidth, in int tileHeight, in int x, in int y)
        {
            return new Rectangle {
                X = x * tileWidth,
                Y = y * tileHeight,
                Width = tileWidth,
                Height = tileHeight,
            };
        }
    }

    public class ImagePart
    {
        public string Name {get; set;}
        public Rectangle Bounds {get; set;}
        public MultiPart[] Mappings {get; set;}

        public class MultiPart
        {
            public Rectangle SourceBounds {get; set;}
            public Rectangle DestBounds {get; set;}
        }
    }
}
