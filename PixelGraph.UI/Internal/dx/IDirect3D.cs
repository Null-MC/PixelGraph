using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Internal.dx
{
    public class DrawEventArgs : EventArgs
    {
        public TimeSpan TotalTime { get; set; }
        public TimeSpan DeltaTime { get; set; }
        public Size RenderSize { get; set; }
        public DXImageSource Target { get; set; }


        public DrawEventArgs()
        {
            TotalTime = TimeSpan.Zero;
        }
    }

    /// <summary>
    /// The DirectX renderer displayed by the DXElement
    /// </summary>
    public interface IDirect3D
    {
        void Reset(DrawEventArgs args);
        void Render(DrawEventArgs args);
    }

    /// <summary>
    /// A IDirect3D context which handles input event as well
    /// </summary>
    public interface IInteractiveDirect3D : IDirect3D
    {
        void OnMouseDown(UIElement ui, MouseButtonEventArgs e);
        void OnMouseMove(UIElement ui, MouseEventArgs e);
        void OnMouseUp(UIElement ui, MouseButtonEventArgs e);
        void OnMouseWheel(UIElement ui, MouseWheelEventArgs e);
        void OnKeyDown(UIElement ui, KeyEventArgs e);
        void OnKeyUp(UIElement ui, KeyEventArgs e);
    }
}
