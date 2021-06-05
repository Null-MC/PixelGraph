using SharpDX;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpDX.Direct3D9;

namespace PixelGraph.UI.Internal.dx
{
    public static class DXSharing
	{
		public static Format ToD3D9(this SharpDX.DXGI.Format dxgiformat)
		{
            return dxgiformat switch {
                SharpDX.DXGI.Format.R10G10B10A2_UNorm => Format.A2B10G10R10,
                SharpDX.DXGI.Format.B8G8R8A8_UNorm => Format.A8R8G8B8,
                SharpDX.DXGI.Format.R16G16B16A16_Float => Format.A16B16G16R16F,
                // not sure those one below will work...
                SharpDX.DXGI.Format.R32G32B32A32_Float => Format.A32B32G32R32F,
                SharpDX.DXGI.Format.R16G16B16A16_UNorm => Format.A16B16G16R16,
                SharpDX.DXGI.Format.R32G32_Float => Format.G32R32F,
                SharpDX.DXGI.Format.R8G8B8A8_UNorm => Format.A8R8G8B8,
                SharpDX.DXGI.Format.R16G16_UNorm => Format.G16R16,
                SharpDX.DXGI.Format.R16G16_Float => Format.G16R16F,
                SharpDX.DXGI.Format.R32_Float => Format.R32F,
                SharpDX.DXGI.Format.R16_Float => Format.R16F,
                SharpDX.DXGI.Format.A8_UNorm => Format.A8,
                SharpDX.DXGI.Format.R8_UNorm => Format.L8,
                //SharpDX.DXGI.Format.BC1_UNorm => Format.MtDxt1,
                //SharpDX.DXGI.Format.BC2_UNorm => Format.MtDxt3,
                //SharpDX.DXGI.Format.BC3_UNorm => Format.MtDxt5,
                _ => Format.Unknown,
            };
        }

		public static Texture GetSharedD3D9(this DeviceEx device, SharpDX.Direct3D11.Texture2D renderTarget)
		{
			if (renderTarget == null) return null;

			if ((renderTarget.Description.OptionFlags & SharpDX.Direct3D11.ResourceOptionFlags.Shared) == 0)
				throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");

			var format = ToD3D9(renderTarget.Description.Format);
			if (format == Format.Unknown)
				throw new ArgumentException("Texture format is not compatible with OpenSharedResource");

            using var resource = renderTarget.QueryInterface<SharpDX.DXGI.Resource>();
            var handle = resource.SharedHandle;
            if (handle == IntPtr.Zero) throw new ArgumentNullException("Handle");

            return new Texture(device, renderTarget.Description.Width, renderTarget.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle);
        }

		#region D3D11.Texture2D: GetBitmap()

		public static unsafe WriteableBitmap GetBitmap(this SharpDX.Direct3D11.Texture2D tex)
		{
            using var copy = tex.GetCopy();
            using var surface = copy.QueryInterface<SharpDX.DXGI.Surface>();
            var db = surface.Map(SharpDX.DXGI.MapFlags.Read);
            // can't destroy the surface now with WARP driver

            var w = tex.Description.Width;
            var h = tex.Description.Height;
            var wb = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgra32, null);
            wb.Lock();

            try {
                var wbb = (uint*)wb.BackBuffer;

                db.Data.Position = 0;
                for (var y = 0; y < h; y++) {
                    db.Data.Position = y * db.Pitch;
                    for (var x = 0; x < w; x++) {
                        var c = db.Data.Read<uint>();
                        wbb[y * w + x] = c;
                    }
                }
            }
            finally {
                wb.AddDirtyRect(new Int32Rect(0, 0, w, h));
                wb.Unlock();
            }

            return wb;
        }

		static SharpDX.Direct3D11.Texture2D GetCopy(this SharpDX.Direct3D11.Texture2D tex)
		{
			var teximg = new SharpDX.Direct3D11.Texture2D(tex.Device, new SharpDX.Direct3D11.Texture2DDescription {
				Usage = SharpDX.Direct3D11.ResourceUsage.Staging,
				BindFlags = SharpDX.Direct3D11.BindFlags.None,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read,
				Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
				ArraySize = tex.Description.ArraySize,
				Height = tex.Description.Height,
				Width = tex.Description.Width,
				MipLevels = tex.Description.MipLevels,
				SampleDescription = tex.Description.SampleDescription,
			});

			tex.Device.ImmediateContext.CopyResource(tex, teximg);
			return teximg;
		}

		#endregion
    }
}
