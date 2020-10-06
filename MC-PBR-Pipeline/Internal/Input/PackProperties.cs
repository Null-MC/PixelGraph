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

        public string InputFormat => Get<string>("input-format");
        public string OutputFormat => Get<string>("output-format");
        public string Sampler => Get<string>("sampler");
        public int? TextureSize => Get<int?>("texture.size");
        public float? TextureScale => Get<float?>("texture.scale");

        public bool AlbedoIncluded => Get("albedo.included", true);
        public string AlbedoInputR => Get("albedo.input.r", EncodingChannel.AlbedoR);
        public string AlbedoInputG => Get("albedo.input.g", EncodingChannel.AlbedoG);
        public string AlbedoInputB => Get("albedo.input.b", EncodingChannel.AlbedoB);
        public string AlbedoInputA => Get("albedo.input.a", EncodingChannel.AlbedoA);
        public string AlbedoOutputR => Get("albedo.output.r", EncodingChannel.AlbedoR);
        public string AlbedoOutputG => Get("albedo.output.g", EncodingChannel.AlbedoG);
        public string AlbedoOutputB => Get("albedo.output.b", EncodingChannel.AlbedoB);
        public string AlbedoOutputA => Get("albedo.output.a", EncodingChannel.AlbedoA);

        public bool NormalIncluded => Get("normal.included", true);
        public string NormalInputR => Get("normal.input.r", EncodingChannel.NormalX);
        public string NormalInputG => Get("normal.input.g", EncodingChannel.NormalY);
        public string NormalInputB => Get("normal.input.b", EncodingChannel.NormalZ);
        public string NormalInputA => Get<string>("normal.input.a");
        public string NormalOutputR => Get("normal.output.r", EncodingChannel.NormalX);
        public string NormalOutputG => Get("normal.output.g", EncodingChannel.NormalY);
        public string NormalOutputB => Get("normal.output.b", EncodingChannel.NormalZ);
        public string NormalOutputA => Get("normal.output.a", EncodingChannel.White);

        public bool SpecularIncluded => Get("specular.included", true);
        public string SpecularInputR => Get("specular.input.r", EncodingChannel.Smooth);
        public string SpecularInputG => Get<string>("specular.input.g");
        public string SpecularInputB => Get<string>("specular.input.b");
        public string SpecularInputA => Get<string>("specular.input.a");
        public string SpecularOutputR => Get("specular.output.r", EncodingChannel.Smooth);
        public string SpecularOutputG => Get<string>("specular.output.g");
        public string SpecularOutputB => Get<string>("specular.output.b");
        public string SpecularOutputA => Get("specular.output.a", EncodingChannel.White);

        public bool EmissiveIncluded => Get("emissive.included", true);
        public string EmissiveInputR => Get<string>("emissive.input.r");
        public string EmissiveInputG => Get<string>("emissive.input.g");
        public string EmissiveInputB => Get<string>("emissive.input.b");
        public string EmissiveInputA => Get<string>("emissive.input.a");
        public string EmissiveOutputR => Get<string>("emissive.output.r");
        public string EmissiveOutputG => Get<string>("emissive.output.g");
        public string EmissiveOutputB => Get<string>("emissive.output.b");
        public string EmissiveOutputA => Get<string>("emissive.output.a");


        public TextureEncoding AlbedoOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Albedo);
        public TextureEncoding NormalOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Normal);
        public TextureEncoding SpecularOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Specular);
        public TextureEncoding EmissiveOutputEncoding => TextureEncoding.CreateOutput(this, TextureTags.Emissive);

        public void ApplyInputFormat()
        {
            if (string.Equals(MaterialFormat.Legacy, InputFormat, StringComparison.InvariantCultureIgnoreCase)) {
                SetIfEmpty("albedo.input.r", EncodingChannel.AlbedoR);
                SetIfEmpty("albedo.input.g", EncodingChannel.AlbedoG);
                SetIfEmpty("albedo.input.b", EncodingChannel.AlbedoB);
                SetIfEmpty("albedo.input.a", EncodingChannel.AlbedoA);

                SetIfEmpty("normal.input.r", EncodingChannel.NormalX);
                SetIfEmpty("normal.input.g", EncodingChannel.NormalY);
                SetIfEmpty("normal.input.b", EncodingChannel.NormalZ);
                SetIfEmpty("normal.input.a", EncodingChannel.Height);

                SetIfEmpty("specular.input.r", EncodingChannel.Smooth);
                SetIfEmpty("specular.input.g", EncodingChannel.Reflect);
                SetIfEmpty("specular.input.b", EncodingChannel.Emissive);
                return;
            }

            if (string.Equals(MaterialFormat.Lab13, InputFormat, StringComparison.InvariantCultureIgnoreCase)) {
                SetIfEmpty("albedo.input.r", EncodingChannel.AlbedoR);
                SetIfEmpty("albedo.input.g", EncodingChannel.AlbedoG);
                SetIfEmpty("albedo.input.b", EncodingChannel.AlbedoB);
                SetIfEmpty("albedo.input.a", EncodingChannel.AlbedoA);

                SetIfEmpty("normal.input.r", EncodingChannel.NormalX);
                SetIfEmpty("normal.input.g", EncodingChannel.NormalY);
                SetIfEmpty("normal.input.b", EncodingChannel.AmbientOcclusion);
                SetIfEmpty("normal.input.a", EncodingChannel.Height);

                SetIfEmpty("specular.input.r", EncodingChannel.PerceptualSmooth);
                SetIfEmpty("specular.input.g", EncodingChannel.Reflect);
                SetIfEmpty("specular.input.b", EncodingChannel.Porosity_SSS);
                SetIfEmpty("specular.input.a", EncodingChannel.Emissive);
                return;
            }

            if (string.IsNullOrEmpty(InputFormat)) {
                
                return;
            }

            throw new ApplicationException($"Unknown input format '{InputFormat}'!");
        }

        private void SetIfEmpty(string key, string value)
        {
            if (!Properties.ContainsKey(key)) Properties[key] = value;
        }

        public void ApplyOutputFormat()
        {
            if (string.IsNullOrEmpty(OutputFormat)) return;

            if (string.Equals(MaterialFormat.Legacy, OutputFormat, StringComparison.InvariantCultureIgnoreCase)) {
                SetIfEmpty("albedo.output.r", EncodingChannel.AlbedoR);
                SetIfEmpty("albedo.output.g", EncodingChannel.AlbedoG);
                SetIfEmpty("albedo.output.b", EncodingChannel.AlbedoB);
                SetIfEmpty("albedo.output.a", EncodingChannel.AlbedoA);

                SetIfEmpty("normal.output.r", EncodingChannel.NormalX);
                SetIfEmpty("normal.output.g", EncodingChannel.NormalY);
                SetIfEmpty("normal.output.b", EncodingChannel.NormalZ);
                SetIfEmpty("normal.output.a", EncodingChannel.Height);

                SetIfEmpty("specular.output.r", EncodingChannel.Smooth);
                SetIfEmpty("specular.output.g", EncodingChannel.Reflect);
                SetIfEmpty("specular.output.b", EncodingChannel.Emissive);
                return;
            }

            if (string.Equals(MaterialFormat.Lab13, OutputFormat, StringComparison.InvariantCultureIgnoreCase)) {
                SetIfEmpty("albedo.output.r", EncodingChannel.AlbedoR);
                SetIfEmpty("albedo.output.g", EncodingChannel.AlbedoG);
                SetIfEmpty("albedo.output.b", EncodingChannel.AlbedoB);
                SetIfEmpty("albedo.output.a", EncodingChannel.AlbedoA);

                SetIfEmpty("normal.output.r", EncodingChannel.NormalX);
                SetIfEmpty("normal.output.g", EncodingChannel.NormalY);
                SetIfEmpty("normal.output.b", EncodingChannel.AmbientOcclusion);
                SetIfEmpty("normal.output.a", EncodingChannel.Height);

                SetIfEmpty("specular.output.r", EncodingChannel.PerceptualSmooth);
                SetIfEmpty("specular.output.g", EncodingChannel.Reflect);
                SetIfEmpty("specular.output.b", EncodingChannel.Porosity_SSS);
                SetIfEmpty("specular.output.a", EncodingChannel.Emissive);
                return;
            }

            throw new ApplicationException($"Unknown output format '{OutputFormat}'!");
        }
    }
}
