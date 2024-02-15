using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using PixelGraph.Rendering.Models;

namespace PixelGraph.Rendering.BlockMesh;

public class BlockMeshNode : MeshNode
{
    protected override RenderCore OnCreateRenderCore()
    {
        return new BlockMeshRenderCore();
    }

    protected override IAttachableBufferModel OnCreateBufferModel(Guid modelGuid, Geometry3D geometry)
    {
        if (geometry != null && geometry.IsDynamic)
            throw new NotImplementedException("Dynamic block meshes not currently supported!");

        return EffectsManager.GeometryBufferManager.Register<BlockMeshGeometryBufferModel>(modelGuid, geometry);
    }
}