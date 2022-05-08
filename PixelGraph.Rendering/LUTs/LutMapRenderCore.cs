using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using HelixToolkit.SharpDX.Core.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace PixelGraph.Rendering.LUTs
{
    internal abstract class LutMapRenderCore : RenderCore, ILutMapSource
    {
        private readonly ConstantBufferComponent modelCB;
        private RenderTargetView renderTargetView;
        protected ShaderPass defaultShaderPass;
        private ShaderResourceViewProxy lutMap;
        private RasterizerStateProxy rasterState;
        private Viewport viewport;
        private int _resolution;
        private bool isRenderValid;

        public ShaderResourceViewProxy LutMap => lutMap;

        protected string PassName;
        public ScreenQuadModelStruct ModelStruct;
        protected Texture2DDescription TextureDesc;
        public long LastUpdated {get; set;}
        
        public int Resolution {
            get => _resolution;
            set => SetAffectsRender(ref _resolution, value);
        }

        protected ShaderPass DefaultShaderPass {
            get => defaultShaderPass;
            private set => SetAffectsRender(ref defaultShaderPass, value);
        }


        protected LutMapRenderCore(RenderType renderType) : base(renderType)
        {
            PassName = DefaultPassNames.Default;
            defaultShaderPass = ShaderPass.NullPass;

            TextureDesc = new Texture2DDescription {
                Format = Format.R8G8B8A8_UNorm,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                ArraySize = 1,
                MipLevels = 0,
            };

            ModelStruct = new ScreenQuadModelStruct {
                TopLeft = new Vector4(-1, 1, 1, 1),
                TopRight = new Vector4(1, 1, 1, 1),
                BottomLeft = new Vector4(-1, -1, 1, 1),
                BottomRight = new Vector4(1, -1, 1, 1),
                TexTopLeft = new Vector2(0, 1),
                TexTopRight = new Vector2(1, 1),
                TexBottomLeft = new Vector2(0, 0),
                TexBottomRight = new Vector2(1, 0),
            };

            var modelBufferDesc = new ConstantBufferDescription(DefaultBufferNames.ScreenQuadCB, ScreenQuadModelStruct.SizeInBytes);
            modelCB = AddComponent(new ConstantBufferComponent(modelBufferDesc));
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            DefaultShaderPass = technique.GetPass(PassName);

            CreateLutMapResources();

            var rasterDesc = new RasterizerStateDescription {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
            };

            CreateRasterState(rasterDesc, true);

            isRenderValid = false;
            return true;
        }

        protected override void OnDetach()
        {
            lutMap = null;
            TextureDesc.Width = TextureDesc.Height = 0;
            rasterState = null;

            renderTargetView = null;
            //isRenderValid = false;

            base.OnDetach();
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            //if (! && !context.UpdatePerFrameRenderableRequested) return;

            if (isRenderValid && !context.UpdateSceneGraphRequested) return;

            if (CreateLutMapResources()) {
                RaiseInvalidateRender();
                return;
            }

            //if (LastUpdated > 0) return;

            deviceContext.SetRenderTarget(null, renderTargetView);
            deviceContext.SetViewport(ref viewport);
            deviceContext.SetScissorRectangle(0, 0, _resolution, _resolution);

            ModelStruct.mWorld = ModelMatrix;
            modelCB.Upload(deviceContext, ref ModelStruct);

            DefaultShaderPass.BindShader(deviceContext);
            DefaultShaderPass.BindStates(deviceContext, StateType.BlendState | StateType.DepthStencilState | StateType.RasterState);

            deviceContext.Draw(4, 0);
            deviceContext.ClearRenderTagetBindings();

            LastUpdated = Environment.TickCount64;
            isRenderValid = true;
        }

        protected virtual bool CreateRasterState(RasterizerStateDescription description, bool force)
        {
            var newRasterState = EffectTechnique.EffectsManager.StateManager.Register(description);

            RemoveAndDispose(ref rasterState);
            rasterState = Collect(newRasterState);
            return true;
        }

        private bool CreateLutMapResources()
        {
            if (TextureDesc.Width == _resolution && lutMap is {IsDisposed: false}) return false;
            
            TextureDesc.Width = TextureDesc.Height = _resolution;

            RemoveAndDispose(ref lutMap);
            lutMap = Collect(new ShaderResourceViewProxy(Device, TextureDesc));

            var srvDesc = new ShaderResourceViewDescription {
                Format = TextureDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                },
            };
            lutMap.CreateView(srvDesc);

            var rtsDesc = new RenderTargetViewDescription {
                Format = TextureDesc.Format,
                Dimension = RenderTargetViewDimension.Texture2D,
                Texture2D = new RenderTargetViewDescription.Texture2DResource {
                    MipSlice = 0,
                },
            };

            RemoveAndDispose(ref renderTargetView);
            renderTargetView = Collect(new RenderTargetView(Device, lutMap.Resource, rtsDesc));

            viewport = new Viewport(0, 0, _resolution, _resolution);
            return true;
        }
    }
}
