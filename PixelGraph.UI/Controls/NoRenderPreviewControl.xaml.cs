using PixelGraph.UI.Models.Scene;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class RenderPreviewControl
    {
        public ScenePropertiesModel SceneModel {
            get => (ScenePropertiesModel)GetValue(SceneModelProperty);
            set => SetValue(SceneModelProperty, value);
        }


        public RenderPreviewControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SceneModelProperty = DependencyProperty
            .Register(nameof(SceneModel), typeof(ScenePropertiesModel), typeof(RenderPreviewControl));
    }
}
