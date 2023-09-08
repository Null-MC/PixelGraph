using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.Rendering.Sky;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls;

public class EquirectangularSkyDome3D : Element3D
{
    public TextureModel Texture {
        get => (TextureModel)GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }


    protected override SceneNode OnCreateSceneNode()
    {
        return new EquirectangularSkyDomeNode();
    }

    protected override void AssignDefaultValuesToSceneNode(SceneNode core)
    {
        base.AssignDefaultValuesToSceneNode(core);
        if (SceneNode is not EquirectangularSkyDomeNode node) return;
            
        node.Texture = Texture;
    }

    public static readonly DependencyProperty TextureProperty = DependencyProperty
        .Register(nameof(Texture), typeof(TextureModel), typeof(EquirectangularSkyDome3D), new PropertyMetadata(null, (d, e) => {
            if (d is Element3DCore {SceneNode: EquirectangularSkyDomeNode node})
                node.Texture = (TextureModel)e.NewValue;
        }));
}