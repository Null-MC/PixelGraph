using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Textures
{
    internal class TextureFilter
    {
        private readonly PbrProperties texture;


        public TextureFilter(PbrProperties texture)
        {
            this.texture = texture;
        }

        public void GenericScaleFilter(ref byte value, in float scale) =>
            value = MathEx.Saturate(value / 255d * scale);

        public void SmoothToPerceptualSmooth(ref byte value) =>
            value = MathEx.Saturate(Math.Sqrt(value / 255d));

        public void PerceptualSmoothToSmooth(ref byte value) =>
            value = MathEx.Saturate(Math.Pow(value / 255d, 2));

        public void EmissiveToEmissiveClipped(ref byte value) => value = (byte)(value - 1);

        public void EmissiveClippedToEmissive(ref byte value) => value = (byte)(value + 1);

        public void Invert(ref byte value) => value = (byte)(255 - value);

        public bool TryGetChannelValue(string encodingChannel, out byte value)
        {
            byte? result = null;

            if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                result = valueFunc(texture);
                value = result ?? 0;
            }
            else value = 0;

            return result.HasValue;
        }

        public float GetScale(string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 1f;
            return scaleMap.TryGetValue(channel, out var value) ? value(texture) : 1f;
        }

        private static readonly Dictionary<string, Func<PbrProperties, byte?>> valueMap = new Dictionary<string, Func<PbrProperties, byte?>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.AlbedoR] = tex => tex.AlbedoValueR,
            [EncodingChannel.AlbedoG] = tex => tex.AlbedoValueG,
            [EncodingChannel.AlbedoB] = tex => tex.AlbedoValueB,
            [EncodingChannel.AlbedoA] = tex => tex.AlbedoValueA,
            [EncodingChannel.Height] = tex => tex.HeightValue,
            [EncodingChannel.NormalX] = tex => tex.NormalValueX,
            [EncodingChannel.NormalY] = tex => tex.NormalValueY,
            [EncodingChannel.NormalZ] = tex => tex.NormalValueZ,
            [EncodingChannel.Occlusion] = tex => tex.OcclusionValue,
            [EncodingChannel.Smooth] = tex => tex.SmoothValue,
            [EncodingChannel.PerceptualSmooth] = tex => tex.SmoothValue,
            [EncodingChannel.Rough] = tex => tex.RoughValue,
            [EncodingChannel.Metal] = tex => tex.MetalValue,
            [EncodingChannel.Emissive] = tex => tex.EmissiveValue,
            [EncodingChannel.EmissiveClipped] = tex => tex.EmissiveValue,
            [EncodingChannel.EmissiveInverse] = tex => tex.EmissiveValue,
            [EncodingChannel.Porosity] = tex => tex.PorosityValue,
        };

        private static readonly Dictionary<string, Func<PbrProperties, float>> scaleMap = new Dictionary<string, Func<PbrProperties, float>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.AlbedoR] = t => t.AlbedoScaleR,
            [EncodingChannel.AlbedoG] = t => t.AlbedoScaleG,
            [EncodingChannel.AlbedoB] = t => t.AlbedoScaleB,
            [EncodingChannel.AlbedoA] = t => t.AlbedoScaleA,
            [EncodingChannel.Height] = t => t.HeightScale,
            [EncodingChannel.Occlusion] = t => t.OcclusionScale,
            [EncodingChannel.Smooth] = t => t.SmoothScale,
            [EncodingChannel.PerceptualSmooth] = t => t.SmoothScale,
            [EncodingChannel.Rough] = t => t.RoughScale,
            [EncodingChannel.Metal] = t => t.MetalScale,
            [EncodingChannel.Emissive] = t => t.EmissiveScale,
            [EncodingChannel.EmissiveClipped] = t => t.EmissiveScale,
            [EncodingChannel.EmissiveInverse] = t => t.EmissiveScale,
            // Porosity-SSS
        };
    }
}
