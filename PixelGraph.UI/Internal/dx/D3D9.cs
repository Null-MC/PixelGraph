using SharpDX.Direct3D9;
using System;
using System.Runtime.InteropServices;

namespace PixelGraph.UI.Internal.dx
{
    public class D3D9 : D3D
	{
        private Texture renderTarget;

		protected Direct3DEx context;
		protected DeviceEx device;

        public bool IsDisposed => device == null;
		public DeviceEx Device => device.GetOrThrow();
		public Texture RenderTarget => Prepared(ref renderTarget);


		protected D3D9(bool b) {}

		public D3D9() : this(null) {}

		public D3D9(DeviceEx device)
        {
            if (device != null) throw new NotSupportedException("dunno how to get the context");
            
            context = new Direct3DEx();

            var presentparams = new PresentParameters {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = GetDesktopWindow(),
                PresentationInterval = PresentInterval.Default,
            };

            this.device = new DeviceEx(context, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				Set(ref device, null);
				Set(ref context, null);
			}
		}

		public override void Reset(int w, int h)
		{
			device.GetOrThrow();

			if (w < 1) throw new ArgumentOutOfRangeException(nameof(w));
            if (h < 1) throw new ArgumentOutOfRangeException(nameof(h));

			Set(ref renderTarget, new Texture(device, w, h, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default));

			// TODO test that...
            using var surface = renderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, surface);
		}

		protected T Prepared<T>(ref T property)
		{
			device.GetOrThrow();

			if (property == null)
				Reset(1, 1);

			return property;
		}

        public override void SetBackBuffer(DXImageSource dximage)
        {
            dximage.SetBackBuffer(RenderTarget);
        }

        public override System.Windows.Media.Imaging.WriteableBitmap ToImage()
        {
            throw new NotImplementedException();
        }

        [DllImport("user32.dll", SetLastError = false)]
		static extern IntPtr GetDesktopWindow();
	}
}
