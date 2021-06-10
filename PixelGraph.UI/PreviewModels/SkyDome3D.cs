using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using System.Windows;

namespace PixelGraph.UI.PreviewModels
{
    public class SkyDome3D : Element3D
    {
        public TextureModel Texture {
            get => (TextureModel)GetValue(TextureProperty);
            set => SetValue(TextureProperty, value);
        }

        protected override SceneNode OnCreateSceneNode()
        {
            return new EnvironmentMapNode(true);
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            base.AssignDefaultValuesToSceneNode(core);
            (SceneNode as EnvironmentMapNode).Texture = Texture;
        }

        public static readonly DependencyProperty TextureProperty = DependencyProperty.Register("Texture", typeof(TextureModel), typeof(SkyDome3D), new PropertyMetadata(null, (d, e) => {
            ((d as Element3DCore).SceneNode as EnvironmentMapNode).Texture = (TextureModel)e.NewValue;
        }));
    }
}
