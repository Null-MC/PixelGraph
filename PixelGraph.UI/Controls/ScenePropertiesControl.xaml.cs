using PixelGraph.UI.Models.Scene;
using System.Windows;

namespace PixelGraph.UI.Controls
{
    public partial class ScenePropertiesControl
    {
        public ScenePropertiesModel Model {
            get => (ScenePropertiesModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }


        public ScenePropertiesControl()
        {
            InitializeComponent();
        }

        //public void Initialize()
        //{
        //    Sun.Enabled = true;
        //    Sun.Time = ;
        //    Sun.Tilt = ;
        //    Sun.Azimuth = ;
        //    SunProperties.SetData(Sun);
        //}

        public static readonly DependencyProperty ModelProperty = DependencyProperty
            .Register(nameof(Model), typeof(ScenePropertiesModel), typeof(ScenePropertiesControl));
    }
}
