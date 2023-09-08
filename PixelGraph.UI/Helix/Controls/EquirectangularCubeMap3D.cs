using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.Rendering.CubeMaps;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls;

public class EquirectangularCubeMap3D : Element3D, ICubeMapSource
{
    public int FaceSize {
        get => (int)GetValue(FaceSizeProperty);
        set => SetValue(FaceSizeProperty, value);
    }

    public TextureModel Texture {
        get => (TextureModel)GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }

    /// <remarks>
    /// This is NOT a valid property!
    /// only used for notification of exposure changes
    /// </remarks>
    public float Exposure {
        get => (float)GetValue(ExposureProperty);
        set => SetValue(ExposureProperty, value);
    }

    public ShaderResourceViewProxy CubeMap => (SceneNode as EquirectangularCubeMapNode)?.CubeMap;
    public long LastUpdated => (SceneNode as EquirectangularCubeMapNode)?.LastUpdated ?? 0;


    protected override SceneNode OnCreateSceneNode()
    {
        return new EquirectangularCubeMapNode();
    }

    protected override void AssignDefaultValuesToSceneNode(SceneNode core)
    {
        if (core is EquirectangularCubeMapNode n) {
            n.FaceSize = FaceSize;
            n.Texture = Texture;
        }

        base.AssignDefaultValuesToSceneNode(core);
    }

    public static readonly DependencyProperty FaceSizeProperty =
        DependencyProperty.Register(nameof(FaceSize), typeof(int), typeof(EquirectangularCubeMap3D), new PropertyMetadata(256, (d, e) => {
            if (d is Element3DCore {SceneNode: EquirectangularCubeMapNode sceneNode})
                sceneNode.FaceSize = (int)e.NewValue;
        }));

    public static readonly DependencyProperty TextureProperty =
        DependencyProperty.Register(nameof(Texture), typeof(TextureModel), typeof(EquirectangularCubeMap3D), new PropertyMetadata(null, (d, e) => {
            if (d is Element3DCore {SceneNode: EquirectangularCubeMapNode sceneNode})
                sceneNode.Texture = (TextureModel)e.NewValue;
        }));

    public static readonly DependencyProperty ExposureProperty =
        DependencyProperty.Register(nameof(Exposure), typeof(float), typeof(EquirectangularCubeMap3D), new PropertyMetadata(1f, (d, e) => {
            if (d is Element3DCore {SceneNode: EquirectangularCubeMapNode sceneNode})
                sceneNode.Exposure = (float)e.NewValue;
        }));
}