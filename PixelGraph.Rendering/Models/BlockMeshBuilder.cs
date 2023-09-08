using HelixToolkit.SharpDX.Core;
using SharpDX;

namespace PixelGraph.Rendering.Models;

internal class BlockMeshBuilder : MeshBuilder
{
    private Vector2Collection textureCoordinateMins;
    private Vector2Collection textureCoordinateMaxs;

    public Vector2Collection TextureCoordinateMins {
        get => textureCoordinateMins;
        set => textureCoordinateMins = value;
    }

    public Vector2Collection TextureCoordinateMaxs {
        get => textureCoordinateMaxs;
        set => textureCoordinateMaxs = value;
    }


    public BlockMeshBuilder() : base(true, true, true)
    {
        textureCoordinateMins = new Vector2Collection();
        textureCoordinateMaxs = new Vector2Collection();
    }

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
        textureCoordinateMins.Add(texMin);
        textureCoordinateMins.Add(texMin);
        textureCoordinateMins.Add(texMin);
        textureCoordinateMins.Add(texMin);
    }

    public void AddTextureCoordinateMax4x(in Vector2 texMax)
    {
        textureCoordinateMaxs.Add(texMax);
        textureCoordinateMaxs.Add(texMax);
        textureCoordinateMaxs.Add(texMax);
        textureCoordinateMaxs.Add(texMax);
    }

    public BlockMeshGeometry3D ToBlockMeshGeometry3D()
    {
        if (HasTangents && Tangents.Count == 0) {
            ComputeTangents(Positions, Normals, TextureCoordinates, TriangleIndices, out var tan, out var bitan);
            Tangents.AddRange(tan);
            BiTangents.AddRange(bitan);
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
}