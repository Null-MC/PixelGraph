using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Models;
using MinecraftMappings.Internal.Models.Block;
using PixelGraph.Common.Extensions;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Models
{
    public interface IBlockModelBuilder : IModelBuilder
    {
        void AppendModelTextureParts(in float cubeSize, Vector3 offset, BlockModelVersion model, string textureId = null);
    }

    public class BlockModelBuilder : ModelBuilder, IBlockModelBuilder
    {
        private static readonly Vector3 blockCenter = new(8f, 8f, 8f);


        public void Clear() => Builder.Clear();

        public void AppendModelTextureParts(in float cubeSize, Vector3 offset, BlockModelVersion modelVersion, string textureId = null)
        {
            Vector3 faceNormal, faceUp;
            float faceOffset, faceWidth, faceHeight;
            Matrix mWorld;

            foreach (var element in modelVersion.Elements) {
                var halfBlock = (element.To - element.From) * 0.5f;
                mWorld = Matrix.Translation(element.From + halfBlock);

                if (element.Rotation != null) {
                    var axis = element.Rotation.GetAxisVector();
                    var angleR = (float)element.Rotation.Angle * MathEx.Deg2RadF;

                    mWorld *= Matrix.Translation(-element.Rotation.Origin);
                    mWorld *= Matrix.RotationAxis(axis, angleR);
                    mWorld *= Matrix.Translation(element.Rotation.Origin);
                }

                mWorld *= Matrix.Translation(-blockCenter + offset);
                mWorld *= Matrix.Scaling(BlockToWorld * cubeSize);

                foreach (var face in ModelElement.AllFaces) {
                    // Skip if no face data
                    var faceData = element.GetFace(face);
                    if (faceData == null) continue;
                    
                    // Skip if building a specific texture that doesn't match this ID
                    if (textureId != null && !string.Equals($"#{textureId}", faceData.Texture, StringComparison.InvariantCultureIgnoreCase)) continue;

                    faceUp = GetUpVector(face);
                    faceNormal = GetFaceNormal(face);
                    (faceWidth, faceHeight, faceOffset) = element.GetWidthHeightOffset(in face);
                    var rotation = faceData.Rotation ?? 0;

                    var uv = faceData.UV ?? GetDefaultUv(element, in face);
                    Multiply(in uv, BlockToWorld, out uv);

                    AddCubeFace(in mWorld, in faceNormal, in faceUp, in faceOffset, in faceWidth, in faceHeight, in uv, in rotation);
                }
            }

            Builder.ComputeTangents(MeshFaces.Default);
        }

        private static void Multiply(in RectangleF region, in float scale, out RectangleF scaledRegion)
        {
            scaledRegion.Left = region.Left * scale;
            scaledRegion.Top = region.Top * scale;
            scaledRegion.Right = region.Right * scale;
            scaledRegion.Bottom = region.Bottom * scale;
        }

        private RectangleF GetDefaultUv(ModelElement element, in ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => UVMap(element.From.X, element.From.Z, element.To.X, element.To.Z),
                ElementFaces.Down => UVMap(element.From.X, element.To.Z, element.To.X, element.From.Z),
                ElementFaces.North => UVMap(element.To.X, element.From.Y, element.From.X, element.To.Y),
                ElementFaces.South => UVMap(element.From.X, element.From.Y, element.To.X, element.To.Y),
                ElementFaces.West => UVMap(element.From.Z, element.From.Y, element.To.Z, element.To.Y),
                ElementFaces.East => UVMap(element.To.Z, element.From.Y, element.From.Z, element.To.Y),
                _ => throw new ApplicationException($"Unknown element face '{face}'!")
            };
        }

        private static Vector3 GetFaceNormal(ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => new Vector3(0, 1, 0),
                ElementFaces.Down => new Vector3(0, -1, 0),
                ElementFaces.North => new Vector3(0, 0, -1),
                ElementFaces.South => new Vector3(0, 0, 1),
                ElementFaces.East => new Vector3(1, 0, 0),
                ElementFaces.West => new Vector3(-1, 0, 0),
                _ => throw new ApplicationException($"Unknown element face '{face}'!"),
            };
        }

        private static Vector3 GetUpVector(ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => -Vector3.UnitZ,
                ElementFaces.Down => Vector3.UnitZ,
                ElementFaces.North => Vector3.UnitY,
                ElementFaces.South => Vector3.UnitY,
                ElementFaces.East => Vector3.UnitY,
                ElementFaces.West => Vector3.UnitY,
                _ => throw new ApplicationException($"Unknown element face '{face}'!"),
            };
        }

        private static RectangleF UVMap(float left, float top, float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }
    }
}
