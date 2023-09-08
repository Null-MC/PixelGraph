using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Rendering.Sky;

namespace PixelGraph.UI.Helix.Controls;

public class DynamicSkyDome3D : Element3D
{
    protected override SceneNode OnCreateSceneNode()
    {
        return new DynamicSkyDomeNode();
    }
}