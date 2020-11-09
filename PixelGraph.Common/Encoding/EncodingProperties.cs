using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using System;

namespace PixelGraph.Common.Encoding
{
    public class EncodingProperties : PropertiesFile
    {
        public const string Raw = "raw";
        public const string Default = "default";
        public const string Legacy = "legacy";
        public const string Lab11 = "lab-1.1";
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

        public string OcclusionInputR => Get<string>("occlusion.input.r");
        public string OcclusionInputG => Get<string>("occlusion.input.g");
        public string OcclusionInputB => Get<string>("occlusion.input.b");
        public string OcclusionInputA => Get<string>("occlusion.input.a");
        public bool OutputOcclusion => Get<bool>("output.occlusion");
        public string OutputOcclusionR => Get<string>("output.occlusion.r");
        public string OutputOcclusionG => Get<string>("output.occlusion.g");
        public string OutputOcclusionB => Get<string>("output.occlusion.b");
        public string OutputOcclusionA => Get<string>("output.occlusion.a");

        public string SpecularInputR => Get<string>("specular.input.r");
        public string SpecularInputG => Get<string>("specular.input.g");
        public string SpecularInputB => Get<string>("specular.input.b");
        public string SpecularInputA => Get<string>("specular.input.a");
        public bool OutputSpecular => Get<bool>("output.specular");
        public string OutputSpecularR => Get<string>("output.specular.r");
        public string OutputSpecularG => Get<string>("output.specular.g");
        public string OutputSpecularB => Get<string>("output.specular.b");
        public string OutputSpecularA => Get<string>("output.specular.a");

        public string SmoothInputR => Get<string>("smooth.input.r");
        public string SmoothInputG => Get<string>("smooth.input.g");
        public string SmoothInputB => Get<string>("smooth.input.b");
        public string SmoothInputA => Get<string>("smooth.input.a");
        public bool OutputSmooth => Get<bool>("output.smooth");
        public string OutputSmoothR => Get<string>("output.smooth.r");
        public string OutputSmoothG => Get<string>("output.smooth.g");
        public string OutputSmoothB => Get<string>("output.smooth.b");
        public string OutputSmoothA => Get<string>("output.smooth.a");

        public string RoughInputR => Get<string>("rough.input.r");
        public string RoughInputG => Get<string>("rough.input.g");
        public string RoughInputB => Get<string>("rough.input.b");
        public string RoughInputA => Get<string>("rough.input.a");
        public bool OutputRough => Get<bool>("output.rough");
        public string OutputRoughR => Get<string>("output.rough.r");
        public string OutputRoughG => Get<string>("output.rough.g");
        public string OutputRoughB => Get<string>("output.rough.b");
        public string OutputRoughA => Get<string>("output.rough.a");

        public string MetalInputR => Get<string>("metal.input.r");
        public string MetalInputG => Get<string>("metal.input.g");
        public string MetalInputB => Get<string>("metal.input.b");
        public string MetalInputA => Get<string>("metal.input.a");
        public bool OutputMetal => Get<bool>("output.metal");
        public string OutputMetalR => Get<string>("output.metal.r");
        public string OutputMetalG => Get<string>("output.metal.g");
        public string OutputMetalB => Get<string>("output.metal.b");
        public string OutputMetalA => Get<string>("output.metal.a");

        public string PorosityInputR => Get<string>("porosity.input.r");
        public string PorosityInputG => Get<string>("porosity.input.g");
        public string PorosityInputB => Get<string>("porosity.input.b");
        public string PorosityInputA => Get<string>("porosity.input.a");
        public bool OutputPorosity => Get<bool>("output.porosity");
        public string OutputPorosityR => Get<string>("output.porosity.r");
        public string OutputPorosityG => Get<string>("output.porosity.g");
        public string OutputPorosityB => Get<string>("output.porosity.b");
        public string OutputPorosityA => Get<string>("output.porosity.a");

        public string SubSurfaceScatteringInputR => Get<string>("sss.input.r");
        public string SubSurfaceScatteringInputG => Get<string>("sss.input.g");
        public string SubSurfaceScatteringInputB => Get<string>("sss.input.b");
        public string SubSurfaceScatteringInputA => Get<string>("sss.input.a");
        public bool OutputSubSurfaceScattering => Get<bool>("output.sss");
        public string OutputSubSurfaceScatteringR => Get<string>("output.sss.r");
        public string OutputSubSurfaceScatteringG => Get<string>("output.sss.g");
        public string OutputSubSurfaceScatteringB => Get<string>("output.sss.b");
        public string OutputSubSurfaceScatteringA => Get<string>("output.sss.a");

        public string EmissiveInputR => Get<string>("emissive.input.r");
        public string EmissiveInputG => Get<string>("emissive.input.g");
        public string EmissiveInputB => Get<string>("emissive.input.b");
        public string EmissiveInputA => Get<string>("emissive.input.a");
        public bool OutputEmissive => Get<bool>("output.emissive");
        public string OutputEmissiveR => Get<string>("output.emissive.r");
        public string OutputEmissiveG => Get<string>("output.emissive.g");
        public string OutputEmissiveB => Get<string>("output.emissive.b");
        public string OutputEmissiveA => Get<string>("output.emissive.a");


        public void Build(PackProperties pack, PbrProperties texture = null)
        {
            Properties.Clear();

            var hasPackInputFormat = !string.IsNullOrWhiteSpace(pack.InputFormat);
            var hasPackOutputFormat = !string.IsNullOrWhiteSpace(pack.OutputFormat);
            var hasTextureInputFormat = !string.IsNullOrWhiteSpace(texture?.InputFormat);

            if (hasTextureInputFormat)
                ApplyInputFormat(texture.InputFormat);
            else if (hasPackInputFormat)
                ApplyInputFormat(pack.InputFormat);

            if (hasPackOutputFormat)
                ApplyOutputFormat(pack.OutputFormat);

            if (!hasTextureInputFormat)
                Properties.Update(pack.Properties);

            if (texture != null)
                Properties.Update(texture.Properties);
        }

        private void ApplyInputFormat(string format)
        {
            if (string.Equals(Raw, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(RawEncoding.InputProperties);
                return;
            }

            if (string.Equals(Default, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(DefaultEncoding.InputProperties);
                return;
            }

            if (string.Equals(Legacy, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(LegacyEncoding.InputProperties);
                return;
            }

            if (string.Equals(Lab11, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(Lab11Encoding.InputProperties);
                return;
            }

            if (string.Equals(Lab13, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(Lab13Encoding.InputProperties);
                return;
            }

            throw new ApplicationException($"Unknown input format '{format}'!");
        }

        private void ApplyOutputFormat(string format)
        {
            if (string.Equals(Raw, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(RawEncoding.OutputProperties);
                return;
            }

            if (string.Equals(Default, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(DefaultEncoding.OutputProperties);
                return;
            }

            if (string.Equals(Legacy, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(LegacyEncoding.OutputProperties);
                return;
            }

            if (string.Equals(Lab11, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(Lab11Encoding.OutputProperties);
                return;
            }

            if (string.Equals(Lab13, format, StringComparison.InvariantCultureIgnoreCase)) {
                Properties.Update(Lab13Encoding.OutputProperties);
                return;
            }

            throw new ApplicationException($"Unknown output format '{format}'!");
        }

        public string GetInput(string textureTag, ColorChannel colorChannel)
        {
            var texture = GetTexturePart(textureTag);
            var channel = GetChannelPart(colorChannel);
            return Get<string>($"{texture}.input.{channel}");
        }

        public string GetOutput(string textureTag, ColorChannel colorChannel)
        {
            var texture = GetTexturePart(textureTag);
            var channel = GetChannelPart(colorChannel);
            return Get<string>($"output.{texture}.{channel}");
        }

        public bool GetExported(string textureTag)
        {
            var texture = GetTexturePart(textureTag);
            return Get($"output.{texture}", false);
        }

        // TODO: DUPLICATE CODE
        private static string GetTexturePart(string textureTag)
        {
            return textureTag switch {
                TextureTags.Albedo => "albedo",
                TextureTags.Height => "height",
                TextureTags.Normal => "normal",
                TextureTags.Occlusion => "occlusion",
                TextureTags.Specular => "specular",
                TextureTags.Rough => "rough",
                TextureTags.Smooth => "smooth",
                TextureTags.Metal => "metal",
                TextureTags.Porosity => "porosity",
                TextureTags.SubSurfaceScattering => "sss",
                TextureTags.Emissive => "emissive",
                // TODO: ...
                _ => null,
            };
        }

        // TODO: DUPLICATE CODE
        private static string GetChannelPart(ColorChannel colorChannel)
        {
            return colorChannel switch {
                ColorChannel.Red => "r",
                ColorChannel.Green => "g",
                ColorChannel.Blue => "b",
                ColorChannel.Alpha => "a",
                _ => null,
            };
        }
    }
}
