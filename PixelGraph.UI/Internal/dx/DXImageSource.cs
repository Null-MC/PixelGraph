using SharpDX.Direct3D9;
using System;
using System.Windows;
using System.Windows.Interop;

namespace PixelGraph.UI.Internal.dx
{
    public class DXImageSource : D3DImage, IDisposable
	{
        private static int activeClients;
        private static D3D9 d3d9;
        private bool isDisposed;
        private Texture backBuffer;

		public bool IsDisposed => isDisposed;


		public DXImageSource()
		{
			StartD3D9();
		}

        ~DXImageSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

		protected void Dispose(bool disposing)
		{
			if (IsDisposed) return;

			if (disposing) {
				SetBackBuffer((Texture)null);
				GC.SuppressFinalize(this);
			}

			EndD3D9();
			isDisposed = true;
		}


        public void Invalidate()
		{
			if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (backBuffer == null) return;

            Lock();
            AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
            Unlock();
        }

        //public void SetBackBuffer(SharpDX.Direct3D10.Texture2D texture)
        //{
        //    SetBackBuffer(d3d9.Device.GetSharedD3D9(texture));
        //}

        public void SetBackBuffer(SharpDX.Direct3D11.Texture2D texture)
        {
            SetBackBuffer(d3d9.Device.GetSharedD3D9(texture));
        }

		public void SetBackBuffer(Texture texture)
		{
			if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

			Texture toDelete = null;

			try {
				if (texture != backBuffer) {
					// if it's from the private (SDX9ImageSource) D3D9 device, dispose of it
					if (backBuffer != null && backBuffer.Device.NativePointer == d3d9.Device.NativePointer)
						toDelete = backBuffer;

					backBuffer = texture;
				}

				if (texture != null) {
                    using var surface = texture.GetSurfaceLevel(0);

                    Lock();
                    SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                    AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
                    Unlock();
                }
				else
				{
					Lock();
					SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
					AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
					Unlock();
				}
			}
			finally {
                toDelete?.Dispose();
            }
		}

		private static void StartD3D9()
		{
			if (activeClients == 0)
				d3d9 = new D3D9();

			activeClients++;
		}

		private static void EndD3D9()
		{
			activeClients--;

			if (activeClients == 0)
				d3d9.Dispose();
		}
    }
}
