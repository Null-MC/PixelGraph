using System;
using System.Windows;
using System.Windows.Input;
using SixLabors.ImageSharp;

namespace PixelGraph.UI.Controls
{
    public partial class TexturePreviewControl
    {
        public event EventHandler RefreshClick;

        public bool ShowOutline {
            set => SetValue(ShowOutlineProperty, value);
        }

        public RectangleF? OutlineBounds {
            set => SetValue(OutlineBoundsProperty, value);
        }


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

        private static void OnShowOutlinePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TexturePreviewControl control) return;
            control.Model.ShowOutline = (bool)e.NewValue;
        }

        private static void OnOutlineBoundsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TexturePreviewControl control) return;
            control.Model.OutlineBounds = (RectangleF?)e.NewValue;
        }

        public static readonly DependencyProperty ShowOutlineProperty = DependencyProperty
            .Register(nameof(ShowOutline), typeof(bool), typeof(TexturePreviewControl), new PropertyMetadata(OnShowOutlinePropertyChanged));

        public static readonly DependencyProperty OutlineBoundsProperty = DependencyProperty
            .Register(nameof(OutlineBounds), typeof(RectangleF?), typeof(TexturePreviewControl), new PropertyMetadata(OnOutlineBoundsPropertyChanged));
    }
}
