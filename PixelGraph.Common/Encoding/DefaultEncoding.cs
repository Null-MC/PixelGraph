using System.Collections.Generic;

namespace PixelGraph.Common.Encoding
{
    internal static class DefaultEncoding
    {
        public static Dictionary<string, string> InputProperties = new Dictionary<string, string> {
            ["albedo.input.r"] = EncodingChannel.Red,
            ["albedo.input.g"] = EncodingChannel.Green,
            ["albedo.input.b"] = EncodingChannel.Blue,
            ["albedo.input.a"] = EncodingChannel.Alpha,

            ["normal.input.r"] = EncodingChannel.NormalX,
            ["normal.input.g"] = EncodingChannel.NormalY,
            ["normal.input.b"] = EncodingChannel.NormalZ,

            ["specular.input.r"] = EncodingChannel.Specular,

            //["emissive.input.a"] = EncodingChannel.Emissive,
        };

        public static Dictionary<string, string> OutputProperties = new Dictionary<string, string> {
            ["output.albedo"] = bool.TrueString,
            ["output.albedo.r"] = EncodingChannel.Red,
            ["output.albedo.g"] = EncodingChannel.Green,
            ["output.albedo.b"] = EncodingChannel.Blue,
            ["output.albedo.a"] = EncodingChannel.Alpha,

            ["output.height"] = bool.TrueString,
            ["output.height.r"] = EncodingChannel.Height,
            ["output.height.a"] = EncodingChannel.White,

            ["output.normal"] = bool.TrueString,
            ["output.normal.r"] = EncodingChannel.NormalX,
            ["output.normal.g"] = EncodingChannel.NormalY,
            ["output.normal.b"] = EncodingChannel.NormalZ,
            ["output.normal.a"] = EncodingChannel.White,

            ["output.specular"] = bool.TrueString,
            ["output.specular.r"] = EncodingChannel.Specular,
            ["output.specular.a"] = EncodingChannel.White,

            //["output.emissive"] = bool.TrueString,
            //["output.emissive.a"] = EncodingChannel.Emissive,
        };
    }
}
