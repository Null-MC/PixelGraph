using McPbrPipeline.Internal.Encoding;
using McPbrPipeline.Internal.Textures;
using System;

namespace McPbrPipeline.Internal.Input
{
    internal class PackProperties : PropertiesFile
    {
        public string Source {get; set;}
        public DateTime WriteTime {get; set;}

        public int PackFormat => Get<int>("pack.format");
        public string PackDescription => Get<string>("pack.description");
        public string PackTags => Get<string>("pack.tags");

        public string InputFormat {
            get => Get<string>(PackProperty.InputFormat);
            set => Properties[PackProperty.InputFormat] = value;
        }

        public string OutputFormat {
            get => Get<string>(PackProperty.OutputFormat);
            set => Properties[PackProperty.OutputFormat] = value;
        }

        public string Sampler => Get<string>("sampler");
        public int? TextureSize => Get<int?>("texture.size");
        public float? TextureScale => Get<float?>("texture.scale");

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

        public TextureEncoding AlbedoOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Albedo);
        public TextureEncoding HeightOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Height);
        public TextureEncoding NormalOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Normal);
        public TextureEncoding SpecularOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Specular);
        public TextureEncoding EmissiveOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Emissive);
        public TextureEncoding OcclusionOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Occlusion);


        public void ApplyFormat()
        {
            if (!string.IsNullOrWhiteSpace(InputFormat))
                EncodingFormat.ApplyInputFormat(Properties, InputFormat);

            if (!string.IsNullOrWhiteSpace(OutputFormat))
                EncodingFormat.ApplyOutputFormat(Properties, OutputFormat);
        }
    }

    internal static class PackProperty
    {
        public const string InputFormat = "input.format";
        public const string OutputFormat = "output.format";
    }
}
