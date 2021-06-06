using System.Windows;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;

namespace PixelGraph.UI.Internal.Preview
{
    public class PbrSpecularMeshModel3D : MeshGeometryModel3D
    {
        public RenderPreviewModes RenderMode {
            get => (RenderPreviewModes)GetValue(RenderModeProperty);
            set => SetValue(RenderModeProperty, value);
        }


        protected override SceneNode OnCreateSceneNode()
        {
            var node = base.OnCreateSceneNode();
            node.OnSetRenderTechnique = OnSetRenderTechnique;
            return node;
        }

        private IRenderTechnique OnSetRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager.GetTechnique(CustomShaderNames.PbrSpecular);

            //switch (RenderMode) {
            //    case RenderPreviewModes.PbrSpecular:
            //        var x = host.EffectsManager.GetTechnique(CustomShaderNames.PbrSpecular);
            //        return x;
            //    default:
            //        var y = host.EffectsManager.GetTechnique(DefaultRenderTechniqueNames.Mesh);
            //        return y;
            //}
        }

        public static readonly DependencyProperty RenderModeProperty =
            DependencyProperty.Register(nameof(RenderMode), typeof(RenderPreviewModes), typeof(PbrSpecularMeshModel3D), new PropertyMetadata(RenderPreviewModes.Diffuse));
    }
}
