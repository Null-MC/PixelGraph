using HelixToolkit.SharpDX.Core;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Models
{
    public interface IModelBuilder
    {
        void BuildCube(in float cubeSize, in int gridSizeX = 1, in int gridSizeY = 1, in int gridSizeZ = 1);
        BlockMeshGeometry3D ToBlockMeshGeometry3D();
    }

    public abstract class ModelBuilder : IModelBuilder
    {
        protected const float BlockToWorld = 1f / 16f;

        internal BlockMeshBuilder Builder {get;}


        protected ModelBuilder()
        {
            Builder = new BlockMeshBuilder();
        }

        public void BuildCube(in float cubeSize, in int gridSizeX = 1, in int gridSizeY = 1, in int gridSizeZ = 1)
        {
            Builder.Clear();

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
                        AppendCube(mWorld, in cubeSize, in uv, 0);
                    }
                }
            }

            Builder.ComputeTangents(MeshFaces.Default);
        }

        public BlockMeshGeometry3D ToBlockMeshGeometry3D()
        {
            return Builder.ToBlockMeshGeometry3D();
        }

        private void AppendCube(in Matrix mWorld, in float size, in RectangleF uv, in int uvRotation)
        {
            AddCubeFace(in mWorld, new Vector3(0, 1, 0), -Vector3.UnitZ, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(in mWorld, new Vector3(0, -1, 0), Vector3.UnitZ, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(in mWorld, new Vector3(0, 0, -1), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(in mWorld, new Vector3(0, 0, 1), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(in mWorld, new Vector3(1, 0, 0), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
            AddCubeFace(in mWorld, new Vector3(-1, 0, 0), Vector3.UnitY, in size, in size, in size, in uv, in uvRotation);
        }

        protected void AddCubeFace(in Matrix mWorld, in Vector3 normal, in Vector3 up, in float dist, in float width, in float height, in RectangleF uv, in int uvRotation)
        {
            CrossProduct(in normal, in up, out var right);

            var n = normal * dist * 0.5f;
            var _up = up * height * 0.5f;
            right *= width * 0.5f;

            var p1 = n - _up - right;
            var p2 = n - _up + right;
            var p3 = n + _up + right;
            var p4 = n + _up - right;

            var i0 = Builder.Positions.Count;

            var wPos1 = Vector3.Transform(p1, mWorld).ToVector3();
            var wPos2 = Vector3.Transform(p2, mWorld).ToVector3();
            var wPos3 = Vector3.Transform(p3, mWorld).ToVector3();
            var wPos4 = Vector3.Transform(p4, mWorld).ToVector3();

            Builder.Positions.Add(wPos1);
            Builder.Positions.Add(wPos2);
            Builder.Positions.Add(wPos3);
            Builder.Positions.Add(wPos4);

            var wNormal = Vector3.TransformNormal(normal, mWorld);
            Vector3.Normalize(ref wNormal, out wNormal);

            if (Builder.HasNormals)
                Builder.AddNormal4x(wNormal);

            if (Builder.HasTexCoords) {
                switch (uvRotation) {
                    case 0:
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));

                        Builder.AddTextureCoordinateMax4x(uv.BottomRight);
                        Builder.AddTextureCoordinateMin4x(uv.TopLeft);
                        break;
                    case 90:
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));

                        Builder.AddTextureCoordinateMax4x(uv.TopRight);
                        Builder.AddTextureCoordinateMin4x(uv.BottomLeft);
                        break;
                    case 180:
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));

                        Builder.AddTextureCoordinateMax4x(uv.TopLeft);
                        Builder.AddTextureCoordinateMin4x(uv.BottomRight);
                        break;
                    case 270:
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Bottom));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Left, uv.Top));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Top));
                        Builder.TextureCoordinates.Add(new Vector2(uv.Right, uv.Bottom));

                        Builder.AddTextureCoordinateMax4x(uv.BottomLeft);
                        Builder.AddTextureCoordinateMin4x(uv.TopRight);
                        break;
                    default:
                        throw new ApplicationException($"Found invalid model element face UV rotation value '{uvRotation}'!");
                }

                //Builder.AddTextureCoordinateMin4x(uv.TopLeft);
                //Builder.AddTextureCoordinateMax4x(uv.BottomRight);
            }

            Builder.TriangleIndices.Add(i0 + 2);
            Builder.TriangleIndices.Add(i0 + 1);
            Builder.TriangleIndices.Add(i0 + 0);
            Builder.TriangleIndices.Add(i0 + 0);
            Builder.TriangleIndices.Add(i0 + 3);
            Builder.TriangleIndices.Add(i0 + 2);
        }

        private static void CrossProduct(in Vector3 vector1, in Vector3 vector2, out Vector3 result)
        {
            result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
        }
    }
}
