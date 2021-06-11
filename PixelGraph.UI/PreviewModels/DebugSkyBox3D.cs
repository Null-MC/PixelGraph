using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.UI.Internal.Preview.Sky;
using System.Windows;

namespace PixelGraph.UI.PreviewModels
{
    public class DebugSkyBox3D : Element3D
    {
        public IEnvironmentCube EnvironmentCube {
            get => (IEnvironmentCube)GetValue(EnvironmentCubeProperty);
            set => SetValue(EnvironmentCubeProperty, value);
        }

        protected override SceneNode OnCreateSceneNode()
        {
            return new DebugSkyBoxNode();
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            base.AssignDefaultValuesToSceneNode(core);

            if (SceneNode is DebugSkyBoxNode skyBoxNode)
                skyBoxNode.EnvironmentCube = EnvironmentCube;
        }

        public static readonly DependencyProperty EnvironmentCubeProperty = DependencyProperty.Register(nameof(EnvironmentCube), typeof(IEnvironmentCube), typeof(DebugSkyBox3D), new PropertyMetadata(null, (d, e) => {
            if (d is Element3DCore {SceneNode: DebugSkyBoxNode skyBoxNode})
                skyBoxNode.EnvironmentCube = (IEnvironmentCube)e.NewValue;
        }));
    }
}
