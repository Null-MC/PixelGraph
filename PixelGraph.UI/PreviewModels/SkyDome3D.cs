using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.UI.Internal.Preview.Sky;

namespace PixelGraph.UI.PreviewModels
{
    public class SkyDome3D : Element3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new SkyDomeNode();
        }
    }
}
