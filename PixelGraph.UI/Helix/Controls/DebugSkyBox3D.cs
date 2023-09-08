using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.Sky;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls;

public class DebugSkyBox3D : Element3D
{
    public ICubeMapSource CubeMapSource {
        get => (ICubeMapSource)GetValue(CubeMapSourceProperty);
        set => SetValue(CubeMapSourceProperty, value);
    }

    protected override SceneNode OnCreateSceneNode()
    {
        return new DebugSkyBoxNode();
    }

    protected override void AssignDefaultValuesToSceneNode(SceneNode core)
    {
        base.AssignDefaultValuesToSceneNode(core);

        if (SceneNode is DebugSkyBoxNode skyBoxNode)
            skyBoxNode.CubeMapSource = CubeMapSource;
    }

    public static readonly DependencyProperty CubeMapSourceProperty = DependencyProperty.Register(nameof(CubeMapSource), typeof(ICubeMapSource), typeof(DebugSkyBox3D), new PropertyMetadata(null, (d, e) => {
        if (d is Element3DCore {SceneNode: DebugSkyBoxNode skyBoxNode})
            skyBoxNode.CubeMapSource = (ICubeMapSource)e.NewValue;
    }));
}