using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;

namespace PixelGraph.UI.Internal.Preview.Models
{
    internal class BlockMeshNode : MeshNode
    {
        protected override RenderCore OnCreateRenderCore()
        {
            return new BlockMeshRenderCore();
        }
    }
}
