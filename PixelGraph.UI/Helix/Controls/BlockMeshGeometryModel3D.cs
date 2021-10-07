using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;

namespace PixelGraph.UI.Helix.Controls
{
    public class BlockMeshGeometryModel3D : MeshGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new BlockMeshNode();
        }
    }
}
