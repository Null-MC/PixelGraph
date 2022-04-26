using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using System;

namespace PixelGraph.Common.Material
{
    public class MaterialOcclusionProperties : IHaveData
    {
        //public const string DefaultSampler = Samplers.Samplers.Nearest;
        //public const float DefaultQuality = 0.1f;
        public const decimal DefaultStepDistance = 0.2m;
        public const decimal DefaultZScale = 6.0m;
        public const decimal DefaultZBias = 0.0m;

        public ResourcePackOcclusionChannelProperties Input {get; set;}
        public string Texture {get; set;}
        public decimal? Value {get; set;}
        public decimal? Shift {get; set;}
        public decimal? Scale {get; set;}
        //public string Sampler {get; set;}
        //public decimal? Quality {get; set;}
        public decimal? ZBias {get; set;}
        public decimal? ZScale {get; set;}
        public decimal? StepDistance {get; set;}
        public bool? ClipEmissive {get; set;}


        public bool HasAnyData()
        {
            if (Input != null && Input.HasAnyData()) return true;
            if (Texture != null) return true;
            if (Value.HasValue) return true;
            if (Shift.HasValue) return true;
            if (Scale.HasValue) return true;

            if (ZBias.HasValue) return true;
            if (ZScale.HasValue) return true;
            if (StepDistance.HasValue) return true;
            if (ClipEmissive.HasValue) return true;
            return false;
        }

        #region Deprecated

        [Obsolete] public int? Steps {
            get => null;
            set {}
        }

        [Obsolete] public decimal? Quality {
            get => null;
            set {}
        }

        #endregion
    }
}
