using HelixToolkit.SharpDX.Core.Utilities;

namespace PixelGraph.UI.Internal.Preview.Sky
{
    public interface IEnvironmentCube
    {
        ShaderResourceViewProxy CubeMap {get;}
    }
}
