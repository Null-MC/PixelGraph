using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Rendering.Materials;
using SharpDX.Direct3D11;
using System.Windows;

namespace PixelGraph.UI.Helix.Materials;

public class CustomNormalsMaterial : Material
{
    public TextureModel OpacityMap {
        get => (TextureModel)GetValue(OpacityMapProperty);
        set => SetValue(OpacityMapProperty, value);
    }

    public TextureModel NormalHeightMap {
        get => (TextureModel)GetValue(NormalHeightMapProperty);
        set => SetValue(NormalHeightMapProperty, value);
    }

    public SamplerStateDescription SurfaceMapSampler {
        get => (SamplerStateDescription)GetValue(SurfaceMapSamplerProperty);
        set => SetValue(SurfaceMapSamplerProperty, value);
    }

    public SamplerStateDescription HeightMapSampler {
        get => (SamplerStateDescription)GetValue(HeightMapSamplerProperty);
        set => SetValue(HeightMapSamplerProperty, value);
    }


    public CustomNormalsMaterial() {}

    public CustomNormalsMaterial(CustomNormalsMaterialCore core) : base(core)
    {
        OpacityMap = core.OpacityMap;
        NormalHeightMap = core.NormalHeightMap;
        SurfaceMapSampler = core.SurfaceMapSampler;
    }

    public virtual CustomNormalsMaterial CloneMaterial()
    {
        return new() {
            OpacityMap = OpacityMap,
            NormalHeightMap = NormalHeightMap,
            SurfaceMapSampler = SurfaceMapSampler,
        };
    }

    protected override MaterialCore OnCreateCore()
    {
        return new CustomNormalsMaterialCore {
            Name = Name,
            OpacityMap = OpacityMap,
            NormalHeightMap = NormalHeightMap,
            SurfaceMapSampler = SurfaceMapSampler,
        };
    }

    protected override Freezable CreateInstanceCore()
    {
        return CloneMaterial();
    }

    public static readonly DependencyProperty OpacityMapProperty =
        DependencyProperty.Register(nameof(OpacityMap), typeof(TextureModel), typeof(CustomNormalsMaterial), new PropertyMetadata(null, (d, e) => {
            ((CustomNormalsMaterialCore)((Material)d).Core).OpacityMap = e.NewValue as TextureModel;
        }));

    public static readonly DependencyProperty NormalHeightMapProperty =
        DependencyProperty.Register(nameof(NormalHeightMap), typeof(TextureModel), typeof(CustomNormalsMaterial), new PropertyMetadata(null, (d, e) => {
            ((CustomNormalsMaterialCore)((Material)d).Core).NormalHeightMap = e.NewValue as TextureModel;
        }));

    public static readonly DependencyProperty SurfaceMapSamplerProperty =
        DependencyProperty.Register(nameof(SurfaceMapSampler), typeof(SamplerStateDescription), typeof(CustomNormalsMaterial), new PropertyMetadata(DefaultSamplers.LinearSamplerWrapAni4, (d, e) => {
            ((CustomNormalsMaterialCore)((Material)d).Core).SurfaceMapSampler = (SamplerStateDescription)e.NewValue;
        }));

    public static readonly DependencyProperty HeightMapSamplerProperty =
        DependencyProperty.Register(nameof(HeightMapSampler), typeof(SamplerStateDescription), typeof(CustomNormalsMaterial), new PropertyMetadata(DefaultSamplers.LinearSamplerWrapAni16, (d, e) => {
            ((CustomNormalsMaterialCore)((Material)d).Core).HeightMapSampler = (SamplerStateDescription)e.NewValue;
        }));
}