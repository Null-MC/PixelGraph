using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;

namespace PixelGraph.UI.Internal.Preview.Models
{
    public class BlockMeshGeometryModel3D : MeshGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new BlockMeshNode();
        }
    }
}
