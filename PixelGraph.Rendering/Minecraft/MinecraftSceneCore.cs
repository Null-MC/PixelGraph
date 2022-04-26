using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Core.Components;
using HelixToolkit.SharpDX.Core.Render;
using HelixToolkit.SharpDX.Core.Shaders;
using PixelGraph.Rendering.Shaders;
using SharpDX;
using System;

namespace PixelGraph.Rendering.Minecraft
{
    public interface IMinecraftSceneCore
    {
        //bool IsRenderValid {get;}
        long LastUpdated {get;}

        void Apply(DeviceContextProxy deviceContext);
        //void ResetValidation();
    }

    public interface IMinecraftScene : IMinecraftSceneCore
    {
        //bool EnableAtmosphere {get;}
    }

    internal class MinecraftSceneCore : RenderCore, IMinecraftScene
    {
        private readonly ConstantBufferComponent buffer;
        private MinecraftSceneStruct data;
        private bool isRenderValid;

        //public bool IsRenderValid {get; private set;}
        public long LastUpdated {get; private set;}

        public bool EnableAtmosphere {
            get => data.EnableAtmosphere;
            set => SetAffectsRender(ref data.EnableAtmosphere, value);
        }

        public Vector3 SunDirection {
            get => data.SunDirection;
            set {
                if (SetAffectsRender(ref data.SunDirection, value))
                    isRenderValid = false;
            }
        }

        public float SunStrength {
            get => data.SunStrength;
            set {
                if (SetAffectsRender(ref data.SunStrength, value))
                    isRenderValid = false;
            }
        }

        public float TimeOfDay {
            get => data.TimeOfDay;
            set {
                if (SetAffectsRender(ref data.TimeOfDay, value))
                    isRenderValid = false;
            }
        }

        public float Wetness {
            get => data.Wetness;
            set {
                if (SetAffectsRender(ref data.Wetness, value))
                    isRenderValid = false;
            }
        }

        public float ParallaxDepth {
            get => data.ParallaxDepth;
            set => SetAffectsRender(ref data.ParallaxDepth, value);
        }

        public int ParallaxSamples {
            get => data.ParallaxSamples;
            set => SetAffectsRender(ref data.ParallaxSamples, value);
        }

        public bool EnableLinearSampling {
            get => data.EnableLinearSampling;
            set {
                if (SetAffectsRender(ref data.EnableLinearSampling, value))
                    isRenderValid = false;
            }
        }

        public bool EnableSlopeNormals {
            get => data.EnableSlopeNormals;
            set {
                if (SetAffectsRender(ref data.EnableSlopeNormals, value))
                    isRenderValid = false;
            }
        }

        public int WaterMode {
            get => data.WaterMode;
            set => SetAffectsRender(ref data.WaterMode, value);
        }

        public float ErpExposure {
            get => data.ErpExposure;
            set {
                if (SetAffectsRender(ref data.ErpExposure, value))
                    isRenderValid = false;
            }
        }


        public MinecraftSceneCore() : base(RenderType.PreProc)
        {
            var bufferDesc = new ConstantBufferDescription(CustomBufferNames.MinecraftSceneCB, MinecraftSceneStruct.SizeInBytes);
            buffer = AddComponent(new ConstantBufferComponent(bufferDesc));

            data.ErpExposure = 1f;
        }

        public void Apply(DeviceContextProxy deviceContext)
        {
            buffer.Upload(deviceContext, ref data);
        }

        //public void ResetValidation()
        //{
        //    isRenderValid = true;
        //}

        protected override bool OnAttach(IRenderTechnique technique)
        {
            return true;
        }

        public override void Render(RenderContext context, DeviceContextProxy deviceContext)
        {
            if (isRenderValid) return;

            Apply(deviceContext);

            LastUpdated = Environment.TickCount64;
            isRenderValid = true;
        }
    }
}
