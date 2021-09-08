using HelixToolkit.SharpDX.Core.Utilities;

namespace PixelGraph.UI.Helix.CubeMaps
{
    public interface ICubeMapSource
    {
        ShaderResourceViewProxy CubeMap {get;}
        long LastUpdated {get;}
    }
}
