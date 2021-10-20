using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Utilities;
using SharpDX;
using SharpDX.Direct3D;

namespace PixelGraph.Rendering.Sky
{
    internal class SkyDomeBufferModel : MeshGeometryBufferModel<Vector3>
    {
        private static readonly MeshGeometry3D sphereMesh;


        static SkyDomeBufferModel()
        {
            var builder = new MeshBuilder(false, false);
            builder.AddSphere(Vector3.Zero, 1);
            sphereMesh = builder.ToMesh();
        }

        public SkyDomeBufferModel() : base(Vector3.SizeInBytes)
        {
            Topology = PrimitiveTopology.TriangleList;
            Geometry = sphereMesh;
        }

        protected override void OnCreateVertexBuffer(DeviceContextProxy context, IElementsBufferProxy buffer, int bufferIndex, Geometry3D geometry, IDeviceResources deviceResources)
        {
            if (bufferIndex != 0 || geometry?.Positions == null || geometry.Positions.Count <= 0) return;

            buffer.UploadDataToBuffer(context, geometry.Positions, geometry.Positions.Count);
        }
    }
}
