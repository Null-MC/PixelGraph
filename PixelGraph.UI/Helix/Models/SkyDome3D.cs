using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.UI.Helix.Sky;

namespace PixelGraph.UI.Helix.Models
{
    public class SkyDome3D : Element3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new SkyDomeNode();
        }
    }
}
