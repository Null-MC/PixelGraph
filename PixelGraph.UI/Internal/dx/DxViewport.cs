using SharpDX.Direct3D9;
using System;
using System.Windows;
using System.Windows.Interop;

namespace PixelGraph.UI.Internal.dx
{
    internal class DxViewport : IDisposable
    {
        private readonly Direct3D d3d;
        private Device d3dDevice;
        private bool isBound;


        public DxViewport()
        {
            d3d = new Direct3D();
        }

        public void Dispose()
        {
            d3dDevice?.Dispose();
            d3d?.Dispose();
        }

        public void Bind(Window window)
        {
            if (isBound) throw new ApplicationException("A D3D device is already bound!");
            isBound = true;

            //Get a handle to the WPF window. This is required to create a device.
            var windowHandle = new WindowInteropHelper(window).Handle;

            //Create a device. Using standard creation param. 
            //Width and height have been set to 1 because we wont be using the backbuffer.
            //Adapter 0 = default adapter.
            var presentationParams = new PresentParameters(1, 1);

            d3dDevice = new Device(d3d, 0, DeviceType.Hardware, windowHandle, CreateFlags.HardwareVertexProcessing, presentationParams);
        }

        public void CreateTarget()
        {
            //Create the surface that will act as the render target.
            //Set as lockable (required for D3DImage)
            var target = Surface.CreateRenderTarget(d3dDevice, ImageWidthPixels, ImageHeightPixels, Format.A8R8G8B8, MultisampleType.None, 0, true);
        }

        public void UpdateSurface()
        {
            //Copy the image surface contents into the target surface.
            d3dDevice.UpdateSurface(imageSurface, target);
        }
    }
}
