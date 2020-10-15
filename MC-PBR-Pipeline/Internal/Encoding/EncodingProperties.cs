using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Encoding
{
    internal class EncodingProperties : PropertiesFile
    {
        public const string Default = "default";
        public const string Legacy = "legacy";
        public const string Lab13 = "lab-1.3";


        public string AlbedoInputR => Get<string>("albedo.input.r");
        public string AlbedoInputG => Get<string>("albedo.input.g");
        public string AlbedoInputB => Get<string>("albedo.input.b");
        public string AlbedoInputA => Get<string>("albedo.input.a");
        public bool OutputAlbedo => Get<bool>("output.albedo");
        public string OutputAlbedoR => Get<string>("output.albedo.r");
        public string OutputAlbedoG => Get<string>("output.albedo.g");
        public string OutputAlbedoB => Get<string>("output.albedo.b");
        public string OutputAlbedoA => Get<string>("output.albedo.a");

        public string HeightInputR => Get<string>("height.input.r");
        public string HeightInputG => Get<string>("height.input.g");
        public string HeightInputB => Get<string>("height.input.b");
        public string HeightInputA => Get<string>("height.input.a");
        public bool OutputHeight => Get<bool>("output.height");
        public string OutputHeightR => Get<string>("output.height.r");
        public string OutputHeightG => Get<string>("output.height.g");
        public string OutputHeightB => Get<string>("output.height.b");
        public string OutputHeightA => Get<string>("output.height.a");

        public string NormalInputR => Get<string>("normal.input.r");
        public string NormalInputG => Get<string>("normal.input.g");
        public string NormalInputB => Get<string>("normal.input.b");
        public string NormalInputA => Get<string>("normal.input.a");
        public bool OutputNormal => Get<bool>("output.normal");
        public string OutputNormalR => Get<string>("output.normal.r");
        public string OutputNormalG => Get<string>("output.normal.g");
        public string OutputNormalB => Get<string>("output.normal.b");
        public string OutputNormalA => Get<string>("output.normal.a");

        public string SpecularInputR => Get<string>("specular.input.r");
        public string SpecularInputG => Get<string>("specular.input.g");
        public string SpecularInputB => Get<string>("specular.input.b");
        public string SpecularInputA => Get<string>("specular.input.a");
        public bool OutputSpecular => Get<bool>("output.specular");
        public string OutputSpecularR => Get<string>("output.specular.r");
        public string OutputSpecularG => Get<string>("output.specular.g");
        public string OutputSpecularB => Get<string>("output.specular.b");
        public string OutputSpecularA => Get<string>("output.specular.a");

        public string EmissiveInputR => Get<string>("emissive.input.r");
        public string EmissiveInputG => Get<string>("emissive.input.g");
        public string EmissiveInputB => Get<string>("emissive.input.b");
        public string EmissiveInputA => Get<string>("emissive.input.a");
        public bool OutputEmissive => Get<bool>("output.emissive");
        public string OutputEmissiveR => Get<string>("output.emissive.r");
        public string OutputEmissiveG => Get<string>("output.emissive.g");
        public string OutputEmissiveB => Get<string>("output.emissive.b");
        public string OutputEmissiveA => Get<string>("output.emissive.a");

        public string OcclusionInputR => Get<string>("occlusion.input.r");
        public string OcclusionInputG => Get<string>("occlusion.input.g");
        public string OcclusionInputB => Get<string>("occlusion.input.b");
        public string OcclusionInputA => Get<string>("occlusion.input.a");
        public bool OutputOcclusion => Get<bool>("output.occlusion");
        public string OutputOcclusionR => Get<string>("output.occlusion.r");
        public string OutputOcclusionG => Get<string>("output.occlusion.g");
        public string OutputOcclusionB => Get<string>("output.occlusion.b");
        public string OutputOcclusionA => Get<string>("output.occlusion.a");

        public string SmoothInputR => Get<string>("smooth.input.r");
        public string SmoothInputG => Get<string>("smooth.input.g");
        public string SmoothInputB => Get<string>("smooth.input.b");
        public string SmoothInputA => Get<string>("smooth.input.a");
        public bool OutputSmooth => Get<bool>("output.smooth");
        public string OutputSmoothR => Get<string>("output.smooth.r");
        public string OutputSmoothG => Get<string>("output.smooth.g");
        public string OutputSmoothB => Get<string>("output.smooth.b");
        public string OutputSmoothA => Get<string>("output.smooth.a");

        public string MetalInputR => Get<string>("metal.input.r");
        public string MetalInputG => Get<string>("metal.input.g");
        public string MetalInputB => Get<string>("metal.input.b");
        public string MetalInputA => Get<string>("metal.input.a");
        public bool OutputMetal => Get<bool>("output.metal");
        public string OutputMetalR => Get<string>("output.metal.r");
        public string OutputMetalG => Get<string>("output.metal.g");
        public string OutputMetalB => Get<string>("output.metal.b");
        public string OutputMetalA => Get<string>("output.metal.a");


        public void Build(PackProperties pack, PbrProperties texture)
        {
            var hasPackInputFormat = !string.IsNullOrWhiteSpace(pack.InputFormat);
            var hasPackOutputFormat = !string.IsNullOrWhiteSpace(pack.OutputFormat);
            var hasTextureInputFormat = !string.IsNullOrWhiteSpace(texture.InputFormat);

            if (hasTextureInputFormat)
                ApplyInputFormat(texture.InputFormat);
            else if (hasPackInputFormat)
                ApplyInputFormat(pack.InputFormat);

            if (hasPackOutputFormat)
                ApplyOutputFormat(pack.OutputFormat);

            if (!hasTextureInputFormat)
                Properties.Update(pack.Properties);

            Properties.Update(texture.Properties);
        }

        private void ApplyInputFormat(string format)
        {
            if (string.Equals(Default, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(new Dictionary<string, string> {
                    ["albedo.input.r"] = EncodingChannel.AlbedoR,
                    ["albedo.input.g"] = EncodingChannel.AlbedoG,
                    ["albedo.input.b"] = EncodingChannel.AlbedoB,
                    ["albedo.input.a"] = EncodingChannel.AlbedoA,

                    ["height.input.r"] = EncodingChannel.Height,

                    ["normal.input.r"] = EncodingChannel.NormalX,
                    ["normal.input.g"] = EncodingChannel.NormalY,
                    ["normal.input.b"] = EncodingChannel.NormalZ,

                    ["smooth.input.r"] = EncodingChannel.Smooth,

                    ["metal.input.r"] = EncodingChannel.Metal,

                    ["emissive.input.r"] = EncodingChannel.Emissive,

                    ["occlusion.input.r"] = EncodingChannel.Occlusion,
                });
                return;
            }

            if (string.Equals(Legacy, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(new Dictionary<string, string> {
                    ["albedo.input.r"] = EncodingChannel.AlbedoR,
                    ["albedo.input.g"] = EncodingChannel.AlbedoG,
                    ["albedo.input.b"] = EncodingChannel.AlbedoB,
                    ["albedo.input.a"] = EncodingChannel.AlbedoA,

                    ["normal.input.r"] = EncodingChannel.NormalX,
                    ["normal.input.g"] = EncodingChannel.NormalY,
                    ["normal.input.b"] = EncodingChannel.NormalZ,
                    ["normal.input.a"] = EncodingChannel.Height,

                    ["specular.input.r"] = EncodingChannel.Smooth,
                    ["specular.input.g"] = EncodingChannel.Metal,
                    ["specular.input.b"] = EncodingChannel.Emissive,
                });
                return;
            }

            if (string.Equals(Lab13, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(new Dictionary<string, string> {
                    ["albedo.input.r"] = EncodingChannel.AlbedoR,
                    ["albedo.input.g"] = EncodingChannel.AlbedoG,
                    ["albedo.input.b"] = EncodingChannel.AlbedoB,
                    ["albedo.input.a"] = EncodingChannel.AlbedoA,

                    ["normal.input.r"] = EncodingChannel.NormalX,
                    ["normal.input.g"] = EncodingChannel.NormalY,
                    ["normal.input.b"] = EncodingChannel.Occlusion,
                    ["normal.input.a"] = EncodingChannel.Height,

                    ["specular.input.r"] = EncodingChannel.PerceptualSmooth,
                    ["specular.input.g"] = EncodingChannel.Metal,
                    ["specular.input.b"] = EncodingChannel.Porosity_SSS,
                    ["specular.input.a"] = EncodingChannel.EmissiveClipped,
                });
                return;
            }

            throw new ApplicationException($"Unknown input format '{format}'!");
        }

        private void ApplyOutputFormat(string format)
        {
            if (string.Equals(Default, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(new Dictionary<string, string> {
                    ["output.albedo"] = bool.TrueString,
                    ["output.albedo.r"] = EncodingChannel.AlbedoR,
                    ["output.albedo.g"] = EncodingChannel.AlbedoG,
                    ["output.albedo.b"] = EncodingChannel.AlbedoB,
                    ["output.albedo.a"] = EncodingChannel.AlbedoA,

                    ["output.height"] = bool.TrueString,
                    ["output.height.r"] = EncodingChannel.Height,
                    ["output.height.a"] = EncodingChannel.White,

                    ["output.normal"] = bool.TrueString,
                    ["output.normal.r"] = EncodingChannel.NormalX,
                    ["output.normal.g"] = EncodingChannel.NormalY,
                    ["output.normal.b"] = EncodingChannel.NormalZ,
                    ["output.normal.a"] = EncodingChannel.White,

                    ["output.specular"] = bool.TrueString,
                    //["output.specular.r"] = EncodingChannel.Smooth,
                    //["output.specular.g"] = EncodingChannel.Metal,
                    ["output.specular.b"] = EncodingChannel.Porosity_SSS,
                    ["output.specular.a"] = EncodingChannel.White,

                    ["output.smooth"] = bool.TrueString,
                    ["output.smooth.r"] = EncodingChannel.Smooth,
                    ["output.smooth.a"] = EncodingChannel.White,

                    ["output.metal"] = bool.TrueString,
                    ["output.metal.r"] = EncodingChannel.Metal,
                    ["output.metal.a"] = EncodingChannel.White,

                    ["output.emissive"] = bool.TrueString,
                    ["output.emissive.r"] = EncodingChannel.Emissive,
                    ["output.emissive.a"] = EncodingChannel.White,

                    ["output.occlusion"] = bool.TrueString,
                    ["output.occlusion.r"] = EncodingChannel.Occlusion,
                    ["output.occlusion.a"] = EncodingChannel.White,
                });
                return;
            }

            if (string.Equals(Legacy, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(new Dictionary<string, string> {
                    ["output.albedo"] = bool.TrueString,
                    ["output.albedo.r"] = EncodingChannel.AlbedoR,
                    ["output.albedo.g"] = EncodingChannel.AlbedoG,
                    ["output.albedo.b"] = EncodingChannel.AlbedoB,
                    ["output.albedo.a"] = EncodingChannel.AlbedoA,

                    ["output.normal"] = bool.TrueString,
                    ["output.normal.r"] = EncodingChannel.NormalX,
                    ["output.normal.g"] = EncodingChannel.NormalY,
                    ["output.normal.b"] = EncodingChannel.NormalZ,
                    ["output.normal.a"] = EncodingChannel.Height,

                    ["output.specular"] = bool.TrueString,
                    ["output.specular.r"] = EncodingChannel.Smooth,
                    ["output.specular.g"] = EncodingChannel.Metal,
                    ["output.specular.b"] = EncodingChannel.Emissive,
                });
                return;
            }

            if (string.Equals(Lab13, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(new Dictionary<string, string> {
                    ["output.albedo"] = bool.TrueString,
                    ["output.albedo.r"] = EncodingChannel.AlbedoR,
                    ["output.albedo.g"] = EncodingChannel.AlbedoG,
                    ["output.albedo.b"] = EncodingChannel.AlbedoB,
                    ["output.albedo.a"] = EncodingChannel.AlbedoA,

                    ["output.normal"] = bool.TrueString,
                    ["output.normal.r"] = EncodingChannel.NormalX,
                    ["output.normal.g"] = EncodingChannel.NormalY,
                    ["output.normal.b"] = EncodingChannel.Occlusion,
                    ["output.normal.a"] = EncodingChannel.Height,

                    ["output.specular"] = bool.TrueString,
                    ["output.specular.r"] = EncodingChannel.PerceptualSmooth,
                    ["output.specular.g"] = EncodingChannel.Metal,
                    ["output.specular.b"] = EncodingChannel.Porosity_SSS,
                    ["output.specular.a"] = EncodingChannel.EmissiveClipped,
                });
                return;
            }

            throw new ApplicationException($"Unknown output format '{format}'!");
        }
    }
}
