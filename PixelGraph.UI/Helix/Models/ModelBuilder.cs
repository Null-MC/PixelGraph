using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Entities;
using MinecraftMappings.Internal.Models;
using PixelGraph.Common.Extensions;
using SharpDX;
using System;
using System.Linq;

namespace PixelGraph.UI.Helix.Models
{
    internal interface IModelBuilder
    {
        BlockMeshGeometry3D BuildCube(in float cubeSize, in int gridSizeX = 1, in int gridSizeY = 1, in int gridSizeZ = 1);
        BlockMeshGeometry3D BuildEntity(in float cubeSize, EntityDataVersion entity);
        BlockMeshGeometry3D BuildModel(in float cubeSize, ModelVersion model);
    }

    internal class ModelBuilder : IModelBuilder
    {
        private const float BlockToWorld = 1f / 16f;
        private static readonly Vector3 blockCenter = new(8, 8, 8);


        public BlockMeshGeometry3D BuildCube(in float cubeSize, in int gridSizeX = 1, in int gridSizeY = 1, in int gridSizeZ = 1)
        {
            var builder = new BlockMeshBuilder();

            var uv = new RectangleF(0f, 0f, 1f, 1f);
            Vector3 offset, position;
            Matrix mWorld;

            offset.X = cubeSize * (gridSizeX - 1) * 0.5f;
            offset.Y = cubeSize * (gridSizeY - 1) * 0.5f;
            offset.Z = cubeSize * (gridSizeZ - 1) * 0.5f;

            for (var z = 0; z < gridSizeZ; z++) {
                for (var y = 0; y < gridSizeY; y++) {
                    for (var x = 0; x < gridSizeX; x++) {
                        position.X = cubeSize * x - offset.X;
                        position.Y = cubeSize * y - offset.Y;
                        position.Z = cubeSize * z - offset.Z;

                        Matrix.Translation(ref position, out mWorld);
                        AppendCube(builder, mWorld, in cubeSize, in uv, 0);
                    }
                }
            }

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToBlockMeshGeometry3D();
        }

        public BlockMeshGeometry3D BuildModel(in float cubeSize, ModelVersion modelVersion)
        {
            var builder = new BlockMeshBuilder();

            Vector3 normal, up;
            float offset, width, height;
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

                mWorld *= Matrix.Translation(-blockCenter);
                mWorld *= Matrix.Scaling(BlockToWorld * cubeSize);

                foreach (var face in ModelElement.AllFaces) {
                    var faceData = element.GetFace(face);
                    if (faceData == null) continue;
                    
                    up = GetUpVector(face);
                    normal = GetFaceNormal(face);
                    (width, height, offset) = element.GetWidthHeightOffset(in face);
                    var rotation = faceData.Rotation ?? 0;

                    var uv = faceData.UV ?? GetDefaultUv(element, in face);
                    Multiply(in uv, BlockToWorld, out uv);

                    AddCubeFace(builder, in mWorld, in normal, in up, in offset, in width, in height, in uv, in rotation);
                }
            }

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToBlockMeshGeometry3D();
        }

        public BlockMeshGeometry3D BuildEntity(in float cubeSize, EntityDataVersion entityVersion)
        {
            var builder = new BlockMeshBuilder();


            foreach (var element in entityVersion.Elements) {
                AppendEntityElement(builder, element, in Matrix.Identity, in cubeSize);
            }

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToBlockMeshGeometry3D();
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

        private static void AppendEntityElement(BlockMeshBuilder builder, EntityElement element, in Matrix mWorldParent, in float cubeSize)
        {
            var halfBlock = element.Size * 0.5f;
            var mWorld = Matrix.Translation(element.Position + halfBlock);
            var m2 = Matrix.Translation(element.Position);

            if (element.HasRotation) {
                mWorld *= Matrix.Translation(-element.RotationOrigin);
                m2 *= Matrix.Translation(-element.RotationOrigin);

                if (element.RotationAngleX != 0) {
                    var angleR = (float)element.RotationAngleX * MathEx.Deg2RadF;
                    mWorld *= Matrix.RotationAxis(Vector3.UnitX, angleR);
                    m2 *= Matrix.RotationAxis(Vector3.UnitX, angleR);
                }

                if (element.RotationAngleY != 0) {
                    var angleR = (float)element.RotationAngleY * MathEx.Deg2RadF;
                    mWorld *= Matrix.RotationAxis(Vector3.UnitY, angleR);
                    m2 *= Matrix.RotationAxis(Vector3.UnitY, angleR);
                }

                if (element.RotationAngleZ != 0) {
                    var angleR = (float)element.RotationAngleZ * MathEx.Deg2RadF;
                    mWorld *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
                    m2 *= Matrix.RotationAxis(Vector3.UnitZ, angleR);
                }

                mWorld *= Matrix.Translation(element.RotationOrigin);
                m2 *= Matrix.Translation(element.RotationOrigin);
            }

            //m2 *= Matrix.Translation(-element.Position);

            if (element.Elements?.Any() ?? false) {
                foreach (var childElement in element.Elements) {
                    var m3 = mWorldParent * m2 * Matrix.Translation(element.Position - childElement.Position);
                    AppendEntityElement(builder, childElement, in m3, in cubeSize);
                }
            }

            mWorld = mWorldParent * mWorld;

            mWorld *= Matrix.Translation(-blockCenter);
            mWorld *= Matrix.Scaling(BlockToWorld * cubeSize);

            var uvScaled = new RectangleF();
            float offset, width, height;
            Vector3 normal, up;

            foreach (var (face, region) in element.Faces) {
                up = GetUpVector(face);
                normal = GetFaceNormal(face);
                (width, height, offset) = element.GetWidthHeightOffset(in face);

                uvScaled.Top = region.Top / element.TextureSize.Y;
                uvScaled.Bottom = region.Bottom / element.TextureSize.Y;
                uvScaled.Left = region.Left / element.TextureSize.X;
                uvScaled.Right = region.Right / element.TextureSize.X;

                AddCubeFace(builder, in mWorld, in normal, in up, in offset, in width, in height, in uvScaled, 0);
            }
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

        private static void AppendCube(BlockMeshBuilder builder, in Matrix mWorld, in float size, in RectangleF uv, in int uvRotation)
        {
            AddCubeFace(builder, in mWorld, new Vector3(1, 0, 0), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(-1, 0, 0), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, 0, 1), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, 0, -1), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, 1, 0), -Vector3.UnitZ, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, -1, 0), Vector3.UnitZ, in size, in size, in size, in uv, in uvRotation);
        }

        private static void AddCubeFace(BlockMeshBuilder builder, in Matrix mWorld, in Vector3 normal, in Vector3 up, in float dist, in float width, in float height, in RectangleF uv, in int uvRotation)
        {
            CrossProduct(in normal, in up, out var right);

            var n = normal * dist * 0.5f;
            var _up = up * height * 0.5f;
            right *= width * 0.5f;

            var p1 = n - _up - right;
            var p2 = n - _up + right;
            var p3 = n + _up + right;
            var p4 = n + _up - right;

            var i0 = builder.Positions.Count;

            var wPos1 = Vector3.Transform(p1, mWorld).ToVector3();
            var wPos2 = Vector3.Transform(p2, mWorld).ToVector3();
            var wPos3 = Vector3.Transform(p3, mWorld).ToVector3();
            var wPos4 = Vector3.Transform(p4, mWorld).ToVector3();

            builder.Positions.Add(wPos1);
            builder.Positions.Add(wPos2);
            builder.Positions.Add(wPos3);
            builder.Positions.Add(wPos4);

            var wNormal = Vector3.TransformNormal(normal, mWorld);
            Vector3.Normalize(ref wNormal, out wNormal);

            if (builder.HasNormals)
                builder.AddNormal4x(wNormal);

            if (builder.HasTexCoords) {
                switch (uvRotation) {
                    case 0:
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        break;
                    case 90:
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        break;
                    case 180:
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        break;
                    case 270:
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        break;
                    default:
                        throw new ApplicationException($"Found invalid model element face UV rotation value '{uvRotation}'!");
                }

                builder.AddTextureCoordinateMin4x(uv.TopLeft);
                builder.AddTextureCoordinateMax4x(uv.BottomRight);
                //builder.AddTextureCoordinateMin4x(new Vector2(uv.Right, uv.Bottom));
                //builder.AddTextureCoordinateMax4x(new Vector2(uv.Left, uv.Top));

                //switch (uvRotation) {
                //    case 0:
                //    case 180:
                //        builder.AddTextureCoordinateMin4x(uv.TopLeft);
                //        builder.AddTextureCoordinateMax4x(uv.BottomRight);
                //        break;
                //    case 90:
                //    case 270:
                //        builder.AddTextureCoordinateMin4x(new Vector2(uv.Top, uv.Left));
                //        builder.AddTextureCoordinateMax4x(new Vector2(uv.Bottom, uv.Right));
                //        break;
                //}
            }

            builder.TriangleIndices.Add(i0 + 2);
            builder.TriangleIndices.Add(i0 + 1);
            builder.TriangleIndices.Add(i0 + 0);
            builder.TriangleIndices.Add(i0 + 0);
            builder.TriangleIndices.Add(i0 + 3);
            builder.TriangleIndices.Add(i0 + 2);
        }

        private static void CrossProduct(in Vector3 vector1, in Vector3 vector2, out Vector3 result)
        {
            result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
        }

        private static RectangleF UVMap(float left, float top, float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }
    }
}
