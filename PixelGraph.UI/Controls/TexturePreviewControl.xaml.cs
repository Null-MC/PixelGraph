using PixelGraph.UI.Internal.Preview;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Controls
{
    public partial class TexturePreviewControl
    {
        public event EventHandler RefreshClick;

        public ITexturePreviewModel Model => model;

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

        private void UpdateMouseColor(System.Windows.Point pos)
        {
            var rect = new Int32Rect((int)pos.X, (int)pos.Y, 1, 1);
            var bytesPerPixel = (model.Texture.Format.BitsPerPixel + 7) / 8;

            var bytes = new byte[bytesPerPixel];
            model.Texture.CopyPixels(rect, bytes, bytesPerPixel, 0);

            if (model.Texture is ImageSharpSource imageSharpTex) {
                if (imageSharpTex.SourceFormat == typeof(Rgb24)) {
                    var hex = ColorHelper.ToHexRGB(bytes[2], bytes[1], bytes[0]);
                    model.MousePixel = $"C={hex}";
                }
                else if (imageSharpTex.SourceFormat == typeof(L8)) {
                    model.MousePixel = $"V={bytes[0]}";
                }
                else {
                    model.MousePixel = null;
                }
            }
            else {
                if (model.Texture.Format == PixelFormats.Bgra32 || model.Texture.Format == PixelFormats.Bgr32) {
                    var hex = ColorHelper.ToHexRGB(bytes[2], bytes[1], bytes[0]);
                    model.MousePixel = $"C={hex}";
                }
                else if (model.Texture.Format == PixelFormats.Gray8 || model.Texture.Format == PixelFormats.Gray16) {
                    model.MousePixel = $"V={bytes[0]}";
                }
                else {
                    model.MousePixel = null;
                }
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (!isCtrl || e.Handled) return;
            e.Handled = true;

            var value = model.Zoom;
            value += e.Delta * value * 0.001f;
            model.Zoom = Math.Clamp(value, 0.01f, 100f);
        }

        private void OnPreviewRefreshClick(object sender, RoutedEventArgs e)
        {
            OnRefreshClick();
        }

        private void OnRefreshClick()
        {
            RefreshClick?.Invoke(this, EventArgs.Empty);
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (model.Texture == null) {
                model.MousePixel = null;
                return;
            }

            try {
                var pos = e.GetPosition(img);
                UpdateMouseColor(pos);
            }
            catch (Exception error) {
                Log.Error(error, "Failed to get pixel value at mouse position!");
                model.MousePixel = null;
            }
        }

        private void OnImageMouseLeave(object sender, MouseEventArgs e)
        {
            model.MousePixel = null;
        }

        private static void OnShowOutlinePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TexturePreviewControl control) return;
            control.model.ShowOutline = (bool)e.NewValue;
        }

        private static void OnOutlineBoundsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TexturePreviewControl control) return;
            control.model.OutlineBounds = (RectangleF?)e.NewValue;
        }

        public static readonly DependencyProperty ShowOutlineProperty = DependencyProperty
            .Register(nameof(ShowOutline), typeof(bool), typeof(TexturePreviewControl), new PropertyMetadata(OnShowOutlinePropertyChanged));

        public static readonly DependencyProperty OutlineBoundsProperty = DependencyProperty
            .Register(nameof(OutlineBounds), typeof(RectangleF?), typeof(TexturePreviewControl), new PropertyMetadata(OnOutlineBoundsPropertyChanged));
    }
}
