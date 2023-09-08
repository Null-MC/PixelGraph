using HelixToolkit.SharpDX.Core.Utilities;

namespace PixelGraph.Rendering.CubeMaps;

public interface ICubeMapSource
{
    ShaderResourceViewProxy CubeMap {get;}
    long LastUpdated {get;}
}