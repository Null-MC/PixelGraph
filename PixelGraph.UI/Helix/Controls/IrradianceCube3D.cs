using System.Windows;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.Rendering.CubeMaps;

namespace PixelGraph.UI.Helix.Controls
{
    public class IrradianceCube3D : Element3D, ICubeMapSource
    {
        public ICubeMapSource EnvironmentCubeMapSource {
            get => (ICubeMapSource)GetValue(EnvironmentCubeMapSourceProperty);
            set => SetValue(EnvironmentCubeMapSourceProperty, value);
        }

        public int FaceSize {
            get => (int)GetValue(FaceSizeProperty);
            set => SetValue(FaceSizeProperty, value);
        }

        public ShaderResourceViewProxy CubeMap => ((IrradianceCubeNode)SceneNode)?.CubeMap;
        public long LastUpdated => ((IrradianceCubeNode)SceneNode)?.LastUpdated ?? 0;


        protected override SceneNode OnCreateSceneNode()
        {
            return new IrradianceCubeNode();
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            if (core is IrradianceCubeNode n) {
                n.EnvironmentCubeMapSource = EnvironmentCubeMapSource;
                n.FaceSize = FaceSize;
            }

            base.AssignDefaultValuesToSceneNode(core);
        }

        public static readonly DependencyProperty EnvironmentCubeMapSourceProperty =
            DependencyProperty.Register(nameof(EnvironmentCubeMapSource), typeof(ICubeMapSource), typeof(IrradianceCube3D), new PropertyMetadata(null, (d, e) => {
                if (d is Element3DCore {SceneNode: IrradianceCubeNode node})
                    node.EnvironmentCubeMapSource = (ICubeMapSource)e.NewValue;
            }));

        public static readonly DependencyProperty FaceSizeProperty =
            DependencyProperty.Register(nameof(FaceSize), typeof(int), typeof(IrradianceCube3D), new PropertyMetadata(128, (d, e) => {
                if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
                    sceneNode.FaceSize = (int)e.NewValue;
            }));
    }
}
