using System.Collections.Generic;

namespace PixelGraph.Common.Encoding
{
    internal static class Lab11Encoding
    {
        public static Dictionary<string, string> InputProperties = new Dictionary<string, string> {
            ["albedo.input.r"] = EncodingChannel.Red,
            ["albedo.input.g"] = EncodingChannel.Green,
            ["albedo.input.b"] = EncodingChannel.Blue,
            ["albedo.input.a"] = EncodingChannel.Alpha,

            ["normal.input.r"] = EncodingChannel.NormalX,
            ["normal.input.g"] = EncodingChannel.NormalY,
            ["normal.input.b"] = EncodingChannel.NormalZ,
            ["normal.input.a"] = EncodingChannel.Height,

            ["specular.input.r"] = EncodingChannel.PerceptualSmooth,
            ["specular.input.g"] = EncodingChannel.Metal,
            ["specular.input.b"] = EncodingChannel.Emissive,
        };

        public static Dictionary<string, string> OutputProperties = new Dictionary<string, string> {
            ["output.albedo"] = bool.TrueString,
            ["output.albedo.r"] = EncodingChannel.Red,
            ["output.albedo.g"] = EncodingChannel.Green,
            ["output.albedo.b"] = EncodingChannel.Blue,
            ["output.albedo.a"] = EncodingChannel.Alpha,

            ["output.normal"] = bool.TrueString,
            ["output.normal.r"] = EncodingChannel.NormalX,
            ["output.normal.g"] = EncodingChannel.NormalY,
            ["output.normal.b"] = EncodingChannel.NormalZ,
            ["output.normal.a"] = EncodingChannel.Height,

            ["output.specular"] = bool.TrueString,
            ["output.specular.r"] = EncodingChannel.Smooth,
            ["output.specular.g"] = EncodingChannel.Metal,
            ["output.specular.b"] = EncodingChannel.Emissive,
        };
    }
}
