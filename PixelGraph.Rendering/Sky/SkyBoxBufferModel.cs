using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Utilities;
using SharpDX;
using SharpDX.Direct3D;

namespace PixelGraph.Rendering.Sky;

internal class SkyBoxBufferModel : PointGeometryBufferModel<Vector3>
{
    public SkyBoxBufferModel() : base(Vector3.SizeInBytes)
    {
        Topology = PrimitiveTopology.TriangleList;
        Geometry = new PointGeometry3D {
            Positions = boxPositions,
        };
    }

    protected override void OnCreateVertexBuffer(DeviceContextProxy context, IElementsBufferProxy buffer, int bufferIndex, Geometry3D geometry, IDeviceResources deviceResources)
    {
        if (geometry?.Positions != null && geometry.Positions.Count > 0) {
            buffer.UploadDataToBuffer(context, geometry.Positions, geometry.Positions.Count);
        }
        else {
            buffer.UploadDataToBuffer(context, emptyVerts, 0);
        }
    }


    private static readonly Vector3Collection boxPositions = new() {
        new Vector3(-10.0f,  10.0f, -10.0f),
        new Vector3( -10.0f, -10.0f, -10.0f),
        new Vector3( 10.0f, -10.0f, -10.0f),
        new Vector3(  10.0f, -10.0f, -10.0f),
        new Vector3(  10.0f,  10.0f, -10.0f),
        new Vector3( -10.0f,  10.0f, -10.0f),

        new Vector3( -10.0f, -10.0f,  10.0f),
        new Vector3(-10.0f, -10.0f, -10.0f),
        new Vector3(  -10.0f,  10.0f, -10.0f),
        new Vector3(  -10.0f,  10.0f, -10.0f),
        new Vector3(  -10.0f,  10.0f,  10.0f),
        new Vector3(  -10.0f, -10.0f,  10.0f),

        new Vector3(   10.0f, -10.0f, -10.0f),
        new Vector3(   10.0f, -10.0f,  10.0f),
        new Vector3(   10.0f,  10.0f,  10.0f),
        new Vector3(   10.0f,  10.0f,  10.0f),
        new Vector3(   10.0f,  10.0f, -10.0f),
        new Vector3(   10.0f, -10.0f, -10.0f),

        new Vector3(  -10.0f, -10.0f,  10.0f),
        new Vector3(  -10.0f,  10.0f,  10.0f),
        new Vector3(   10.0f,  10.0f,  10.0f),
        new Vector3(   10.0f,  10.0f,  10.0f),
        new Vector3(   10.0f, -10.0f,  10.0f),
        new Vector3(  -10.0f, -10.0f,  10.0f),

        new Vector3(  -10.0f,  10.0f, -10.0f),
        new Vector3(   10.0f,  10.0f, -10.0f),
        new Vector3(   10.0f,  10.0f,  10.0f),
        new Vector3(   10.0f,  10.0f,  10.0f),
        new Vector3(  -10.0f,  10.0f,  10.0f),
        new Vector3(  -10.0f,  10.0f, -10.0f),

        new Vector3(  -10.0f, -10.0f, -10.0f),
        new Vector3(  -10.0f, -10.0f,  10.0f),
        new Vector3(   10.0f, -10.0f, -10.0f),
        new Vector3(   10.0f, -10.0f, -10.0f),
        new Vector3( -10.0f, -10.0f,  10.0f),
        new Vector3(   10.0f, -10.0f,  10.0f)
    };
}