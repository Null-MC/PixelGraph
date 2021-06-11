using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Preview.Sky;
using System.Windows;

namespace PixelGraph.UI.PreviewModels
{
    public class EnvironmentCube3D : Element3D, IEnvironmentCube
    {
        public IMinecraftScene Scene {
            get => (IMinecraftScene)GetValue(SceneProperty);
            set => SetValue(SceneProperty, value);
        }

        //public Vector3 SunDirection {
        //    get => (Vector3)GetValue(SunDirectionProperty);
        //    set => SetValue(SunDirectionProperty, value);
        //}

        //public float SunStrength {
        //    get => (float)GetValue(SunStrengthProperty);
        //    set => SetValue(SunStrengthProperty, value);
        //}

        //public float TimeOfDay {
        //    get => (float)GetValue(TimeOfDayProperty);
        //    set => SetValue(TimeOfDayProperty, value);
        //}

        //public float Wetness {
        //    get => (float)GetValue(WetnessProperty);
        //    set => SetValue(WetnessProperty, value);
        //}

        public int FaceSize {
            get => (int)GetValue(FaceSizeProperty);
            set => SetValue(FaceSizeProperty, value);
        }

        public ShaderResourceViewProxy CubeMap => ((EnvironmentCubeNode)SceneNode)?.CubeMap;


        protected override SceneNode OnCreateSceneNode()
        {
            return new EnvironmentCubeNode();
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            if (core is EnvironmentCubeNode n) {
                n.Scene = Scene;
                //n.SunDirection = SunDirection;
                //n.SunStrength = SunStrength;
                //n.TimeOfDay = TimeOfDay;
                //n.Wetness = Wetness;
                n.FaceSize = FaceSize;
            }

            base.AssignDefaultValuesToSceneNode(core);
        }

        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register(nameof(Scene), typeof(IMinecraftScene), typeof(EnvironmentCube3D), new PropertyMetadata(null, (d, e) => {
                if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
                    sceneNode.Scene = (IMinecraftScene)e.NewValue;
            }));

        //public static readonly DependencyProperty SunDirectionProperty =
        //    DependencyProperty.Register(nameof(SunDirection), typeof(Vector3), typeof(EnvironmentCube3D), new PropertyMetadata(Vector3.UnitY, (d, e) => {
        //        if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
        //            sceneNode.SunDirection = (Vector3)e.NewValue;
        //    }));

        //public static readonly DependencyProperty SunStrengthProperty =
        //    DependencyProperty.Register(nameof(SunStrength), typeof(float), typeof(EnvironmentCube3D), new PropertyMetadata(1f, (d, e) => {
        //        if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
        //            sceneNode.SunStrength = (float)e.NewValue;
        //    }));

        //public static readonly DependencyProperty TimeOfDayProperty =
        //    DependencyProperty.Register(nameof(TimeOfDay), typeof(float), typeof(EnvironmentCube3D), new PropertyMetadata(0f, (d, e) => {
        //        if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
        //            sceneNode.TimeOfDay = (float) e.NewValue;
        //    }));

        //public static readonly DependencyProperty WetnessProperty =
        //    DependencyProperty.Register(nameof(Wetness), typeof(float), typeof(EnvironmentCube3D), new PropertyMetadata(0f, (d, e) => {
        //        if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
        //            sceneNode.Wetness = (float)e.NewValue;
        //    }));

        public static readonly DependencyProperty FaceSizeProperty =
            DependencyProperty.Register(nameof(FaceSize), typeof(int), typeof(EnvironmentCube3D), new PropertyMetadata(256, (d, e) => {
                if (d is Element3DCore {SceneNode: EnvironmentCubeNode sceneNode})
                    sceneNode.FaceSize = (int)e.NewValue;
            }));
    }
}
