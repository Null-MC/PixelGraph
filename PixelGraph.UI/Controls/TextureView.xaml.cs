using PixelGraph.UI.Internal;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Controls
{
    public partial class TextureView
    {
        public ImageSource Texture {
            get => (ImageSource)GetValue(TextureProperty);
            set => SetValue(TextureProperty, value);
        }

        public double Zoom {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public string ZoomText {
            get => ZoomHelper.Format(Zoom);
            set => Zoom = ZoomHelper.Parse(value);
        }


        public TextureView()
        {
            InitializeComponent();
        }

        private void OnTextureViewCanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (!isCtrl || e.Handled) return;
            e.Handled = true;

            var value = Zoom;
            value += e.Delta * value * 0.001f;
            Zoom = Math.Clamp(value, 0.01f, 100f);
        }

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zoomText = ZoomHelper.Format((double)e.NewValue);
            d.SetCurrentValue(ZoomTextProperty, zoomText);
        }

        private static void OnZoomTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zoom = ZoomHelper.Parse((string)e.NewValue);
            d.SetCurrentValue(ZoomProperty, zoom);
        }

        public static readonly DependencyProperty TextureProperty = DependencyProperty
            .Register("Texture", typeof(ImageSource), typeof(TextureView));

        public static readonly DependencyProperty ZoomProperty = DependencyProperty
            .Register("Zoom", typeof(double), typeof(TextureView), new PropertyMetadata(1d, OnZoomChanged));

        public static readonly DependencyProperty ZoomTextProperty = DependencyProperty
            .Register("ZoomText", typeof(string), typeof(TextureView), new PropertyMetadata("100%", OnZoomTextChanged));
    }
}
