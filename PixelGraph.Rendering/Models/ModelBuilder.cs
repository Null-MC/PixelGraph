using HelixToolkit.SharpDX.Core;
using SharpDX;
using System;
using System.Text;
using MinecraftMappings.Internal.Models;

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
                var texcoords = GetRotatedTexcoords(in uv, in uvRotation);
                Builder.TextureCoordinates.AddRange(texcoords);

                var (texMin, texMax) = GetTexMinMax(in uv, in uvRotation);
                Builder.AddTextureCoordinateMin4x(texMin);
                Builder.AddTextureCoordinateMax4x(texMax);

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

        public static RectangleF GetRotatedRegion(in RectangleF uv, in int uvRotation)
        {
            return uvRotation switch {
                0 => new RectangleF {
                    Left = uv.Left,
                    Top = uv.Top,
                    Right = uv.Right,
                    Bottom = uv.Bottom,
                },
                90 => new RectangleF {
                    Left = uv.Bottom,
                    Top = uv.Left,
                    Right = uv.Top,
                    Bottom = uv.Right,
                },
                180 => new RectangleF {
                    Left = uv.Right,
                    Top = uv.Bottom,
                    Right = uv.Left,
                    Bottom = uv.Top,
                },
                270 => new RectangleF {
                    Left = uv.Top,
                    Top = uv.Right,
                    Right = uv.Bottom,
                    Bottom = uv.Left,
                },
                _ => throw new ApplicationException($"Invalid block model texture rotation value '{uvRotation}'!")
            };
        }

        private static Vector2[] GetRotatedTexcoords(in RectangleF uv, in int uvRotation)
        {
            return uvRotation switch {
                0 => new[] {
                    new Vector2(uv.Right, uv.Bottom),
                    new Vector2(uv.Left, uv.Bottom),
                    new Vector2(uv.Left, uv.Top),
                    new Vector2(uv.Right, uv.Top),
                },
                90 => new[] {
                    new Vector2(uv.Right, uv.Top),
                    new Vector2(uv.Right, uv.Bottom),
                    new Vector2(uv.Left, uv.Bottom),
                    new Vector2(uv.Left, uv.Top),
                },
                180 => new[] {
                    new Vector2(uv.Left, uv.Top),
                    new Vector2(uv.Right, uv.Top),
                    new Vector2(uv.Right, uv.Bottom),
                    new Vector2(uv.Left, uv.Bottom),
                },
                270 => new[] {
                    new Vector2(uv.Left, uv.Bottom),
                    new Vector2(uv.Left, uv.Top),
                    new Vector2(uv.Right, uv.Top),
                    new Vector2(uv.Right, uv.Bottom),
                },
                _ => throw new ApplicationException($"Invalid block model texture rotation value '{uvRotation}'!")
            };
        }

        public static string GetFaceName(in ElementFaces face)
        {
            return face switch {
                ElementFaces.Up => "up",
                ElementFaces.Down => "down",
                ElementFaces.North => "north",
                ElementFaces.South => "south",
                ElementFaces.West => "west",
                ElementFaces.East => "east",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static string GetFaceName(in string elementName, in ElementFaces face)
        {
            var faceName = GetFaceName(face);
            return $"{elementName}-{faceName}";
        }

        private static (Vector2 min, Vector2 max) GetTexMinMax(in RectangleF uv, in int uvRotation)
        {
            return uvRotation switch {
                0 => (uv.TopLeft, uv.BottomRight),
                90 => (uv.BottomLeft, uv.TopRight),
                180 => (uv.BottomRight, uv.TopLeft),
                270 => (uv.TopRight, uv.BottomLeft),
                _ => throw new ApplicationException($"Invalid block model texture rotation value '{uvRotation}'!")
            };
        }

        private static void CrossProduct(in Vector3 vector1, in Vector3 vector2, out Vector3 result)
        {
            result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
        }
    }
}
