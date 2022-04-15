using HelixToolkit.SharpDX.Core.Utilities;

namespace PixelGraph.Rendering.LUTs
{
    public interface ILutMapSource
    {
        ShaderResourceViewProxy LutMap {get;}
        long LastUpdated {get;}
    }
}
