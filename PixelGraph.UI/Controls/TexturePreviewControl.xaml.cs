using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class TexturePreviewControl
    {
        public event EventHandler RefreshClick;

        //public BitmapSource Texture {
        //    get => (BitmapSource)GetValue(TextureProperty);
        //    set => SetValue(TextureProperty, value);
        //}


        public TexturePreviewControl()
        {
            InitializeComponent();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (!isCtrl || e.Handled) return;
            e.Handled = true;

            var value = Model.Zoom;
            value += e.Delta * value * 0.001f;
            Model.Zoom = Math.Clamp(value, 0.01f, 100f);
        }

        private void OnPreviewRefreshClick(object sender, RoutedEventArgs e)
        {
            OnRefreshClick();
        }

        private void OnRefreshClick()
        {
            RefreshClick?.Invoke(this, EventArgs.Empty);
        }

        //private static void OnTextureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (sender is not TexturePreviewControl control) return;

        //    control.Model.Texture = e.NewValue as BitmapSource;
        //}

        //public static readonly DependencyProperty TextureProperty = DependencyProperty
        //    .Register(nameof(Texture), typeof(BitmapSource), typeof(TexturePreviewControl), new PropertyMetadata(null, OnTextureChanged));
    }
}
