using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftMappings.Internal.Models
{
    public class ModelElement
    {
        public string Name;
        public Vector3 From;
        public Vector3 To;
        public ModelElementRotation Rotation;
        public ModelFace FaceUp;
        public ModelFace FaceDown;
        public ModelFace FaceNorth;
        public ModelFace FaceSouth;
        public ModelFace FaceWest;
        public ModelFace FaceEast;

        public IEnumerable<(ElementFaces, ModelFace)> Faces => Enum.GetValues(typeof(ElementFaces))
            .OfType<ElementFaces>().Select(face => (face, GetFace(face)));


        public ModelFace GetFace(in ElementFaces face)
        {
            switch (face) {
                case ElementFaces.Up:
                    return FaceUp;
                case ElementFaces.Down:
                    return FaceDown;
                case ElementFaces.North:
                    return FaceNorth;
                case ElementFaces.South:
                    return FaceSouth;
                case ElementFaces.West:
                    return FaceWest;
                case ElementFaces.East:
                    return FaceEast;
                default:
                    throw new ApplicationException($"Unknown element face '{face}'!");
            }
        }

        public void SetFace(in ElementFaces face, ModelFace modelFace)
        {
            switch (face) {
                case ElementFaces.Up:
                    FaceUp = modelFace;
                    break;
                case ElementFaces.Down:
                    FaceDown = modelFace;
                    break;
                case ElementFaces.North:
                    FaceNorth = modelFace;
                    break;
                case ElementFaces.South:
                    FaceSouth = modelFace;
                    break;
                case ElementFaces.West:
                    FaceWest = modelFace;
                    break;
                case ElementFaces.East:
                    FaceEast = modelFace;
                    break;
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
                        width: To.X - From.X,
                        height: To.Z - From.Z,
                        offset: To.Y - From.Y);
                case ElementFaces.North:
                case ElementFaces.South:
                    return (
                        width: To.X - From.X,
                        height: To.Y - From.Y,
                        offset: To.Z - From.Z);
                case ElementFaces.East:
                case ElementFaces.West:
                    return (
                        width: To.Z - From.Z,
                        height: To.Y - From.Y,
                        offset: To.X - From.X);
                default:
                    throw new ApplicationException($"Unknown element face '{face}'!");
            }
        }
    }
}
