using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Entities;
using MinecraftMappings.Internal.Models;
using PixelGraph.Common.Extensions;
using SharpDX;
using System;

namespace PixelGraph.UI.Helix.Models
{
    internal interface IModelBuilder
    {
        BlockMeshGeometry3D BuildCross(in float cubeSize);
        BlockMeshGeometry3D BuildCube(in float cubeSize, in int gridSizeX = 1, in int gridSizeY = 1, in int gridSizeZ = 1);
        BlockMeshGeometry3D BuildEntity(in float cubeSize, EntityDataVersion entity);
        BlockMeshGeometry3D BuildModel(in float cubeSize, ModelVersion model);
    }

    internal class ModelBuilder : IModelBuilder
    {
        private const float BlockToWorld = 1f / 16f;
        private static readonly float crossDiagonal = MathEx.CosD(45f);
        private static readonly Vector3 blockCenter = new(8, 8, 8);


        public BlockMeshGeometry3D BuildCross(in float cubeSize)
        {
            var builder = new BlockMeshBuilder();

            var uv = new RectangleF(0f, 0f, 1f, 1f);
            var uvInvX = new RectangleF(1f, 0f, 0f, 1f);

            AddCubeFace(builder, Matrix.Identity, new Vector3( crossDiagonal, 0,  crossDiagonal), Vector3.UnitY, 0f, cubeSize, cubeSize, in uv, 0);
            AddCubeFace(builder, Matrix.Identity, new Vector3(-crossDiagonal, 0, -crossDiagonal), -Vector3.UnitY, 0f, cubeSize, cubeSize, in uvInvX, 0);

            AddCubeFace(builder, Matrix.Identity, new Vector3( crossDiagonal, 0, -crossDiagonal), Vector3.UnitY, 0f, cubeSize, cubeSize, in uv, 0);
            AddCubeFace(builder, Matrix.Identity, new Vector3(-crossDiagonal, 0,  crossDiagonal), -Vector3.UnitY, 0f, cubeSize, cubeSize, in uvInvX, 0);

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToBlockMeshGeometry3D();
        }

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

        //public MeshGeometry3D BuildBell(in float cubeSize)
        //{
        //    var builder = new MeshBuilder(true, true, true);

        //    var centerTop = new Vector3(0f, -2.5f, 0f) - blockCenter;
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 1, 0,  0), Vector3.UnitY, 6, 6, 7, new Vector2(0f, 3/16f), new Vector2(3/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3(-1, 0,  0), Vector3.UnitY, 6, 6, 7, new Vector2(6/16f, 3/16f), new Vector2(9/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 0, 0,  1), Vector3.UnitY, 6, 6, 7, new Vector2(9/16f, 3/16f), new Vector2(12/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 0, 0, -1), Vector3.UnitY, 6, 6, 7, new Vector2(3/16f, 3/16f), new Vector2(6/16f, 6.5f/16f));
        //    builder.AddEntityCubeFace(centerTop, new Vector3( 0, 1,  0), Vector3.UnitX, 7, 6, 6, new Vector2(6/16f, 0f), new Vector2(9/16f, 3/16f));

        //    var centerBottom = new Vector3(0f, -7f, 0f) - blockCenter;
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 1,  0,  0), Vector3.UnitY, 8, 8, 2, new Vector2(0f, 10.5f/16f), new Vector2(4/16f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3(-1,  0,  0), Vector3.UnitY, 8, 8, 2, new Vector2(8/16f, 10.5f/16f), new Vector2(12/16f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0,  0,  1), Vector3.UnitY, 8, 8, 2, new Vector2(12/16f, 10.5f/16f), new Vector2(1f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0,  0, -1), Vector3.UnitY, 8, 8, 2, new Vector2(4/16f, 10.5f/16f), new Vector2(8/16f, 11.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0,  1,  0), Vector3.UnitX, 2, 8, 8, new Vector2(8/16f, 6.5f/16f), new Vector2(12/16f, 10.5f/16f));
        //    builder.AddEntityCubeFace(centerBottom, new Vector3( 0, -1,  0), Vector3.UnitX, 2, 8, 8, new Vector2(4/16f, 6.5f/16f), new Vector2(8/16f, 10.5f/16f));

        //    //var scale = BlockToWorld * cubeSize;
        //    //builder.Scale(scale, scale, scale);

        //    builder.ComputeTangents(MeshFaces.Default);
        //    return builder.ToMeshGeometry3D();
        //}

        public BlockMeshGeometry3D BuildModel(in float cubeSize, ModelVersion modelVersion)
        {
            var builder = new BlockMeshBuilder();

            RectangleF uvScaled;
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

                foreach (var (face, faceData) in element.Faces) {
                    if (faceData == null) continue;
                    
                    up = GetUpVector(face);
                    normal = GetFaceNormal(face);
                    (width, height, offset) = element.GetWidthHeightOffset(in face);

                    uvScaled = Multiply(in faceData.UV, BlockToWorld);
                    AddCubeFace(builder, in mWorld, in normal, in up, in offset, in width, in height, in uvScaled, in faceData.Rotation);
                }
            }

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToBlockMeshGeometry3D();
        }

        private static RectangleF Multiply(in RectangleF region, in float scale)
        {
            return new RectangleF {
                X = region.X * scale,
                Y = region.Y * scale,
                Right = region.Right * scale,
                Bottom = region.Bottom * scale,
            };
        }

        public BlockMeshGeometry3D BuildEntity(in float cubeSize, EntityDataVersion entityVersion)
        {
            var builder = new BlockMeshBuilder();

            Matrix mWorld;
            RectangleF uvScaled;
            Vector3 normal, up;
            float offset, width, height;

            foreach (var element in entityVersion.Elements) {
                Matrix.Scaling(BlockToWorld * cubeSize, out mWorld);
                
                var position = element.Position + element.Size * 0.5f;
                mWorld *= Matrix.Translation(position);

                foreach (var (face, region) in element.Faces) {
                    up = GetUpVector(face);
                    normal = GetFaceNormal(face);
                    (width, height, offset) = element.GetWidthHeightOffset(in face);
                    uvScaled = Multiply(region, BlockToWorld);

                    AddCubeFace(builder, in mWorld, in normal, in up, in offset, in width, in height, in uvScaled, 0);
                }
            }

            builder.ComputeTangents(MeshFaces.Default);
            return builder.ToBlockMeshGeometry3D();
        }

        private static Vector3 GetFaceNormal(ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => new Vector3(0, 1, 0),
                ElementFaces.Down => new Vector3(0, -1, 0),
                ElementFaces.North => new Vector3(0, 0, 1),
                ElementFaces.South => new Vector3(0, 0, -1),
                ElementFaces.East => new Vector3(-1, 0, 0),
                ElementFaces.West => new Vector3(1, 0, 0),
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
            AddCubeFace(builder, in mWorld, new Vector3(-1, 0, 0), -Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, 0, 1), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, 0, -1), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, 1, 0), Vector3.UnitX, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(builder, in mWorld, new Vector3(0, -1, 0), Vector3.UnitX, in size, in size, in size, in uv, in uvRotation);
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

                //for (var i = 0; i < 4; i++) {
                //    switch (uvRotation) {
                //        case 0:
                //            builder.TextureCoordinateMins.Add(new Vector2(uv.Left, uv.Top));
                //            builder.TextureCoordinateMaxs.Add(new Vector2(uv.Right, uv.Bottom));
                //            break;
                //        case 90:
                //            builder.TextureCoordinateMins.Add(new Vector2(uv.Top, uv.Left));
                //            builder.TextureCoordinateMaxs.Add(new Vector2(uv.Bottom, uv.Right));
                //            break;
                //        case 180:
                //            builder.TextureCoordinateMins.Add(new Vector2(uv.Right, uv.Bottom));
                //            builder.TextureCoordinateMaxs.Add(new Vector2(uv.Left, uv.Top));
                //            break;
                //        case 270:
                //            builder.TextureCoordinateMins.Add(new Vector2(uv.Top, uv.Right));
                //            builder.TextureCoordinateMaxs.Add(new Vector2(uv.Bottom, uv.Left));
                //            break;
                //    }
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
    }
}
