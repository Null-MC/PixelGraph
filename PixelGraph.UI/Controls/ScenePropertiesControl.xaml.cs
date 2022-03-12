using PixelGraph.UI.Models.Scene;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class ScenePropertiesControl
    {
        public ScenePropertiesModel SceneProperties {
            get => (ScenePropertiesModel)GetValue(ScenePropertiesProperty);
            set => SetValue(ScenePropertiesProperty, value);
        }


        public ScenePropertiesControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScenePropertiesProperty = DependencyProperty
            .Register(nameof(SceneProperties), typeof(ScenePropertiesModel), typeof(ScenePropertiesControl));
    }
}
