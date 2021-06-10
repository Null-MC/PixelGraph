using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.UI.Internal.Preview;
using SharpDX;
using System.Windows;

namespace PixelGraph.UI.PreviewModels
{
    public class MinecraftScene3D : Element3D
    {
        public float TimeOfDay {
            get => (float)GetValue(TimeOfDayProperty);
            set => SetValue(TimeOfDayProperty, value);
        }

        public Vector3 SunDirection {
            get => (Vector3)GetValue(SunDirectionProperty);
            set => SetValue(SunDirectionProperty, value);
        }

        public float Wetness {
            get => (float)GetValue(WetnessProperty);
            set => SetValue(WetnessProperty, value);
        }

        public float ParallaxDepth {
            get => (float)GetValue(ParallaxDepthProperty);
            set => SetValue(ParallaxDepthProperty, value);
        }

        public int ParallaxSamplesMin {
            get => (int)GetValue(ParallaxSamplesMinProperty);
            set => SetValue(ParallaxSamplesMinProperty, value);
        }

        public int ParallaxSamplesMax {
            get => (int)GetValue(ParallaxSamplesMaxProperty);
            set => SetValue(ParallaxSamplesMaxProperty, value);
        }


        protected override SceneNode OnCreateSceneNode()
        {
            return new MinecraftSceneNode();
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            if (core is MinecraftSceneNode n) {
                n.TimeOfDay = TimeOfDay;
                n.Wetness = Wetness;
                n.ParallaxDepth = Wetness;
                n.ParallaxSamplesMin = ParallaxSamplesMin;
                n.ParallaxSamplesMax = ParallaxSamplesMax;
            }

            base.AssignDefaultValuesToSceneNode(core);
        }

        public static readonly DependencyProperty TimeOfDayProperty =
            DependencyProperty.Register(nameof(TimeOfDay), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(0f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.TimeOfDay = (float) e.NewValue;
            }));

        public static readonly DependencyProperty SunDirectionProperty =
            DependencyProperty.Register(nameof(SunDirection), typeof(Vector3), typeof(MinecraftScene3D), new PropertyMetadata(Vector3.UnitY, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.SunDirection = (Vector3) e.NewValue;
            }));

        public static readonly DependencyProperty WetnessProperty =
            DependencyProperty.Register(nameof(Wetness), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(0f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.Wetness = (float)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxDepthProperty =
            DependencyProperty.Register(nameof(ParallaxDepth), typeof(float), typeof(MinecraftScene3D), new PropertyMetadata(0f, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.ParallaxDepth = (float)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxSamplesMinProperty =
            DependencyProperty.Register(nameof(ParallaxSamplesMin), typeof(int), typeof(MinecraftScene3D), new PropertyMetadata(0, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.ParallaxSamplesMin = (int)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxSamplesMaxProperty =
            DependencyProperty.Register(nameof(ParallaxSamplesMax), typeof(int), typeof(MinecraftScene3D), new PropertyMetadata(0, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftSceneNode sceneNode})
                    sceneNode.ParallaxSamplesMax = (int)e.NewValue;
            }));
    }
}
