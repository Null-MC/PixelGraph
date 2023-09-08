using HelixToolkit.SharpDX.Core;
using MinecraftMappings.Internal.Models;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Models;

public interface IModelBuilder
{
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

    public BlockMeshGeometry3D ToBlockMeshGeometry3D()
    {
        return Builder.ToBlockMeshGeometry3D();
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

            GetTexMinMax(in uv, in uvRotation, out var texMin, out var texMax);
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

    private static void GetTexMinMax(in RectangleF uv, in int uvRotation, out Vector2 min, out Vector2 max)
    {
        switch (uvRotation) {
            case 0:
                min.X = uv.Left;
                min.Y = uv.Top;
                max.X = uv.Right;
                max.Y = uv.Bottom;
                break;
            case 90:
                min.X = 1f - uv.Bottom;
                min.Y = uv.Left;
                max.X = 1f - uv.Top;
                max.Y = uv.Right;
                break;
            case 180:
                min.X = 1f - uv.Right;
                min.Y = 1f - uv.Bottom;
                max.X = 1f - uv.Left;
                max.Y = 1f - uv.Top;
                break;
            case 270:
                min.X = uv.Top;
                min.Y = 1f - uv.Right;
                max.X = uv.Bottom;
                max.Y = 1f - uv.Left;
                break;
            default:
                throw new ApplicationException($"Invalid block model texture rotation value '{uvRotation}'!");
        }
    }

    private static void CrossProduct(in Vector3 vector1, in Vector3 vector2, out Vector3 result)
    {
        result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
        result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
        result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
    }

    protected static Vector3 GetFaceNormal(ElementFaces face)
    {
        switch (face) {
            case ElementFaces.Up:
                return new Vector3(0, 1, 0);
            case ElementFaces.Down:
                return new Vector3(0, -1, 0);
            case ElementFaces.North:
                return new Vector3(0, 0, -1);
            case ElementFaces.South:
                return new Vector3(0, 0, 1);
            case ElementFaces.East:
                return new Vector3(1, 0, 0);
            case ElementFaces.West:
                return new Vector3(-1, 0, 0);
            default:
                throw new ApplicationException($"Unknown element face '{face}'!");
        }
    }

    protected static Vector3 GetUpVector(ElementFaces face)
    {
        switch (face) {
            case ElementFaces.Up:
                return -Vector3.UnitZ;
            case ElementFaces.Down:
                return Vector3.UnitZ;
            case ElementFaces.North:
            case ElementFaces.South:
            case ElementFaces.East:
            case ElementFaces.West:
                return Vector3.UnitY;
            default:
                throw new ApplicationException($"Unknown element face '{face}'!");
        }
    }
}