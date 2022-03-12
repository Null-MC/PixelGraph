using PixelGraph.UI.Models.MockScene;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class RenderPreviewControl
    {
        public MockScenePropertiesModel SceneProperties {
            get => (MockScenePropertiesModel)GetValue(ScenePropertiesProperty);
            set => SetValue(ScenePropertiesProperty, value);
        }

        public MockRenderPropertiesModel RenderProperties {
            get => (MockRenderPropertiesModel)GetValue(RenderPropertiesProperty);
            set => SetValue(RenderPropertiesProperty, value);
        }


        public RenderPreviewControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScenePropertiesProperty = DependencyProperty
            .Register(nameof(SceneProperties), typeof(MockScenePropertiesModel), typeof(RenderPreviewControl));

        public static readonly DependencyProperty RenderPropertiesProperty = DependencyProperty
            .Register(nameof(RenderProperties), typeof(MockRenderPropertiesModel), typeof(RenderPreviewControl));
    }
}
