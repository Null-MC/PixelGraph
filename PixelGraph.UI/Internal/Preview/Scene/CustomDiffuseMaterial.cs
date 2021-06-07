using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.Wpf.SharpDX;
using SharpDX.Direct3D11;
using System.Windows;

namespace PixelGraph.UI.Internal.Preview.Scene
{
    public class CustomDiffuseMaterial : Material
    {
        public TextureModel DiffuseAlphaMap {
            get => (TextureModel)GetValue(DiffuseAlphaMapProperty);
            set => SetValue(DiffuseAlphaMapProperty, value);
        }

        public TextureModel EmissiveMap {
            get => (TextureModel)GetValue(EmissiveMapProperty);
            set => SetValue(EmissiveMapProperty, value);
        }

        public bool RenderShadowMap {
            get => (bool)GetValue(RenderShadowMapProperty);
            set => SetValue(RenderShadowMapProperty, value);
        }

        public SamplerStateDescription SurfaceMapSampler {
            get => (SamplerStateDescription)GetValue(SurfaceMapSamplerProperty);
            set => SetValue(SurfaceMapSamplerProperty, value);
        }


        public CustomDiffuseMaterial() {}

        public CustomDiffuseMaterial(CustomDiffuseMaterialCore core) : base(core)
        {
            DiffuseAlphaMap = core.DiffuseAlphaMap;
            EmissiveMap = core.EmissiveMap;
            SurfaceMapSampler = core.SurfaceMapSampler;
            RenderShadowMap = core.RenderShadowMap;
        }

        public virtual CustomDiffuseMaterial CloneMaterial()
        {
            return new() {
                DiffuseAlphaMap = DiffuseAlphaMap,
                EmissiveMap = EmissiveMap,
                SurfaceMapSampler = SurfaceMapSampler,
                RenderShadowMap = RenderShadowMap,
            };
        }

        protected override MaterialCore OnCreateCore()
        {
            return new CustomDiffuseMaterialCore {
                Name = Name,
                DiffuseAlphaMap = DiffuseAlphaMap,
                EmissiveMap = EmissiveMap,
                SurfaceMapSampler = SurfaceMapSampler,
                RenderShadowMap = RenderShadowMap,
            };
        }

        protected override Freezable CreateInstanceCore()
        {
            return CloneMaterial();
        }

        public static readonly DependencyProperty DiffuseAlphaMapProperty =
            DependencyProperty.Register(nameof(DiffuseAlphaMap), typeof(TextureModel), typeof(CustomDiffuseMaterial), new PropertyMetadata(null, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).DiffuseAlphaMap = e.NewValue as TextureModel;
            }));

        public static readonly DependencyProperty EmissiveMapProperty =
            DependencyProperty.Register(nameof(EmissiveMap), typeof(TextureModel), typeof(CustomDiffuseMaterial), new PropertyMetadata(null, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).EmissiveMap = e.NewValue as TextureModel;
            }));

        public static readonly DependencyProperty RenderShadowMapProperty =
            DependencyProperty.Register(nameof(RenderShadowMap), typeof(bool), typeof(CustomDiffuseMaterial), new PropertyMetadata(false, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).RenderShadowMap = (bool)e.NewValue;
            }));

        public static readonly DependencyProperty SurfaceMapSamplerProperty =
            DependencyProperty.Register(nameof(SurfaceMapSampler), typeof(SamplerStateDescription), typeof(CustomDiffuseMaterial), new PropertyMetadata(DefaultSamplers.LinearSamplerWrapAni4, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).SurfaceMapSampler = (SamplerStateDescription)e.NewValue;
            }));
    }
}
