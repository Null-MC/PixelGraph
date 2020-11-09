using System.Collections.Generic;

namespace PixelGraph.Common.Encoding
{
    internal static class RawEncoding
    {
        public static Dictionary<string, string> InputProperties = new Dictionary<string, string> {
            ["albedo.input.r"] = EncodingChannel.Red,
            ["albedo.input.g"] = EncodingChannel.Green,
            ["albedo.input.b"] = EncodingChannel.Blue,
            ["albedo.input.a"] = EncodingChannel.Alpha,

            ["height.input.r"] = EncodingChannel.Height,

            ["normal.input.r"] = EncodingChannel.NormalX,
            ["normal.input.g"] = EncodingChannel.NormalY,
            ["normal.input.b"] = EncodingChannel.NormalZ,

            ["occlusion.input.r"] = EncodingChannel.Occlusion,

            ["specular.input.r"] = EncodingChannel.Specular,

            ["rough.input.r"] = EncodingChannel.Rough,

            ["smooth.input.r"] = EncodingChannel.Smooth,

            ["metal.input.r"] = EncodingChannel.Metal,

            ["porosity.input.r"] = EncodingChannel.Porosity,

            ["sss.input.r"] = EncodingChannel.SubSurfaceScattering,

            ["emissive.input.r"] = EncodingChannel.Emissive,
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

            ["output.occlusion"] = bool.TrueString,
            ["output.occlusion.r"] = EncodingChannel.Occlusion,
            ["output.occlusion.a"] = EncodingChannel.White,

            ["output.specular"] = bool.TrueString,
            ["output.specular.r"] = EncodingChannel.Specular,
            ["output.specular.a"] = EncodingChannel.White,

            ["output.rough"] = bool.TrueString,
            ["output.rough.r"] = EncodingChannel.Rough,
            ["output.rough.a"] = EncodingChannel.White,

            ["output.smooth"] = bool.TrueString,
            ["output.smooth.r"] = EncodingChannel.Smooth,
            ["output.smooth.a"] = EncodingChannel.White,

            ["output.metal"] = bool.TrueString,
            ["output.metal.r"] = EncodingChannel.Metal,
            ["output.metal.a"] = EncodingChannel.White,

            ["output.porosity"] = bool.TrueString,
            ["output.porosity.r"] = EncodingChannel.Porosity,
            ["output.porosity.a"] = EncodingChannel.White,

            ["output.sss"] = bool.TrueString,
            ["output.sss.r"] = EncodingChannel.SubSurfaceScattering,
            ["output.sss.a"] = EncodingChannel.White,

            ["output.emissive"] = bool.TrueString,
            ["output.emissive.r"] = EncodingChannel.Emissive,
            ["output.emissive.a"] = EncodingChannel.White,
        };
    }
}
