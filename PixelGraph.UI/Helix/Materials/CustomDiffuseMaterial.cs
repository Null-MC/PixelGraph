using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.Wpf.SharpDX;
using PixelGraph.Rendering.CubeMaps;
using PixelGraph.Rendering.Materials;
using SharpDX;
using SharpDX.Direct3D11;
using System.Windows;

namespace PixelGraph.UI.Helix.Materials
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

        public ICubeMapSource IrradianceCubeMapSource {
            get => (ICubeMapSource)GetValue(IrradianceCubeMapSourceProperty);
            set => SetValue(IrradianceCubeMapSourceProperty, value);
        }

        public SamplerStateDescription SurfaceMapSampler {
            get => (SamplerStateDescription)GetValue(SurfaceMapSamplerProperty);
            set => SetValue(SurfaceMapSamplerProperty, value);
        }

        public SamplerStateDescription IrradianceMapSampler {
            get => (SamplerStateDescription)GetValue(IrradianceMapSamplerProperty);
            set => SetValue(IrradianceMapSamplerProperty, value);
        }

        public Color4 ColorTint {
            get => (Color4)GetValue(ColorTintProperty);
            set => SetValue(ColorTintProperty, value);
        }

        public bool RenderEnvironmentMap {
            get => (bool)GetValue(RenderEnvironmentMapProperty);
            set => SetValue(RenderEnvironmentMapProperty, value);
        }

        public bool RenderShadowMap {
            get => (bool)GetValue(RenderShadowMapProperty);
            set => SetValue(RenderShadowMapProperty, value);
        }


        public CustomDiffuseMaterial() {}

        public CustomDiffuseMaterial(CustomDiffuseMaterialCore core) : base(core)
        {
            DiffuseAlphaMap = core.DiffuseAlphaMap;
            EmissiveMap = core.EmissiveMap;
            IrradianceCubeMapSource = core.IrradianceCubeMapSource;
            SurfaceMapSampler = core.SurfaceMapSampler;
            IrradianceMapSampler = core.IrradianceMapSampler;
            ColorTint = core.ColorTint;
            RenderEnvironmentMap = core.RenderEnvironmentMap;
            RenderShadowMap = core.RenderShadowMap;
        }

        public virtual CustomDiffuseMaterial CloneMaterial()
        {
            return new() {
                DiffuseAlphaMap = DiffuseAlphaMap,
                EmissiveMap = EmissiveMap,
                IrradianceCubeMapSource = IrradianceCubeMapSource,
                SurfaceMapSampler = SurfaceMapSampler,
                IrradianceMapSampler = IrradianceMapSampler,
                ColorTint = ColorTint,
                RenderShadowMap = RenderShadowMap,
                RenderEnvironmentMap = RenderEnvironmentMap,
            };
        }

        protected override MaterialCore OnCreateCore()
        {
            return new CustomDiffuseMaterialCore {
                Name = Name,
                DiffuseAlphaMap = DiffuseAlphaMap,
                EmissiveMap = EmissiveMap,
                IrradianceCubeMapSource = IrradianceCubeMapSource,
                SurfaceMapSampler = SurfaceMapSampler,
                IrradianceMapSampler = IrradianceMapSampler,
                ColorTint = ColorTint,
                RenderShadowMap = RenderShadowMap,
                RenderEnvironmentMap = RenderEnvironmentMap,
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

        public static readonly DependencyProperty IrradianceCubeMapSourceProperty =
            DependencyProperty.Register(nameof(IrradianceCubeMapSource), typeof(ICubeMapSource), typeof(CustomDiffuseMaterial), new PropertyMetadata(null, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).IrradianceCubeMapSource = e.NewValue as ICubeMapSource;
            }));

        public static readonly DependencyProperty SurfaceMapSamplerProperty =
            DependencyProperty.Register(nameof(SurfaceMapSampler), typeof(SamplerStateDescription), typeof(CustomDiffuseMaterial), new PropertyMetadata(DefaultSamplers.LinearSamplerWrapAni4, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).SurfaceMapSampler = (SamplerStateDescription)e.NewValue;
            }));

        public static readonly DependencyProperty IrradianceMapSamplerProperty =
            DependencyProperty.Register(nameof(IrradianceMapSampler), typeof(SamplerStateDescription), typeof(CustomDiffuseMaterial), new PropertyMetadata(DefaultSamplers.IBLSampler, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).IrradianceMapSampler = (SamplerStateDescription)e.NewValue;
            }));

        public static readonly DependencyProperty ColorTintProperty =
            DependencyProperty.Register(nameof(ColorTint), typeof(Color4), typeof(CustomDiffuseMaterial), new PropertyMetadata(Color4.White, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).ColorTint = (Color4)e.NewValue;
            }));

        public static readonly DependencyProperty RenderEnvironmentMapProperty =
            DependencyProperty.Register(nameof(RenderEnvironmentMap), typeof(bool), typeof(CustomDiffuseMaterial), new PropertyMetadata(false, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).RenderEnvironmentMap = (bool)e.NewValue;
            }));

        public static readonly DependencyProperty RenderShadowMapProperty =
            DependencyProperty.Register(nameof(RenderShadowMap), typeof(bool), typeof(CustomDiffuseMaterial), new PropertyMetadata(false, (d, e) => {
                ((CustomDiffuseMaterialCore)((Material)d).Core).RenderShadowMap = (bool)e.NewValue;
            }));
    }
}
