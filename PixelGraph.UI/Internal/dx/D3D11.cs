using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;

namespace PixelGraph.UI.Internal.dx
{
    public class D3D11 : D3D
	{
		protected Device device;
        protected Texture2D renderTarget;
        protected RenderTargetView renderTargetView;
        protected Texture2D depthStencil;
        protected DepthStencilView depthStencilView;

        // must be shared to be displayed in a D3DImage
        ResourceOptionFlags mRenderTargetOptionFlags = ResourceOptionFlags.Shared;

        public bool IsDisposed => device == null;

        public Device Device => device.GetOrThrow();

        public Texture2D RenderTarget => Prepared(ref renderTarget);
        public RenderTargetView RenderTargetView => Prepared(ref renderTargetView);

        public Texture2D DepthStencil => Prepared(ref depthStencil);
        public DepthStencilView DepthStencilView => Prepared(ref depthStencilView);

        public ResourceOptionFlags RenderTargetOptionFlags {
            get => mRenderTargetOptionFlags;
            set {
                if (value == mRenderTargetOptionFlags) return;
                mRenderTargetOptionFlags = value;
                OnPropertyChanged("RenderTargetOptionFlags");
            }
        }


		protected D3D11(bool b) {}

		public D3D11(FeatureLevel minLevel)
		{
			device = DeviceUtil.Create11(DeviceCreationFlags.BgraSupport, minLevel);
			if (device == null) throw new NotSupportedException();
		}

		public D3D11() : this((Device)null) {}

		public D3D11(Device dev)
		{
			// REMARK: SharpDX.Direct3D.DriverType.Warp works without graphics card!
			if (dev != null) {
				dev.AddReference();
				device = dev;
			}
			else {
				device = DeviceUtil.Create11(DeviceCreationFlags.BgraSupport);
				if (device == null) throw new NotSupportedException();
			}
		}

		public D3D11(Adapter a)
		{
			if (a == null) {
				device = DeviceUtil.Create11(DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
				if (device == null) throw new NotSupportedException();
			}

			device = new Device(a);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			// NOTE: SharpDX 1.3 requires explicit Dispose() of everything
			Set(ref device, null);
			Set(ref renderTarget, null);
			Set(ref renderTargetView, null);
			Set(ref depthStencil, null);
			Set(ref depthStencilView, null);
		}

        public  override void SetBackBuffer(DXImageSource dximage) { dximage.SetBackBuffer(RenderTarget); }

		public override void Reset(int w, int h)
		{
			device.GetOrThrow();

			if (w < 1) throw new ArgumentOutOfRangeException(nameof(w));
			if (h < 1) throw new ArgumentOutOfRangeException(nameof(h));

			var desc = new Texture2DDescription {
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				Format = Format.B8G8R8A8_UNorm,
				Width = w,
				Height = h,
				MipLevels = 1,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				OptionFlags = RenderTargetOptionFlags,
				CpuAccessFlags = CpuAccessFlags.None,
				ArraySize = 1
			};

			Set(ref renderTarget, new Texture2D(device, desc));
			Set(ref renderTargetView, new RenderTargetView(device, renderTarget));

			Set(ref depthStencil, device.CreateTexture2D(w, h, BindFlags.DepthStencil, Format.D24_UNorm_S8_UInt));
			Set(ref depthStencilView, new DepthStencilView(device, depthStencil));
	
			device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, w, h, 0.0f, 1.0f));
			device.ImmediateContext.OutputMerger.SetRenderTargets(1, new[] { renderTargetView }, depthStencilView);
		}

		public override void BeginRender(DrawEventArgs args)
		{
			device.GetOrThrow();
			Device.ImmediateContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
		}

		public override void EndRender(DrawEventArgs args)
		{
			Device.ImmediateContext.Flush();
		}

		protected T Prepared<T>(ref T property)
		{
			device.GetOrThrow();
			if (property == null) Reset(1, 1);
			return property;
		}

        public override System.Windows.Media.Imaging.WriteableBitmap ToImage()
        {
            return RenderTarget.GetBitmap();
        }
	}
}
