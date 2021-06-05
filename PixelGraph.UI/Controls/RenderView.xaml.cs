using SharpDX.Direct3D9;
using System.Windows;
using System.Windows.Interop;

namespace PixelGraph.UI.Controls
{
    public partial class RenderView
    {
        public RenderView()
        {
            InitializeComponent();
        }

        public void UpdateSurface(Surface target)
        {
            Lock();
            SetBackBuffer(D3DResourceType.IDirect3DSurface9, target.NativePointer);
            AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
            Unlock();
        }
    }
}
