using HelixToolkit.SharpDX.Core.Utilities;

namespace PixelGraph.UI.Internal.Preview.CubeMaps
{
    public interface ICubeMapSource
    {
        ShaderResourceViewProxy CubeMap {get;}
        long LastUpdated {get;}
    }
}
