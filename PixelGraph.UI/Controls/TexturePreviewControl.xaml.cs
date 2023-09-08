using PixelGraph.UI.Models;
using Serilog;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Controls;

public partial class TexturePreviewControl
{
    public event EventHandler RefreshClick;

    public ITexturePreviewModel TexturePreviewModel {
        get => (ITexturePreviewModel)GetValue(TexturePreviewModelProperty);
        set => SetValue(TexturePreviewModelProperty, value);
    }

    public bool ShowOutline {
        set => SetValue(ShowOutlineProperty, value);
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

    private void OnImageMouseMove(object sender, MouseEventArgs e)
    {
        if (TexturePreviewModel.Texture == null) {
            TexturePreviewModel.MousePixel = null;
            return;
        }

        try {
            var pos = e.GetPosition(img);
            Model.UpdateMouseColor(pos);
        }
        catch (Exception error) {
            Log.Error(error, "Failed to get pixel value at mouse position!");
            TexturePreviewModel.MousePixel = null;
        }
    }

    private void OnImageMouseLeave(object sender, MouseEventArgs e)
    {
        TexturePreviewModel.MousePixel = null;
    }

    private static void OnTexturePreviewModelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TexturePreviewControl control) return;
        control.Model.TexturePreviewData = (ITexturePreviewModel)e.NewValue;
    }

    private static void OnShowOutlinePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TexturePreviewControl control) return;
        control.Model.TexturePreviewData.ShowOutline = (bool)e.NewValue;
    }

    public static readonly DependencyProperty TexturePreviewModelProperty = DependencyProperty
        .Register(nameof(TexturePreviewModel), typeof(ITexturePreviewModel), typeof(TexturePreviewControl), new PropertyMetadata(OnTexturePreviewModelPropertyChanged));

    public static readonly DependencyProperty ShowOutlineProperty = DependencyProperty
        .Register(nameof(ShowOutline), typeof(bool), typeof(TexturePreviewControl), new PropertyMetadata(OnShowOutlinePropertyChanged));
}