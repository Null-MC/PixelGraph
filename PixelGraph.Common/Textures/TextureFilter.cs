using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Textures
{
    internal class TextureFilter
    {
        private readonly MaterialProperties material;


        public TextureFilter(MaterialProperties material)
        {
            this.material = material;
        }

        public void GenericScaleFilter(ref byte value, in float scale) =>
            value = MathEx.Saturate(value / 255d * scale);

        public void SmoothToPerceptualSmooth(ref byte value) =>
            value = MathEx.Saturate(Math.Sqrt(value / 255d));

        public void PerceptualSmoothToSmooth(ref byte value) =>
            value = MathEx.Saturate(Math.Pow(value / 255d, 2));

        public void EmissiveToEmissiveClipped(ref byte value) => value = (byte)(value - 1);

        public void EmissiveClippedToEmissive(ref byte value) => value = (byte)(value + 1);

        public void PorosityToPSSS(ref byte value) => value = MathEx.Clamp(value * 0.25f);

        public void SSSToPSSS(ref byte value) => value = MathEx.Clamp(65f + value * (190f / 255f));

        public void PSSSToSSS(ref byte value) => value = MathEx.Clamp((value - 65) * (255f / 190f));

        public void Invert(ref byte value) => value = (byte)(255 - value);

        public bool TryGetChannelValue(string encodingChannel, out byte value)
        {
            byte? result = null;

            if (valueMap.TryGetValue(encodingChannel, out var valueFunc)) {
                result = valueFunc(material);
                value = result ?? 0;
            }
            else value = 0;

            return result.HasValue;
        }

        public float GetScale(string channel)
        {
            if (EncodingChannel.IsEmpty(channel)) return 1f;
            return scaleMap.TryGetValue(channel, out var value) ? (float)value(material) : 1f;
        }

        private static readonly Dictionary<string, Func<MaterialProperties, byte?>> valueMap = new Dictionary<string, Func<MaterialProperties, byte?>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.Red] = mat => mat.Albedo?.ValueRed,
            [EncodingChannel.Green] = mat => mat.Albedo?.ValueGreen,
            [EncodingChannel.Blue] = mat => mat.Albedo?.ValueBlue,
            [EncodingChannel.Alpha] = mat => mat.Albedo?.ValueAlpha,
            [EncodingChannel.Height] = mat => mat.Height?.Value,
            [EncodingChannel.NormalX] = mat => mat.Normal?.ValueX,
            [EncodingChannel.NormalY] = mat => mat.Normal?.ValueY,
            [EncodingChannel.NormalZ] = mat => mat.Normal?.ValueZ,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Value,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Value,
            [EncodingChannel.PerceptualSmooth] = mat => mat.Smooth?.Value,
            [EncodingChannel.Rough] = mat => mat.Rough?.Value,
            [EncodingChannel.Metal] = mat => mat.Metal?.Value,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Value,
            [EncodingChannel.EmissiveClipped] = mat => mat.Emissive?.Value,
            [EncodingChannel.EmissiveInverse] = mat => mat.Emissive?.Value,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Value,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Value,
            //[EncodingChannel.Porosity_SSS] = mat => mat.PorosityValue ?? mat.SssValue,
        };

        private static readonly Dictionary<string, Func<MaterialProperties, decimal>> scaleMap = new Dictionary<string, Func<MaterialProperties, decimal>>(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.Red] = mat => mat.Albedo?.ScaleRed ?? 1m,
            [EncodingChannel.Green] = mat => mat.Albedo?.ScaleGreen ?? 1m,
            [EncodingChannel.Blue] = mat => mat.Albedo?.ScaleBlue ?? 1m,
            [EncodingChannel.Alpha] = mat => mat.Albedo?.ScaleAlpha ?? 1m,
            [EncodingChannel.Height] = mat => mat.Height?.Scale ?? 1m,
            [EncodingChannel.Occlusion] = mat => mat.Occlusion?.Scale ?? 1m,
            [EncodingChannel.Smooth] = mat => mat.Smooth?.Scale ?? 1m,
            [EncodingChannel.PerceptualSmooth] = mat => mat.Smooth?.Scale ?? 1m,
            [EncodingChannel.Rough] = mat => mat.Rough?.Scale ?? 1m,
            [EncodingChannel.Metal] = mat => mat.Metal?.Scale ?? 1m,
            [EncodingChannel.Emissive] = mat => mat.Emissive?.Scale ?? 1m,
            [EncodingChannel.EmissiveClipped] = mat => mat.Emissive?.Scale ?? 1m,
            [EncodingChannel.EmissiveInverse] = mat => mat.Emissive?.Scale ?? 1m,
            [EncodingChannel.Porosity] = mat => mat.Porosity?.Scale ?? 1m,
            [EncodingChannel.SubSurfaceScattering] = mat => mat.SSS?.Scale ?? 1m,
            [EncodingChannel.Porosity_SSS] = mat => mat.Porosity?.Scale ?? mat.SSS?.Scale ?? 1m,
        };
    }
}
