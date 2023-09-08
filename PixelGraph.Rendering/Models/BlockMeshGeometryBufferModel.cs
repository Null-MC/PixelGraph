using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace PixelGraph.Rendering.Models;

public class BlockMeshGeometryBufferModel : MeshGeometryBufferModel<BlockMeshVertex>
{
    //private static readonly Vector2[] emptyTextureArray = Array.Empty<Vector2>();
    //private static readonly Vector4[] emptyColorArray = Array.Empty<Vector4>();


    public BlockMeshGeometryBufferModel() : base(PrimitiveTopology.TriangleList, new IElementsBufferProxy[] {
        new ImmutableBufferProxy(BlockMeshVertex.SizeInBytes, BindFlags.VertexBuffer),
    }) {}

    //public BlockMeshGeometryBufferModel(int structSize, bool dynamic = false) : base(structSize, dynamic) {}
    //public BlockMeshGeometryBufferModel(int structSize, PrimitiveTopology topology, bool dynamic = false) : base(structSize, topology, dynamic) {}
    //public BlockMeshGeometryBufferModel(PrimitiveTopology topology, IElementsBufferProxy[] vertexBuffers, bool dynamic = false) : base(topology, vertexBuffers, dynamic) {}
    //public BlockMeshGeometryBufferModel(PrimitiveTopology topology, IElementsBufferProxy vertexBuffer, IElementsBufferProxy indexBuffer) : base(topology, vertexBuffer, indexBuffer) {}
    //public BlockMeshGeometryBufferModel(PrimitiveTopology topology, IElementsBufferProxy[] vertexBuffer, IElementsBufferProxy indexBuffer) : base(topology, vertexBuffer, indexBuffer) {}

    protected override void OnCreateVertexBuffer(DeviceContextProxy context, IElementsBufferProxy buffer, int bufferIndex, Geometry3D geometry, IDeviceResources deviceResources)
    {
        if (geometry is not BlockMeshGeometry3D mesh) return;

        switch (bufferIndex) {
            case 0:
                // -- set geometry if given
                if (geometry.Positions != null && geometry.Positions.Count > 0) {
                    // --- get geometry
                    var data = BuildVertexArray(mesh);
                    buffer.UploadDataToBuffer(context, data, geometry.Positions.Count, 0, geometry.PreDefinedVertexCount);
                }
                else {
                    //buffer.DisposeAndClear();
                    buffer.UploadDataToBuffer(context, emptyVerts, 0);
                }
                break;
            //case 1:
            //    if(mesh.TextureCoordinates != null && mesh.TextureCoordinates.Count > 0) {
            //        var data = BuildTexcoordVertexArray(mesh);
            //        buffer.UploadDataToBuffer(context, mesh.TextureCoordinates, mesh.TextureCoordinates.Count, 0, geometry.PreDefinedVertexCount);
            //    }
            //    else {
            //        buffer.UploadDataToBuffer(context, emptyTextureArray, 0);
            //    }
            //    break;
            //case 2:
            //    if (geometry.Colors != null && geometry.Colors.Count > 0) {
            //        buffer.UploadDataToBuffer(context, geometry.Colors, geometry.Colors.Count, 0, geometry.PreDefinedVertexCount);
            //    }
            //    else {
            //        buffer.UploadDataToBuffer(context, emptyColorArray, 0);
            //    }
            //    break;
        }
    }

    private BlockMeshVertex[] BuildVertexArray(BlockMeshGeometry3D geometry)
    {
        //var geometry = this.geometryInternal as MeshGeometry3D;
        var vertexCount = geometry.Positions.Count;
        using var positions = geometry.Positions.GetEnumerator();
        using var normals = geometry.Normals.GetEnumerator();
        using var tangents = geometry.Tangents.GetEnumerator();
        using var bitangents = geometry.BiTangents.GetEnumerator();
        using var texcoords = geometry.TextureCoordinates.GetEnumerator();
        using var texcoordMins = geometry.TextureCoordinateMins.GetEnumerator();
        using var texcoordMaxs = geometry.TextureCoordinateMaxs.GetEnumerator();

        var array = ThreadBufferManager<BlockMeshVertex>.GetBuffer(vertexCount);

        for (var i = 0; i < vertexCount; i++) {
            positions.MoveNext();
            normals.MoveNext();
            tangents.MoveNext();
            bitangents.MoveNext();
            texcoords.MoveNext();
            texcoordMins.MoveNext();
            texcoordMaxs.MoveNext();

            array[i].Position = new Vector4(positions.Current, 1f);
            array[i].Normal = normals.Current;
            array[i].Tangent = tangents.Current;
            array[i].BiTangent = bitangents.Current;
            array[i].TexCoord = texcoords.Current;
            array[i].TexCoordMin = texcoordMins.Current;
            array[i].TexCoordMax = texcoordMaxs.Current;
        }

        return array;
    }
}