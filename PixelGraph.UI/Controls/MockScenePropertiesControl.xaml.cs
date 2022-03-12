using PixelGraph.UI.Models.MockScene;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class ScenePropertiesControl
    {
        public MockScenePropertiesModel SceneProperties {
            get => (MockScenePropertiesModel)GetValue(ScenePropertiesProperty);
            set => SetValue(ScenePropertiesProperty, value);
        }


        public ScenePropertiesControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScenePropertiesProperty = DependencyProperty
            .Register(nameof(SceneProperties), typeof(MockScenePropertiesModel), typeof(ScenePropertiesControl));
    }
}
