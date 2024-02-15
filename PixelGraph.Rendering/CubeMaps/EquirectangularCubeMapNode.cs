using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using PixelGraph.Rendering.Shaders;
using SharpDX;

namespace PixelGraph.Rendering.CubeMaps;

public class EquirectangularCubeMapNode : SceneNode, ICubeMapSource
{
    public int FaceSize {
        get => _renderCore?.FaceSize ?? 0;
        set {
            if (_renderCore != null)
                _renderCore.FaceSize = value;
        }
    }

    public TextureModel? Texture {
        get => _renderCore?.Texture;
        set {
            if (_renderCore != null)
                _renderCore.Texture = value;
        }
    }

    public float Exposure {
        get => _renderCore?.Intensity ?? 1f;
        set {
            if (_renderCore != null)
                _renderCore.Intensity = value;
        }
    }

    private EquirectangularCubeMapCore? _renderCore => RenderCore as EquirectangularCubeMapCore;
    public ShaderResourceViewProxy? CubeMap => _renderCore?.CubeMap;
    public long LastUpdated => _renderCore?.LastUpdated ?? 0;


    protected override RenderCore OnCreateRenderCore()
    {
        return new EquirectangularCubeMapCore();
    }

    protected override void AssignDefaultValuesToCore(RenderCore core)
    {
        base.AssignDefaultValuesToCore(core);
        if (core is not EquirectangularCubeMapCore c) return;

        c.FaceSize = FaceSize;
        c.Texture = Texture;
        c.Intensity = Exposure;
    }

    protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
    {
        return effectsManager[CustomRenderTechniqueNames.DynamicSkybox];
    }

    protected override bool CanHitTest(HitTestContext context) => false;

    protected override bool OnHitTest(HitTestContext context, Matrix totalModelMatrix, ref List<HitTestResult> hits) => false;
}