using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using MinecraftMappings.Internal;
using PixelGraph.Rendering.Minecraft;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Internal.Utilities;
using SharpDX;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls
{
    public class MinecraftMesh3D : Element3D
    {
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

        public bool EnableLinearSampling {
            get => (bool)GetValue(EnableLinearSamplingProperty);
            set => SetValue(EnableLinearSamplingProperty, value);
        }

        public bool EnableSlopeNormals {
            get => (bool)GetValue(EnableSlopeNormalsProperty);
            set => SetValue(EnableSlopeNormalsProperty, value);
        }

        public int WaterMode {
            get => (int)GetValue(WaterModeProperty);
            set => SetValue(WaterModeProperty, value);
        }

        public float SubSurfaceBlur {
            get => (float)GetValue(SubSurfaceBlurProperty);
            set => SetValue(SubSurfaceBlurProperty, value);
        }


        //public void Apply(DeviceContextProxy deviceContext)
        //{
        //    MeshNode?.Apply(deviceContext);
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
            n.EnableLinearSampling = EnableLinearSampling;
            n.EnableSlopeNormals = EnableSlopeNormals;
            n.WaterMode = WaterMode;
            n.SubSurfaceBlur = SubSurfaceBlur;
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

        public static readonly DependencyProperty EnableLinearSamplingProperty = DependencyProperty
            .Register(nameof(EnableLinearSampling), typeof(bool), typeof(MinecraftMesh3D), new PropertyMetadata(false, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftMeshNode sceneNode})
                    sceneNode.EnableLinearSampling = (bool)e.NewValue;
            }));

        public static readonly DependencyProperty EnableSlopeNormalsProperty = DependencyProperty
            .Register(nameof(EnableSlopeNormals), typeof(bool), typeof(MinecraftMesh3D), new PropertyMetadata(false, (d, e) => {
                if (d is Element3DCore {SceneNode: MinecraftMeshNode sceneNode})
                    sceneNode.EnableSlopeNormals = (bool)e.NewValue;
            }));

        public static readonly DependencyProperty WaterModeProperty = DependencyProperty
            .Register(nameof(WaterMode), typeof(int), typeof(MinecraftMesh3D), new PropertyMetadata(0, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode })
                    meshNode.WaterMode = (int)e.NewValue;
            }));

        public static readonly DependencyProperty SubSurfaceBlurProperty = DependencyProperty
            .Register(nameof(SubSurfaceBlur), typeof(float), typeof(MinecraftMesh3D), new PropertyMetadata((float)RenderPreviewSettings.Default_SubSurfaceBlur, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode })
                    meshNode.SubSurfaceBlur = (float)e.NewValue;
            }));
    }
}
