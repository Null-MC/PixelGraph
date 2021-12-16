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
    public class MinecraftMesh3D : Element3D, IMinecraftScene
    {
        private MinecraftMeshNode MeshNode => SceneNode as MinecraftMeshNode;
        public bool IsRenderValid => MeshNode.IsRenderValid;

        public string BlendMode {
            get => (string)GetValue(BlendModeProperty);
            set => SetValue(BlendModeProperty, value);
        }

        public string TintColor {
            get => (string)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }


        public void Apply(DeviceContextProxy deviceContext)
        {
            MeshNode?.Apply(deviceContext);
        }

        public void ResetValidation()
        {
            MeshNode?.ResetValidation();
        }

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
        }

        public static readonly DependencyProperty BlendModeProperty =
            DependencyProperty.Register(nameof(BlendMode), typeof(string), typeof(MinecraftMesh3D), new PropertyMetadata(null, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode }) {
                    var rawValue = (string)e.NewValue;
                    meshNode.BlendMode = BlendModes.TryParse(rawValue, out var value) ? value : BlendModes.Opaque;
                }
            }));

        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.Register(nameof(TintColor), typeof(string), typeof(MinecraftMesh3D), new PropertyMetadata(null, (d, e) => {
                if (d is Element3DCore { SceneNode: MinecraftMeshNode meshNode }) {
                    var rawValue = (string)e.NewValue;
                    meshNode.TintColor = ColorHelper.RGBFromHex(rawValue)?.ToColor4().ToVector3() ?? Vector3.One;
                }
            }));
    }
}
