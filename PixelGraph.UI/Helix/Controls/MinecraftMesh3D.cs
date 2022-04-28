using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using MinecraftMappings.Internal;
using PixelGraph.Rendering.Minecraft;
using PixelGraph.UI.Internal.Utilities;
using SharpDX;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls
{
    public class MinecraftMesh3D : Element3D, IMinecraftSceneCore
    {
        private MinecraftMeshNode MeshNode => SceneNode as MinecraftMeshNode;
        //public bool IsRenderValid => MeshNode.IsRenderValid;
        public long LastUpdated => MeshNode.LastUpdated;

        public string BlendMode {
            get => (string)GetValue(BlendModeProperty);
            set => SetValue(BlendModeProperty, value);
        }

        public string TintColor {
            get => (string)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }

        public float ParallaxDepth {
            get => (float)GetValue(ParallaxDepthProperty);
            set => SetValue(ParallaxDepthProperty, value);
        }

        public int ParallaxSamples {
            get => (int)GetValue(ParallaxSamplesProperty);
            set => SetValue(ParallaxSamplesProperty, value);
        }

        public int WaterMode {
            get => (int)GetValue(WaterModeProperty);
            set => SetValue(WaterModeProperty, value);
        }


        public void Apply(DeviceContextProxy deviceContext)
        {
            MeshNode?.Apply(deviceContext);
        }

        //public void ResetValidation()
        //{
        //    MeshNode?.ResetValidation();
        //}

        protected override SceneNode OnCreateSceneNode()
        {
            return new MinecraftMeshNode();
        }

        protected override void AssignDefaultValuesToSceneNode(SceneNode core)
        {
            base.AssignDefaultValuesToSceneNode(core);
            if (core is not MinecraftMeshNode n) return;

            n.BlendMode = BlendModes.TryParse(BlendMode, out var value) ? value : BlendModes.Opaque;
            n.TintColor = ColorHelper.RGBFromHex(TintColor)?.ToColor4().ToVector3() ?? Vector3.One;
            n.ParallaxDepth = ParallaxDepth;
            n.ParallaxSamples = ParallaxSamples;
            n.WaterMode = WaterMode;
        }

        public static readonly DependencyProperty BlendModeProperty = DependencyProperty
            .Register(nameof(BlendMode), typeof(string), typeof(MinecraftMesh3D), new PropertyMetadata(null, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode }) {
                    var rawValue = (string)e.NewValue;
                    meshNode.BlendMode = BlendModes.TryParse(rawValue, out var value) ? value : BlendModes.Opaque;
                }
            }));

        public static readonly DependencyProperty TintColorProperty = DependencyProperty
            .Register(nameof(TintColor), typeof(string), typeof(MinecraftMesh3D), new PropertyMetadata(null, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode }) {
                    var rawValue = (string)e.NewValue;
                    meshNode.TintColor = ColorHelper.RGBFromHex(rawValue)?.ToColor4().ToVector3() ?? Vector3.One;
                }
            }));

        public static readonly DependencyProperty ParallaxDepthProperty = DependencyProperty
            .Register(nameof(ParallaxDepth), typeof(float), typeof(MinecraftMesh3D), new PropertyMetadata(0f, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode })
                    meshNode.ParallaxDepth = (float)e.NewValue;
            }));

        public static readonly DependencyProperty ParallaxSamplesProperty = DependencyProperty
            .Register(nameof(ParallaxSamples), typeof(int), typeof(MinecraftMesh3D), new PropertyMetadata(128, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode })
                    meshNode.ParallaxSamples = (int)e.NewValue;
            }));

        public static readonly DependencyProperty WaterModeProperty = DependencyProperty
            .Register(nameof(WaterMode), typeof(int), typeof(MinecraftMesh3D), new PropertyMetadata(0, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode })
                    meshNode.WaterMode = (int)e.NewValue;
            }));
    }
}
