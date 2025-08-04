using HelixToolkit.SharpDX.Core;
using SharpDX;

namespace PixelGraph.Rendering.Models;

internal class BlockMeshBuilder() : MeshBuilder(true, true, true)
{
    private Vector2Collection TextureCoordinateMins {get; set;} = [];
    private Vector2Collection TextureCoordinateMaxs {get; set;} = [];


    public void Clear()
    {
        Positions.Clear();
        TriangleIndices.Clear();
        Normals.Clear();
        Tangents.Clear();
        BiTangents.Clear();
        TextureCoordinates.Clear();
        TextureCoordinateMins.Clear();
        TextureCoordinateMaxs.Clear();
    }

    public void AddNormal4x(in Vector3 normal)
    {
        Normals.Add(normal);
        Normals.Add(normal);
        Normals.Add(normal);
        Normals.Add(normal);
    }

    public void AddTextureCoordinateMin4x(in Vector2 texMin)
    {
        TextureCoordinateMins.Add(texMin);
        TextureCoordinateMins.Add(texMin);
        TextureCoordinateMins.Add(texMin);
        TextureCoordinateMins.Add(texMin);
    }

    public void AddTextureCoordinateMax4x(in Vector2 texMax)
    {
        TextureCoordinateMaxs.Add(texMax);
        TextureCoordinateMaxs.Add(texMax);
        TextureCoordinateMaxs.Add(texMax);
        TextureCoordinateMaxs.Add(texMax);
    }

    public BlockMeshGeometry3D ToBlockMeshGeometry3D()
    {
        if (HasTangents) {
            ComputeTangents();
        }

        return new BlockMeshGeometry3D {
            Positions = Positions,
            Indices = TriangleIndices,
            Normals = HasNormals ? Normals : null,
            TextureCoordinates = HasTexCoords ? TextureCoordinates : null,
            TextureCoordinateMins = HasTexCoords ? TextureCoordinateMins : null,
            TextureCoordinateMaxs = HasTexCoords ? TextureCoordinateMaxs : null,
            Tangents = HasTangents ? Tangents : null,
            BiTangents = HasTangents ? BiTangents : null,
        };
    }

    private void ComputeTangents()
    {
        var tan1 = new Vector3[Positions.Count];
        var tan2 = new Vector3[Positions.Count];

        for (var t = 0; t < TriangleIndices.Count; t += 3) {
            var i1 = TriangleIndices[t];
            var i2 = TriangleIndices[t + 1];
            var i3 = TriangleIndices[t + 2];

            var v1 = Positions[i1];
            var v2 = Positions[i2];
            var v3 = Positions[i3];

            var w1 = TextureCoordinates[i1];
            var w2 = TextureCoordinates[i2];
            var w3 = TextureCoordinates[i3];

            var x1 = v2.X - v1.X;
            var x2 = v3.X - v1.X;
            var y1 = v2.Y - v1.Y;
            var y2 = v3.Y - v1.Y;
            var z1 = v2.Z - v1.Z;
            var z2 = v3.Z - v1.Z;
            var s1 = w2.X - w1.X;
            var s2 = w3.X - w1.X;
            var t1 = w2.Y - w1.Y;
            var t2 = w3.Y - w1.Y;

            var r = 1.0f / (s1 * t2 - s2 * t1);
            
            var sdir = r * new Vector3(
                t2 * x1 - t1 * x2,
                t2 * y1 - t1 * y2,
                t2 * z1 - t1 * z2);

            var tdir = r * new Vector3(
                s1 * x2 - s2 * x1,
                s1 * y2 - s2 * y1,
                s1 * z2 - s2 * z1);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }

        Tangents = new Vector3Collection(Positions.Count);
        BiTangents = new Vector3Collection(Positions.Count);

        for (var i = 0; i < Positions.Count; i++) {
            var n = Normals[i];
            var t = tan1[i];
            t -= n * Vector3.Dot(n, t);
            t.Normalize();

            var b = tan2[i];//Vector3.Cross(n, t);

            Tangents.Add(t);
            BiTangents.Add(b);
        }
    }
}
