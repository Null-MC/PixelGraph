using MinecraftMappings.Internal.Models;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftMappings.Internal.Entities
{
    public class EntityElement
    {
        public string Name {get; set;}
        public Vector3 Position {get; set;}
        public Vector3 Size {get; set;}
        public Vector2 UV {get; set;}
        //public Vector3 Pivot {get; set;}
        //public ModelElementRotation Rotation {get; set;}

        public IEnumerable<(ElementFaces face, RectangleF region)> Faces => Enum.GetValues(typeof(ElementFaces))
            .OfType<ElementFaces>().Select(face => (face, GetFaceRectangle(face)));


        public RectangleF GetFaceRectangle(ElementFaces face)
        {
            switch (face) {
                case ElementFaces.Up:
                    return new RectangleF(UV.X + Size.Z, UV.Y, Size.X, Size.Z);
                case ElementFaces.Down:
                    return new RectangleF(UV.X + Size.Z + Size.X, UV.Y, Size.X, Size.Z);
                case ElementFaces.East:
                    return new RectangleF(UV.X, UV.Y + Size.Z, Size.Z, Size.Y);
                case ElementFaces.North:
                    return new RectangleF(UV.X + Size.Z, UV.Y + Size.Z, Size.X, Size.Y);
                case ElementFaces.West:
                    return new RectangleF(UV.X + Size.Z + Size.X, UV.Y + Size.Z, Size.Z, Size.Y);
                case ElementFaces.South:
                    return new RectangleF(UV.X + 2f * Size.Z + Size.X, UV.Y + Size.Z, Size.X, Size.Y);
                default:
                    throw new ApplicationException($"Unknown element face '{face}'!");
            }
        }

        public (float width, float height, float offset) GetWidthHeightOffset(in ElementFaces face)
        {
            switch (face) {
                case ElementFaces.Up:
                case ElementFaces.Down:
                    return (
                        width: Size.X,
                        height: Size.Z,
                        offset: Size.Y);
                case ElementFaces.North:
                case ElementFaces.South:
                    return (
                        width: Size.X,
                        height: Size.Y,
                        offset: Size.Z);
                case ElementFaces.East:
                case ElementFaces.West:
                    return (
                        width: Size.Z,
                        height: Size.Y,
                        offset: Size.X);
                default:
                    throw new ApplicationException($"Unknown element face '{face}'!");
            }
        }
    }
}
