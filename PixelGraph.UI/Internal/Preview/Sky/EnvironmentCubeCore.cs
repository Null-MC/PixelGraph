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
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Preview.Sky
{
    internal class EnvironmentCubeCore : RenderCore, IEnvironmentCube
    {
        private const float NearField = 0.1f;
        private const float FarField = 100f;

        private readonly Vector3[] targets;
        private readonly Vector3[] lookVector;
        private readonly Vector3[] upVectors;
        private readonly CubeFaceCamerasStruct cubeFaceCameras;
        private readonly RenderTargetView[] cubeRTVs;
        private readonly ConstantBufferComponent modelCB;
        private readonly CommandList[] commands;

        private RasterizerStateProxy invertCullModeState;
        private SkyDomeBufferModel geometryBuffer;
        private Texture2DDescription textureDesc;
        private IElementsBufferModel instanceBuffer;
        private IDeviceContextPool contextPool;
        private ShaderPass defaultShaderPass;
        private ShaderResourceViewProxy cubeMap;
        private RasterizerStateProxy rasterState;
        private Viewport viewport;
        private IMinecraftScene _scene;
        private int faceSize;

        public ShaderResourceViewProxy CubeMap => cubeMap;
        
        public IMinecraftScene Scene {
            get => _scene;
            set => SetAffectsRender(ref _scene, value);
        }

        public int FaceSize {
            get => faceSize;
            set => SetAffectsRender(ref faceSize, value);
        }

        protected ShaderPass DefaultShaderPass {
            get => defaultShaderPass;
            private set => SetAffectsRender(ref defaultShaderPass, value);
        }

        public IElementsBufferModel InstanceBuffer {
            get => instanceBuffer;
            set {
                var old = instanceBuffer;
                if (!SetAffectsCanRenderFlag(ref instanceBuffer, value)) return;

                if (old != null)
                    old.ElementChanged -= OnElementChanged;

                if (instanceBuffer != null) {
                    instanceBuffer.ElementChanged += OnElementChanged;
                }
                else {
                    instanceBuffer = MatrixInstanceBufferModel.Empty;
                }
            }
        }


        public EnvironmentCubeCore() : base(RenderType.PreProc)
        {
            faceSize = 256;

            lookVector = new[] { Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ };
            upVectors = new[] { Vector3.UnitY, Vector3.UnitY, -Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY, Vector3.UnitY };
            cubeRTVs = new RenderTargetView[6];
            commands = new CommandList[6];
            targets = new Vector3[6];

            cubeFaceCameras = new CubeFaceCamerasStruct {
                Cameras = new CubeFaceCamera[6],
            };

            instanceBuffer = MatrixInstanceBufferModel.Empty;
            defaultShaderPass = ShaderPass.NullPass;

            textureDesc = new Texture2DDescription {
                Format = Format.R8G8B8A8_UNorm,
                ArraySize = 6,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                MipLevels = 0,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
            };

            var modelBufferDesc = new ConstantBufferDescription(DefaultBufferNames.GlobalTransformCB, GlobalTransformStruct.SizeInBytes);
            modelCB = AddComponent(new ConstantBufferComponent(modelBufferDesc));

            UpdateTargets();
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            if (CreateCubeMapResources()) {
                RaiseInvalidateRender();
                return;
            }

            if (_scene == null || _scene.IsRenderValid) {
                if (!context.UpdateSceneGraphRequested && !context.UpdatePerFrameRenderableRequested) return;
            }

            context.IsInvertCullMode = true;

            Exception exception = null;
            Parallel.For(0, 6, index => {
                try {
                    var ctx = contextPool.Get();

                    ctx.ClearRenderTargetView(cubeRTVs[index], Color.Red);

                    ctx.SetRenderTarget(null, cubeRTVs[index]);
                    ctx.SetViewport(ref viewport);
                    ctx.SetScissorRectangle(0, 0, FaceSize, FaceSize);

                    var transforms = new GlobalTransformStruct {
                        Projection = cubeFaceCameras.Cameras[index].Projection,
                        View = cubeFaceCameras.Cameras[index].View,
                        Viewport = new Vector4(FaceSize, FaceSize, 1f / FaceSize, 1f / FaceSize),
                    };

                    transforms.ViewProjection = transforms.View * transforms.Projection;

                    modelCB.Upload(ctx, ref transforms);
                    Scene.Apply(ctx);

                    DefaultShaderPass.BindShader(ctx);
                    DefaultShaderPass.BindStates(ctx, StateType.BlendState | StateType.DepthStencilState);
                    ctx.SetRasterState(invertCullModeState);

                    var vertexStartSlot = 0;
                    if (geometryBuffer.AttachBuffers(ctx, ref vertexStartSlot, EffectTechnique.EffectsManager))
                        InstanceBuffer.AttachBuffer(ctx, ref vertexStartSlot);

                    ctx.DrawIndexed(geometryBuffer.IndexBuffer.ElementCount, 0, 0);

                    commands[index] = ctx.FinishCommandList(true);
                    contextPool.Put(ctx);
                }
                catch(Exception ex) {
                    exception = ex;
                }                
            });

            context.IsInvertCullMode = false;

            if (exception != null) throw exception;
            
            for (var i = 0; i < commands.Length; ++i) {
                if (commands[i] == null) continue;

                Device.ImmediateContext.ExecuteCommandList(commands[i], true);
                Disposer.RemoveAndDispose(ref commands[i]);
            }

            deviceContext.GenerateMips(cubeMap);
            context.SharedResource.EnvironmentMapMipLevels = cubeMap.TextureView.Description.TextureCube.MipLevels;
            
            context.UpdatePerFrameData(true, false, deviceContext);
            _scene?.Apply(deviceContext);

            _scene?.ResetValidation();
        }

        protected override bool OnAttach(IRenderTechnique technique)
        {
            DefaultShaderPass = technique[DefaultPassNames.Default];

            contextPool = technique.EffectsManager.DeviceContextPool;

            CreateCubeMapResources();

            geometryBuffer = Collect(new SkyDomeBufferModel());

            var rasterDesc = new RasterizerStateDescription {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
            };

            CreateRasterState(rasterDesc, true);

            return true;
        }

        protected override void OnDetach()
        {
            contextPool = null;
            cubeMap = null;
            textureDesc.Width = textureDesc.Height = 0;
            geometryBuffer = null;
            invertCullModeState = null;
            rasterState = null;

            for (var i = 0; i < 6; ++i)
                cubeRTVs[i] = null;

            base.OnDetach();
        }

        protected void OnElementChanged(object sender, EventArgs e)
        {
            UpdateCanRenderFlag();
            RaiseInvalidateRender();
        }

        protected override bool OnUpdateCanRenderFlag()
        {
            return base.OnUpdateCanRenderFlag() && geometryBuffer != null;
        }

        protected virtual bool CreateRasterState(RasterizerStateDescription description, bool force)
        {
            var newRasterState = EffectTechnique.EffectsManager.StateManager.Register(description);
            var invCull = description;

            if (description.CullMode != CullMode.None) {
                invCull.CullMode = description.CullMode == CullMode.Back ? CullMode.Front : CullMode.Back;
            }

            var newInvertCullModeState = EffectTechnique.EffectsManager.StateManager.Register(invCull);

            RemoveAndDispose(ref rasterState);
            RemoveAndDispose(ref invertCullModeState);
            rasterState = Collect(newRasterState);
            invertCullModeState = Collect(newInvertCullModeState);
            return true;
        }

        private bool CreateCubeMapResources()
        {
            if (textureDesc.Width == faceSize && cubeMap is {IsDisposed: false}) return false;
            
            textureDesc.Width = textureDesc.Height = FaceSize;

            RemoveAndDispose(ref cubeMap);
            cubeMap = Collect(new ShaderResourceViewProxy(Device, textureDesc));

            var srvDesc = new ShaderResourceViewDescription {
                Format = textureDesc.Format,
                Dimension = ShaderResourceViewDimension.TextureCube,
                TextureCube = new ShaderResourceViewDescription.TextureCubeResource {
                    MostDetailedMip = 0,
                    MipLevels = -1,
                },
            };
            cubeMap.CreateView(srvDesc);

            var rtsDesc = new RenderTargetViewDescription {
                Format = textureDesc.Format,
                Dimension = RenderTargetViewDimension.Texture2DArray,
                Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource {
                    MipSlice = 0,
                    FirstArraySlice = 0,
                    ArraySize = 1,
                },
            };

            for (var i = 0; i < 6; ++i) {
                RemoveAndDispose(ref cubeRTVs[i]);
                rtsDesc.Texture2DArray.FirstArraySlice = i;
                cubeRTVs[i] = Collect(new RenderTargetView(Device, cubeMap.Resource, rtsDesc));
            }

            viewport = new Viewport(0, 0, FaceSize, FaceSize);
            return true;
        }

        private void UpdateTargets()
        {
            for (var i = 0; i < 6; ++i) {
                var mView = Matrix.LookAtRH(Vector3.Zero, lookVector[i], upVectors[i]);
                var mProj = Matrix.PerspectiveFovRH((float)Math.PI * 0.5f, 1, NearField, FarField);

                targets[i] = lookVector[i];
                cubeFaceCameras.Cameras[i].View = mView * Matrix.Scaling(-1, 1, 1);
                cubeFaceCameras.Cameras[i].Projection = mProj;
            }
        }
    }
}
