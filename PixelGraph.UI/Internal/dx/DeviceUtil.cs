using System;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.dx
{
    public static class DeviceUtil
	{
        private static int sAdapterCount = -1; // cache it, as the underlying code rely on Exception to find the value!!!
		
		public static int AdapterCount {
			get {
                if (sAdapterCount == -1) {
                    using var f = new Factory(IntPtr.Zero);
                    sAdapterCount = f.GetAdapterCount();
                }

                return sAdapterCount;
			}
		}

		public static IEnumerable<Adapter> GetAdapters(DisposeGroup dg)
        {
            // NOTE: SharpDX 1.3 requires explicit Dispose() of everything
			// hence the DisposeGroup, to enforce it
            using var f = new Factory(IntPtr.Zero);
            var n = AdapterCount;

            for (var i = 0; i < n; i++)
                yield return dg.Add(f.GetAdapter(i));
        }

		public static Adapter GetBestAdapter(DisposeGroup dg)
		{
			var high = FeatureLevel.Level_9_1;
			Adapter ada = null;

			foreach (var item in GetAdapters(dg)) {
				var level = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(item);

				if (ada == null || level > high) {
					ada = item;
					high = level;
				}
			}

			return ada;
		}

		//public static SharpDX.Direct3D10.Device1 Create10(
		//	Direct3D10.DeviceCreationFlags cFlags = Direct3D10.DeviceCreationFlags.None
		//	, Direct3D10.FeatureLevel minLevel = Direct3D10.FeatureLevel.Level_9_1)
		//{
		//	using (var dg = new DisposeGroup())
		//	{
		//		var ada = GetBestAdapter(dg);
		//		if (ada == null)
		//			return null;
		//		var level = Direct3D11.Device.GetSupportedFeatureLevel(ada);
		//		Direct3D10.FeatureLevel level10 = Direct3D10.FeatureLevel.Level_10_1;
		//		if (level < Direct3D.FeatureLevel.Level_10_1)
		//			level10 = (Direct3D10.FeatureLevel)(int)level;
		//		if (level10 < minLevel)
		//			return null;
		//		return new Direct3D10.Device1(ada, cFlags, level10);
		//	}
		//}

		public static SharpDX.Direct3D11.Device Create11(
            SharpDX.Direct3D11.DeviceCreationFlags cFlags = SharpDX.Direct3D11.DeviceCreationFlags.None,
			FeatureLevel minLevel = FeatureLevel.Level_9_1)
        {
            using var dg = new DisposeGroup();

            var ada = GetBestAdapter(dg);
            if (ada == null) return null;

            var level = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(ada);
            if (level < minLevel) return null;

            return new SharpDX.Direct3D11.Device(ada, cFlags, level);
        }
	}
}
