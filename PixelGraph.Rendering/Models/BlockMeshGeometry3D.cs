using HelixToolkit.SharpDX.Core;
using System.Runtime.Serialization;

namespace PixelGraph.Rendering.Models;

public class BlockMeshGeometry3D : MeshGeometry3D
{
    private Vector2Collection? textureCoordinateMins = [];
    private Vector2Collection? textureCoordinateMaxs = [];

    [DataMember]
    public Vector2Collection? TextureCoordinateMins {
        get => textureCoordinateMins;
        set => Set(ref textureCoordinateMins, value);
    }

    [DataMember]
    public Vector2Collection? TextureCoordinateMaxs {
        get => textureCoordinateMaxs;
        set => Set(ref textureCoordinateMaxs, value);
    }


    protected override void OnAssignTo(Geometry3D target)
    {
        base.OnAssignTo(target);

        if (target is BlockMeshGeometry3D mesh) {
            mesh.TextureCoordinateMins = TextureCoordinateMins;
            mesh.TextureCoordinateMaxs = TextureCoordinateMaxs;
        }
    }

    protected override void OnClearAllGeometryData()
    {
        base.OnClearAllGeometryData();

        TextureCoordinateMins?.Clear();
        TextureCoordinateMins?.TrimExcess();

        TextureCoordinateMaxs?.Clear();
        TextureCoordinateMaxs?.TrimExcess();
    }

    //public void Freeze()
    //{
    //    throw new System.NotImplementedException();
    //}
}
